using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MineDirt;
using MineDirt.Src;
using MineDirt.Src.Noise;

public class Subchunk
{
    public static ushort Size { get; private set; } = 16;
    public Vector3 Position { get; private set; }
    public Block[] Blocks { get; private set; }
    public ushort BlockCount;
    public VertexBuffer VertexBuffer { get; private set; }
    public IndexBuffer IndexBuffer { get; private set; }

    public int VertexCount => VertexBuffer?.VertexCount ?? 0;
    public int IndexCount => IndexBuffer?.IndexCount ?? 0;

    //static readonly Vector3[] faceDirections =
    //[
    //    new Vector3(0, 0, -1), // Front
    //    new Vector3(0, 0, 1), // Back
    //    new Vector3(-1, 0, 0), // Left
    //    new Vector3(1, 0, 0), // Right
    //    new Vector3(0, 1, 0), // Top
    //    new Vector3(0, -1, 0), // Bottom
    //];

    public Subchunk(Chunk chunk, Vector3 position)
    {
        Position = position;
        Blocks = new Block[Size * Size * Size];

        // Generate blocks in the chunk
        GenerateBlocks();
    }

    int l = 16;
    int m = 16;
    int n = 16;

    private void GenerateBlocks()
    {
        //for (int i = 0; i < l; i++)
        //{
        //    for (int j = 0; j < m; j++)
        //    {
        //        for (int k = 0; k < n; k++)
        //        {
        //            Block block = new();

        //            block.Type = BlockType.Grass;
        //            block.SetBlockOpacity(true);

        //            int blockIndex = GetIndexFromX(i) + GetIndexFromY(j) + GetIndexFromZ(k);

        //            Blocks[blockIndex] = block;
        //            BlockCount++;
        //        }
        //    }
        //}

        //l = 0;
        //m = 0;
        //n = 0;
        //return;

        for (byte x = 0; x < Size; x++)
        {
            for (byte z = 0; z < Size; z++)
            {
                float noiseValue =
                    Utils.ScaleNoise(
                        MineDirtGame.Noise.GetNoise(Position.X + x, Position.Z + z),
                        0.25f,
                        0.75f
                    ) * Chunk.Height;
                int maxHeight = MathHelper.Clamp((int)Math.Round(noiseValue), 1, Chunk.Height - 1);

                for (byte y = 0; y < Size; y++)
                {
                    Vector3 blockPosition = new(x, y, z);
                    Vector3 worldBlockPosition = blockPosition + Position;

                    int blockIndex = GetIndexFromX(x) + GetIndexFromY(y) + GetIndexFromZ(z);
                    Block block = new();
                    block.SetBlockOpacity(true);

                    if (worldBlockPosition.Y == 0)
                    {
                        // Bedrock at the bottom layer
                        //ChunkBlocks.Add(blockPosition, Blocks.Bedrock(blockPosition));
                        block.Type = BlockType.Bedrock;
                        BlockCount++;
                    }
                    else if (worldBlockPosition.Y < maxHeight - 10)
                    {
                        // Stone below the surface
                        //ChunkBlocks.Add(blockPosition, Blocks.Stone(blockPosition));
                        block.Type = BlockType.Stone;
                        BlockCount++;
                    }
                    else if (worldBlockPosition.Y < maxHeight - 1)
                    {
                        // Dirt below the surface
                        //ChunkBlocks.Add(blockPosition, Blocks.Dirt(blockPosition));
                        block.Type = BlockType.Dirt;
                        BlockCount++;
                    }
                    else if (worldBlockPosition.Y == maxHeight - 1)
                    {
                        // Grass on the surface
                        //ChunkBlocks.Add(blockPosition, Blocks.Grass(blockPosition));
                        block.Type = BlockType.Grass;
                        BlockCount++;
                    }

                    Blocks[blockIndex] = block;
                }
            }
        }
    }

