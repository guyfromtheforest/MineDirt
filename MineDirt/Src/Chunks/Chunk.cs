using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MineDirt;
using MineDirt.Src;
using MineDirt.Src.Chunks;
using MineDirt.Src.Noise;
using System;
using System.Collections.Generic;
using System.Drawing;

public class Chunk
{
    public static ushort Width { get; private set; } = 16;
    public static int Height { get; private set; } = 8 * Width; // Max chunk Height
    public Vector3 Position { get; private set; }
    public Block[] Blocks { get; private set; }
    public ushort BlockCount;
    public VertexBuffer VertexBuffer { get; private set; }
    public VertexBuffer TransparentVertexBuffer { get; private set; }
    public IndexBuffer IndexBuffer { get; private set; }
    public IndexBuffer TransparentIndexBuffer { get; private set; }
    public int VertexCount => VertexBuffer?.VertexCount ?? 0;
    public int IndexCount => IndexBuffer?.IndexCount ?? 0;
    public int TransparentVertexCount => TransparentVertexBuffer?.VertexCount ?? 0;
    public int TransparentIndexCount => TransparentIndexBuffer?.IndexCount ?? 0;

    public bool HasGeneratedTerrain { get; private set; } = false;

    public Chunk(Vector3 position)
    {
        Position = position;
        Blocks = new Block[Width * Height * Width];
    }

    public static int GetXFromIndex(int index) => index % Width;
    public static int GetYFromIndex(int index) => (index / Width) % Height;
    public static int GetZFromIndex(int index) => index / (Width * Height);
    public static int GetIndexFromX(int x) => x;
    public static int GetIndexFromY(int y) => y * Width;
    public static int GetIndexFromZ(int z) => z * Width * Height;

    public void GenerateTerrain()
    {
        Vector3 blockPosition;
        Vector3 worldBlockPosition;

        const float terrainFrequency = 1f;
        const float sandPatchFrequency = 2f; // Controls the size of sand patches

        // --- Terrain Parameters ---
        const int dirtLayerDepth = 3;
        int seaLevel = Height / 2;
        const int beachHeight = 1; // How many blocks above sea level beaches can form

        for (byte x = 0; x < Width; x++)
        {
            for (byte z = 0; z < Width; z++)
            {
                // --- PERFORM ALL NOISE CALLS HERE (ONCE PER COLUMN) ---
                float worldX = Position.X + x;
                float worldZ = Position.Z + z;

                // 1. Primary Terrain Height
                float heightNoise = Math.Max(MineDirtGame.Noise.GetNoise(worldX * terrainFrequency, worldZ * terrainFrequency), MineDirtGame.Noise.GetNoise(worldX * terrainFrequency * 1.25f, worldZ * terrainFrequency * 1.25f));
                int maxHeight = (int)(Utils.ScaleNoise(heightNoise, 0.25f, 0.75f) * Height);
                maxHeight = MathHelper.Clamp(maxHeight, 1, Height - 1);

                // 2. Sand Patches
                float sandNoise = MineDirtGame.Noise.GetNoise(worldX * sandPatchFrequency, worldZ * sandPatchFrequency);
                bool createSandPatch = sandNoise > 0.25f;

                // --- GENERATE THE BLOCK COLUMN ---
                for (byte y = 0; y < Height; y++)
                {
                    blockPosition = new Vector3(x, y, z);
                    worldBlockPosition.Y = Position.Y + y;

                    Block block;
                    int blockIndex = GetIndexFromX(x) + GetIndexFromY(y) + GetIndexFromZ(z);

                    if (worldBlockPosition.Y > maxHeight)
                    {
                        // Position is ABOVE ground level, fill with Air or Water
                        if (worldBlockPosition.Y <= seaLevel)
                        {
                            block = new Block(BlockType.Water);
                            BlockCount++;
                        }
                        else
                        {
                            block = new Block(BlockType.Air);
                        }
                    }
                    else
                    {
                        // Position is AT or BELOW ground level, fill with solid blocks
                        BlockCount++; // We know it's a solid block, so increment count here

                        // Check if the terrain is low enough to be a beach or seabed
                        bool isBeachZone = maxHeight <= seaLevel + beachHeight;

                        if (worldBlockPosition.Y == 0)
                        {
                            block = new Block(BlockType.Bedrock);
                        }
                        else if (createSandPatch && isBeachZone && worldBlockPosition.Y > maxHeight - dirtLayerDepth)
                        {
                            // If we are in a designated sand patch area (near sea level),
                            // replace the entire topsoil layer with sand.
                            block = new Block(BlockType.Sand);
                        }
                        else if (worldBlockPosition.Y == maxHeight)
                        {
                            // This is the surface block.
                            // If it's not a sand beach, it's grass (above water) or dirt (underwater).
                            block = new Block(worldBlockPosition.Y >= seaLevel ? BlockType.Grass : BlockType.Dirt);
                        }
                        else if (worldBlockPosition.Y > maxHeight - dirtLayerDepth)
                        {
                            // The standard dirt layer just below the surface.
                            block = new Block(BlockType.Dirt);
                        }
                        else
                        {
                            // Deep underground stone layer, with variation.
                            block = new Block(BlockType.Stone);
                        }
                    }

                    Blocks[blockIndex] = block;
                }
            }
        }

        HasGeneratedTerrain = true;
    }

