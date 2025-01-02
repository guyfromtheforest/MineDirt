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

    // List to store all blocks in the chunk
    public List<Block> ChunkBlocks { get; private set; }

    // Vertex and Index Buffers for the entire chunk
    public VertexBuffer VertexBuffer { get; private set; }
    public IndexBuffer IndexBuffer { get; private set; }

    public Subchunk(Vector3 position)
    {
        Position = position;
        ChunkBlocks = [];

        // Generate blocks in the chunk
        GenerateBlocks();
        CreateBuffers();
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
                        ChunkBlocks.Add(Blocks.Bedrock(blockPosition));
                    }
                    else if (blockPosition.Y < maxHeight - 1)
                    {
                        // Stone below the surface
                        ChunkBlocks.Add(Blocks.Stone(blockPosition));
                    }
                    else if (blockPosition.Y == maxHeight - 1)
                    {
                        // Grass on the surface
                        ChunkBlocks.Add(Blocks.Grass(blockPosition));
                    }
                    else
                    {
                        // Air (no block) above the surface
                        // Optionally skip adding blocks above the surface for optimization
                    }
                }
            }
        }
    }

    private void CreateBuffers()
    {
        if(ChunkBlocks.Count == 0)
            return; 

        // Calculate total number of vertices and indices needed for the chunk
        int totalVertices = ChunkBlocks.Count * 24;  // 24 vertices per block (6 faces, 4 vertices per face)
        int totalIndices = ChunkBlocks.Count * 36;   // 36 indices per block (6 faces, 2 triangles per face)

        // Create vertex and index arrays
        VertexPositionTexture[] allVertices = new VertexPositionTexture[totalVertices];
        int[] allIndices = new int[totalIndices];

        int vertexOffset = 0;
        int indexOffset = 0;

        // Fill the vertex and index arrays for each block
        foreach (var block in ChunkBlocks)
        {
            // Add block's vertices and indices to the arrays
            for (int i = 0; i < block.Vertices.Length; i++)
                allVertices[vertexOffset + i] = block.Vertices[i];

            for (int i = 0; i < block.Indices.Length; i++)
                allIndices[indexOffset + i] = (block.Indices[i] + vertexOffset);  // Adjust index for global vertex offset

            vertexOffset += block.Vertices.Length;
            indexOffset += block.Indices.Length;
        }

        // Create the buffers
        VertexBuffer = new VertexBuffer(MineDirtGame.Graphics.GraphicsDevice, typeof(VertexPositionTexture), allVertices.Length, BufferUsage.WriteOnly);
        VertexBuffer.SetData(allVertices);

        IndexBuffer = new IndexBuffer(MineDirtGame.Graphics.GraphicsDevice, IndexElementSize.ThirtyTwoBits, allIndices.Length, BufferUsage.WriteOnly);
        IndexBuffer.SetData(allIndices);
    }

    public void Draw(BasicEffect effect)
    {
        if(ChunkBlocks.Count == 0)
            return;

        // Set the texture for the chunk
        effect.TextureEnabled = true;
        effect.Texture = MineDirtGame.BlockTextures;

        // Set the chunk's vertex buffer and index buffer
        MineDirtGame.Graphics.GraphicsDevice.SetVertexBuffer(VertexBuffer);
        MineDirtGame.Graphics.GraphicsDevice.Indices = IndexBuffer;

        // Apply the effect and draw the chunk
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            MineDirtGame.Graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, IndexBuffer.IndexCount / 3);
        }
    }
}
