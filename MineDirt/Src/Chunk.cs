using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MineDirt;
using System.Collections.Generic;
using System.Linq;
public class Chunk
{
    public static int Height { get; private set; } = 64; // Max chunk size
    public Vector3 Position { get; private set; }

    public long VertexCount => Subchunks.Values.ToList().Sum(subchunk => subchunk.VertexCount);
    public long IndexCount => Subchunks.Values.ToList().Sum(subchunk => subchunk.IndexCount);

    public bool HasBlocks => Subchunks.Values.Any(subchunk => subchunk.ChunkBlocks.Count != 0);

    public bool HasUpdatedBuffers = false; 
    public int UpdateCount = 0;

    // List to store all subchunks
    public Dictionary<Vector3, Subchunk> Subchunks { get; private set; }

    public Chunk(Vector3 position)
    {
        Position = position;
        Subchunks = [];

        // Generate subchunks
        GenerateSubchunkBlocks();
    }

    private void GenerateSubchunkBlocks()
    {
        int subchunkCount = Height / Subchunk.Size;

        for (int y = 0; y < subchunkCount; y++)
        {
            Vector3 subchunkPosition = new(Position.X, y * Subchunk.Size, Position.Z);

            // Create and add the subchunk
            Subchunks.Add(subchunkPosition, new Subchunk(this, subchunkPosition));
        }
    }

    public void GenerateSubchunkBuffers()
    {
        HasUpdatedBuffers = true;

        foreach (var subchunk in Subchunks)
        {
            subchunk.Value.GenerateBuffers();
        }        
        
        UpdateCount++;
    }   

    public void Draw(BasicEffect effect)
    {
        foreach (var item in Subchunks)
            item.Value.Draw(effect);
    }
}