    public ChunkMeshData GenerateMeshData()
    {
        if (BlockCount <= 0)
            return default;

        // Create vertex and index lists
        List<QuantizedVertex> allVertices = [];
        List<int> allIndices = [];

        int vertexOffset = 0;
        int indexOffset = 0;

        // Create transparent vertex and index lists
        List<QuantizedVertex> allTransparentVertices = [];
        List<int> allTransparentIndices = [];

        int transparentVertexOffset = 0;
        int transparentIndexOffset = 0;

        Vector3 blockPos = new();
        for (ushort k = 0; k < Blocks.Length; k++)
        {
            Block block = Blocks[k];
            if (block.Type == BlockType.Air)
                continue;

            // TODO: optimize in the shader
            blockPos.X = GetXFromIndex(k);
            blockPos.Y = GetYFromIndex(k);
            blockPos.Z = GetZFromIndex(k);

            for (byte faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                if (!IsFaceVisible(k, Block.Faces[faceIndex]))
                    continue;

                // Add the vertices and indices for this face
                QuantizedVertex[] faceVertices = BlockRendering.GetFaceVertices(
                    block.Type,
                    faceIndex,
                    blockPos + Position
                );

                if (block.IsOpaque)
                {
                    for (int i = 0; i < faceVertices.Length; i++)
                        allVertices.Add(faceVertices[i]);

                    for (byte i = 0; i < BlockRendering.Indices.Length; i++)
                        allIndices.Add(BlockRendering.Indices[i] + vertexOffset);

                    vertexOffset += faceVertices.Length;
                    indexOffset += BlockRendering.Indices.Length;
                }
                else
                {
                    for (int i = 0; i < faceVertices.Length; i++)
                        allTransparentVertices.Add(faceVertices[i]);

                    for (byte i = 0; i < BlockRendering.Indices.Length; i++)
                        allTransparentIndices.Add(BlockRendering.Indices[i] + transparentVertexOffset);

                    transparentVertexOffset += faceVertices.Length;
                    transparentIndexOffset += BlockRendering.Indices.Length;
                }
                // Update the block's bitmask
                Blocks[k].SetAdjacentFaceVisibility(Block.AdjentFaceMask.Front, true);
            }

            // Update the block's bitmask
            Blocks[k].SetIsBitmaskBuilt(true);
        }

        return new ChunkMeshData()
        {
            Indices = allIndices,
            Vertices = allVertices,
            TransparentIndices = allTransparentIndices,
            TransparentVertices = allTransparentVertices,
        };
    }

    public void GenerateBuffers(ChunkMeshData meshData)
    {
        if (meshData.Indices.Count > 0)
        {
            VertexBuffer = new VertexBuffer(
                MineDirtGame.Graphics.GraphicsDevice,
                typeof(QuantizedVertex),
                meshData.Vertices.Count,
                BufferUsage.WriteOnly
            );

            VertexBuffer.SetData(meshData.Vertices.ToArray());

            IndexBuffer = new IndexBuffer(
                MineDirtGame.Graphics.GraphicsDevice,
                IndexElementSize.ThirtyTwoBits,
                meshData.Indices.Count,
                BufferUsage.WriteOnly
            );

            IndexBuffer.SetData(meshData.Indices.ToArray());
        }
        else
        {
            VertexBuffer = null;
            IndexBuffer = null;
        }

        if (meshData.TransparentIndices.Count > 0)
        {
            TransparentVertexBuffer = new VertexBuffer(
                MineDirtGame.Graphics.GraphicsDevice,
                typeof(QuantizedVertex),
                meshData.TransparentVertices.Count,
                BufferUsage.WriteOnly
            );
            
            TransparentVertexBuffer.SetData(meshData.TransparentVertices.ToArray());

            TransparentIndexBuffer = new IndexBuffer(
                MineDirtGame.Graphics.GraphicsDevice,
                IndexElementSize.ThirtyTwoBits,
                meshData.TransparentIndices.Count,
                BufferUsage.WriteOnly
            );

            TransparentIndexBuffer.SetData(meshData.TransparentIndices.ToArray());
        }
        else
        {
            TransparentVertexBuffer = null;
            TransparentIndexBuffer = null;
        }
    }