    public void GenerateBuffers()
    {
        if (BlockCount <= 0 || MineDirtGame.Graphics?.GraphicsDevice == null)
            return;

        // Create vertex and index arrays
        List<QuantizedVertex> allVertices = [];
        List<int> allIndices = [];

        int vertexOffset = 0;
        int indexOffset = 0;

        Vector3 blockPos = new();
        for (ushort k = 0; k < Blocks.Length; k++)
        {
            BlockType block = Blocks[k].Type;
            if (block == BlockType.Air)
                continue;

            // TODO: optimize in the shader
            blockPos.X = k / (Size * Size) % Size;
            blockPos.Y = k / Size % Size;
            blockPos.Z = k % Size;

            for (byte faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                if (!IsFaceVisible(k, Block.Faces[faceIndex]))
                    continue;

                // Add the vertices and indices for this face
                QuantizedVertex[] faceVertices = BlockRendering.GetFaceVertices(
                    block,
                    faceIndex,
                    blockPos + Position
                );

                for (int i = 0; i < faceVertices.Length; i++)
                    allVertices.Add(faceVertices[i]);

                for (byte i = 0; i < BlockRendering.Indices.Length; i++)
                    allIndices.Add(BlockRendering.Indices[i] + vertexOffset);

                vertexOffset += faceVertices.Length;
                indexOffset += BlockRendering.Indices.Length;

                // Update the block's bitmask
                Blocks[k].SetAdjacentFaceVisibility(Block.AdjentFaceMask.Front, true);
            }

            // Update the block's bitmask
            Blocks[k].SetIsBitmaskBuilt(true);
        }

        // Create the buffers
        if (allVertices.Count == 0 || allIndices.Count == 0)
            return;

        VertexBuffer = new VertexBuffer(
            MineDirtGame.Graphics.GraphicsDevice,
            typeof(QuantizedVertex),
            allVertices.Count,
            BufferUsage.WriteOnly
        );
        VertexBuffer.SetData(allVertices.ToArray());

        IndexBuffer = new IndexBuffer(
            MineDirtGame.Graphics.GraphicsDevice,
            IndexElementSize.ThirtyTwoBits,
            allIndices.Count,
            BufferUsage.WriteOnly
        );
        IndexBuffer.SetData(allIndices.ToArray());
    }

    bool IsFaceVisible(ushort blockIndex, short direction)
    {
        int unwrappedNbIndex = blockIndex + direction;
        int unwX = GetXFromIndex(blockIndex) + GetXFromIndex(direction);
        int unwY = GetYFromIndex(blockIndex) + GetYFromIndex(direction);
        int unwZ = GetZFromIndex(blockIndex) + GetZFromIndex(direction);

        int wrappedNbIndex =
            GetIndexFromX((unwX + Size) % Size)
            + GetIndexFromY((unwY + Size) % Size)
            + GetIndexFromZ((unwZ + Size) % Size);

        bool isOutOfBounds =
            unwX < 0 || unwX >= Size || unwY < 0 || unwY >= Size || unwZ < 0 || unwZ >= Size;

        // Determine if neighbor position is out of bounds
        if (isOutOfBounds)
        {
            Vector3 subchunkDirection = new(
                direction / (Size * Size) % Size,
                direction / Size % Size,
                direction % Size
            );

            return World.TryGetBlock(
                    Position + (subchunkDirection * Size),
                    wrappedNbIndex,
                    out Block test
                )
                && test.Type == BlockType.Air;
        }

        // Check if neighbor block exists within the same subchunk
        return Blocks[unwrappedNbIndex].Type == BlockType.Air;
    }

    public static int GetXFromIndex(int index) => index / (Size * Size) % Size;

    public static int GetIndexFromX(int x) => x * Size * Size;

    public static int GetYFromIndex(int index) => index / Size % Size;

    public static int GetIndexFromY(int y) => y * Size;

    public static int GetZFromIndex(int index) => index % Size;

    public static int GetIndexFromZ(int z) => z;

    public void Draw(Effect effect)
    {
        if (VertexCount == 0 || IndexCount == 0)
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
}
