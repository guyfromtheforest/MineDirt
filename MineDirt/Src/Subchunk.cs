using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MineDirt;
using MineDirt.Src;
using System;
using System.Collections.Generic;

public class Subchunk
{
    public static int Size { get; private set; } = 16;
    public Vector3 Position { get; private set; }
    public Dictionary<Vector3, Block> ChunkBlocks { get; private set; }
    public Chunk Chunk { get; private set; }
    public VertexBuffer VertexBuffer { get; private set; }
    public IndexBuffer IndexBuffer { get; private set; }
    
    public int VertexCount => VertexBuffer?.VertexCount ?? 0;
    public int IndexCount => IndexBuffer?.IndexCount ?? 0;

    static Vector3[] faceDirections =
    [
        new Vector3(0, 0, -1), // Front
        new Vector3(0, 0, 1),  // Back
        new Vector3(-1, 0, 0), // Left
        new Vector3(1, 0, 0),  // Right
        new Vector3(0, 1, 0),  // Top
        new Vector3(0, -1, 0), // Bottom
    ];

    public Subchunk(Chunk chunk, Vector3 position)
    {
        Chunk = chunk;
        Position = position;
        ChunkBlocks = [];

        // Generate blocks in the chunk
        GenerateBlocks();
    }

    private void GenerateBlocks()
    {
        for (int x = 0; x < Size; x++)
        {
            for (int z = 0; z < Size; z++)
            {
                float noiseValue = MineDirtGame.Noise.Generate(Position.X + x, Position.Z + z) * Chunk.Height; // Scale noise to desired height
                int maxHeight = MathHelper.Clamp((int)Math.Round(noiseValue), 1, Chunk.Height * Size - 1);

                for (int y = 0; y < Size; y++)
                {
                    Vector3 blockPosition = Position + new Vector3(x, y, z);

                    if (blockPosition.Y == 0)
                    {
                        // Bedrock at the bottom layer
                        ChunkBlocks.Add(blockPosition, Blocks.Bedrock(blockPosition));
                    }
                    else if (blockPosition.Y < maxHeight - 10)
                    {
                        // Stone below the surface
                        ChunkBlocks.Add(blockPosition, Blocks.Stone(blockPosition));
                    }
                    else if (blockPosition.Y < maxHeight - 1)
                    {
                        // Dirt below the surface
                        ChunkBlocks.Add(blockPosition, Blocks.Dirt(blockPosition));
                    }
                    else if (blockPosition.Y == maxHeight - 1)
                    {
                        // Grass on the surface
                        ChunkBlocks.Add(blockPosition, Blocks.Grass(blockPosition));
                    }
                    else
                    {
                        // Air (no block) above the surface
                        // Optionally skip adding blocks above the surface for optimization
                    }
                }
            }
        }

        //int l = 1;
        //for (int i = 0; i < l; i++)
        //{
        //    for (int j = 0; j < l; j++)
        //    {
        //        for (int k = 0; k < l; k++)
        //        {
        //            ChunkBlocks.Add(new Vector3(i, j, k), Blocks.Stone(new Vector3(i, j, k)));
        //        }
        //    }
        //}
    }

    public void GenerateBuffers()
    {
        if (ChunkBlocks.Count == 0)
            return;

        // Calculate total number of vertices and indices needed for the chunk
        int totalVertices = ChunkBlocks.Count * 24;  // 24 vertices per block (6 faces, 4 vertices per face)
        int totalIndices = ChunkBlocks.Count * 36;   // 36 indices per block (6 faces, 2 triangles per face)

        // Create vertex and index arrays
        VertexPositionTexture[] allVertices = new VertexPositionTexture[totalVertices];
        int[] allIndices = new int[totalIndices];

        int vertexOffset = 0;
        int indexOffset = 0;

        foreach (KeyValuePair<Vector3, Block> block in ChunkBlocks)
        {
            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                if (IsFaceVisible(block.Key, faceDirections[faceIndex]))
                {
                    // Add the vertices and indices for this face
                    VertexPositionTexture[] faceVertices = block.Value.GetFaceVertices(faceIndex, block.Key);

                    for (int i = 0; i < faceVertices.Length; i++)
                        allVertices[vertexOffset + i] = faceVertices[i];

                    for (int i = 0; i < Block.Indices.Length; i++)
                        allIndices[indexOffset + i] = Block.Indices[i] + vertexOffset;

                    vertexOffset += faceVertices.Length;
                    indexOffset += Block.Indices.Length;
                }
            }
        }

        // Create the buffers
        if (allVertices.Length == 0 || allIndices.Length == 0)
            return;

        VertexBuffer = new VertexBuffer(MineDirtGame.Graphics.GraphicsDevice, typeof(VertexPositionTexture), allVertices.Length, BufferUsage.WriteOnly);
        VertexBuffer.SetData(allVertices);

        IndexBuffer = new IndexBuffer(MineDirtGame.Graphics.GraphicsDevice, IndexElementSize.ThirtyTwoBits, allIndices.Length, BufferUsage.WriteOnly);
        IndexBuffer.SetData(allIndices);
    }

    bool IsFaceVisible(Vector3 blockPosition, Vector3 direction)
    {
        Vector3 neighborPosition = blockPosition + direction;

        // Check if neighborPosition is out of the current subchunk bounds
        bool isOutOfBounds =
            neighborPosition.X < Position.X || neighborPosition.X >= Position.X + Size ||
            neighborPosition.Y < Position.Y || neighborPosition.Y >= Position.Y + Size ||
            neighborPosition.Z < Position.Z || neighborPosition.Z >= Position.Z + Size;

        if (isOutOfBounds)
        {
            // Calculate the chunk position in world coordinates
            Vector3 chunkPos = neighborPosition.ToChunkPosition();

            Chunk chunk; 
            
            if(direction.Z == 0 && direction.X == 0)
                chunk = Chunk; 
            else
                chunk = World.Chunks.GetValueOrDefault(chunkPos);

            if (chunk == null)
                return true; // Neighbor chunk does not exist, face is visible

            // Calculate the subchunk position in world coordinates
            Vector3 subchunkPos = neighborPosition.ToSubchunkPosition();

            Subchunk subchunk = chunk.Subchunks.GetValueOrDefault(subchunkPos);

            if (subchunk == null)
                return true; // Neighbor subchunk does not exist, face is visible

            // Check if the neighbor block exists
            if (subchunk.ChunkBlocks.TryGetValue(neighborPosition, out Block neighborBlock))
                return false; // Neighbor block exists, face is not visible
        }
        else if (ChunkBlocks.TryGetValue(neighborPosition, out Block neighborBlock))
        {
            return false; // Neighbor block exists within the same subchunk
        }

        return true; // No neighbor block exists, face is visible
    }

    public void Draw(BasicEffect effect)
    {
        if (VertexCount == 0)
            return;

        // Set the texture for the chunk
        effect.TextureEnabled = true;
        effect.Texture = MineDirtGame.BlockTextures;

        // Set the chunk's vertex buffer and index buffer
        MineDirtGame.Graphics.GraphicsDevice.SetVertexBuffer(VertexBuffer);
        MineDirtGame.Graphics.GraphicsDevice.Indices = IndexBuffer;

        // Apply the effect and draw the chunk
        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            MineDirtGame.Graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, IndexBuffer.IndexCount / 3);
        }
    }
}