    bool IsFaceVisible(ushort blockIndex, short direction)
    {
        Block block = Blocks[blockIndex];
        int unwrappedNbIndex = blockIndex + direction;
        int unwX = GetXFromIndex(blockIndex) + GetXFromIndex(direction);
        int unwY = GetYFromIndex(blockIndex) + GetYFromIndex(direction);
        int unwZ = GetZFromIndex(blockIndex) + GetZFromIndex(direction);

        if (unwY < 0 || unwY >= Height)
            return true;

        if (unwX < 0 || unwX >= Width || unwY < 0 || unwY >= Height || unwZ < 0 || unwZ >= Width)
        {
            var worldBlockPos = new Vector3(
                Position.X + unwX,
                Position.Y + unwY,
                Position.Z + unwZ
            );

            if (World.TryGetBlock(worldBlockPos, out Block neighborBlock))
                return !(neighborBlock.Type == block.Type || neighborBlock.IsOpaque);
            else
                return true; // Neighbor chunk not loaded, face is visible.
        }

        Block nbBlock = Blocks[unwrappedNbIndex];

        return !(nbBlock.Type == block.Type || nbBlock.IsOpaque);
    }

    public bool TryGetBlockChunkNeighbours(int index, out List<Vector3> subchunkPos)
    {
        int x = GetXFromIndex(index);
        int y = GetYFromIndex(index);
        int z = GetZFromIndex(index);

        subchunkPos = [];

        if (x == 0)
            subchunkPos.Add(Position + (new Vector3(-1, 0, 0) * Width));

        if (x == Width - 1)
            subchunkPos.Add(Position + (new Vector3(1, 0, 0) * Width));

        if (y == 0)
            subchunkPos.Add(Position + (new Vector3(0, -1, 0) * Height));

        if (y == Height - 1)
            subchunkPos.Add(Position + (new Vector3(0, 1, 0) * Height));

        if (z == 0)
            subchunkPos.Add(Position + (new Vector3(0, 0, -1) * Width));

        if (z == Width - 1)
            subchunkPos.Add(Position + (new Vector3(0, 0, 1) * Width));

        return subchunkPos.Count > 0;
    }

    // TODO: block ordering for transparency 
    public void DrawOpaque(Effect effect)
    {
        if (BlockCount <= 0 || VertexBuffer == null || IndexBuffer == null)
            return;

        // Set the chunk's vertex buffer and index buffer
        MineDirtGame.Graphics.GraphicsDevice.SetVertexBuffer(VertexBuffer);
        MineDirtGame.Graphics.GraphicsDevice.Indices = IndexBuffer;

        // Apply the custom shader
        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply(); // Apply the pass to set up the shader
            MineDirtGame.Graphics.GraphicsDevice.DrawIndexedPrimitives(
                PrimitiveType.TriangleList,
                0,
                0,
                IndexCount / 3
            );
        }
    }

    public void DrawTransparent(Effect effect)
    {
        if (BlockCount <= 0 || TransparentVertexBuffer == null || TransparentIndexBuffer == null)
            return;

        // Set the chunk's vertex buffer and index buffer
        MineDirtGame.Graphics.GraphicsDevice.SetVertexBuffer(TransparentVertexBuffer);
        MineDirtGame.Graphics.GraphicsDevice.Indices = TransparentIndexBuffer;

        // Apply the custom shader
        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply(); // Apply the pass to set up the shader
            MineDirtGame.Graphics.GraphicsDevice.DrawIndexedPrimitives(
                PrimitiveType.TriangleList,
                0,
                0,
                TransparentIndexCount / 3
            );
        }
    }
}
