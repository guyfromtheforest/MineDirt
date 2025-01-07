using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MineDirt.Src;

public static class World
{
    public static ConcurrentDictionary<Vector3, Chunk> Chunks = [];
    public static short RenderDistance { get; private set; } = 16;

    public static long VertexCount => Chunks.Values.ToList().Sum(chunk => chunk.VertexCount);
    public static long IndexCount => Chunks.Values.ToList().Sum(chunk => chunk.IndexCount);

    public static TaskProcessor WorldGenThread = new(4);

    public static void Initialize() { }

    public static bool done = false;

    public static void UpdateChunks()
    {
        Vector3[] positions = [new(0, 0, 0), new(16, 0, 0), new(0, 0, 16), new(16, 0, 16)];

        if (!done)
            foreach (var position in positions)
                AddChunk(position);

        done = true;
        return;

        Vector3 cameraPosition = MineDirtGame.Camera.Position;
        int chunkSize = Subchunk.Size; // Assuming Subchunk.Size is 16
        float renderDistanceSquared = RenderDistance * RenderDistance * chunkSize * chunkSize; // Square of the radius to avoid sqrt calculation

        // HashSet for efficient chunk presence checks
        HashSet<Vector3> chunksToKeep = new();

        // Loop through chunks within the render distance (in terms of chunk count, not world units)
        for (int x = -RenderDistance; x <= RenderDistance; x++)
        {
            for (int z = -RenderDistance; z <= RenderDistance; z++)
            {
                // Calculate the chunk's world position based on the camera's position and chunk size
                Vector3 chunkPosition = new(
                    (int)(Math.Floor(cameraPosition.X / chunkSize) + x) * chunkSize,
                    0, // Assuming Y is always 0 for simplicity
                    (int)(Math.Floor(cameraPosition.Z / chunkSize) + z) * chunkSize
                );

                Vector3 chunkCenter = chunkPosition + new Vector3(chunkSize / 2, 0, chunkSize / 2);
                float distanceSquared = Vector3.DistanceSquared(
                    new Vector3(cameraPosition.X, 0, cameraPosition.Z),
                    chunkCenter
                );

                //If the chunk is within the render distance(in squared distance to avoid sqrt)
                if (distanceSquared <= renderDistanceSquared + 1)
                {
                    chunksToKeep.Add(chunkPosition);

                    //  Add new chunk if it doesn't already exist
                    if (!Chunks.ContainsKey(chunkPosition))
                        AddChunk(chunkPosition);
                }
            }
        }

        //Remove chunks outside the render distance
        foreach (var item in Chunks.Keys)
            if (!chunksToKeep.Contains(item))
                Chunks.Remove(item, out _);
    }

    private static void AddChunk(Vector3 position)
    {
        // Check if the chunk already exists

        // Add the new chunk without generating buffers
        Chunk newChunk = new(position);
        Chunks.TryAdd(position, newChunk);

        // Add neighboring chunks
        Vector3[] neighbors =
        [
            new Vector3(position.X - Subchunk.Size, 0, position.Z), // West
            new Vector3(position.X + Subchunk.Size, 0, position.Z), // East
            new Vector3(position.X, 0, position.Z - Subchunk.Size), // South
            new Vector3(position.X, 0, position.Z + Subchunk.Size), // North
            //new Vector3(position.X + Subchunk.Size, position.Y, position.Z + Subchunk.Size),  // North-East
            //new Vector3(position.X - Subchunk.Size, position.Y, position.Z + Subchunk.Size),  // North-West
            //new Vector3(position.X + Subchunk.Size, position.Y, position.Z - Subchunk.Size),  // South-East
            //new Vector3(position.X - Subchunk.Size, position.Y, position.Z - Subchunk.Size),  // South-West
        ];

        foreach (var item in neighbors)
        {
            if (Chunks.TryGetValue(item, out Chunk chunk))
            {
                if (!chunk.IsUpdatingBuffers)
                {
                    chunk.IsUpdatingBuffers = true;
                    chunk.GenerateSubchunkBuffers();
                }
            }
        }

        //foreach (Vector3 neighborPosition in neighbors)
        //{
        //    Chunk neighbourChunk;

        //    if (!Chunks.ContainsKey(neighborPosition))
        //    {
        //        // Add the neighboring chunk without generating buffers
        //        neighbourChunk = new(neighborPosition);
        //        Chunks.TryAdd(neighborPosition, neighbourChunk);
        //    }
        //    else
        //    {
        //        neighbourChunk = Chunks[neighborPosition];
        //    }

        //    //neighbourChunk.HasUpdatedBuffers = false;

        //    //if (HasAllNeighbours(neighborPosition))
        //    //{
        //    //    neighbourChunk.HasUpdatedBuffers = false;

        //    //    Vector3[] neighboursNeighbours = [
        //    //        new Vector3(neighborPosition.X - Subchunk.Size, neighborPosition.Y, neighborPosition.Z), // West
        //    //        new Vector3(neighborPosition.X + Subchunk.Size, neighborPosition.Y, neighborPosition.Z), // East
        //    //        new Vector3(neighborPosition.X, neighborPosition.Y, neighborPosition.Z - Subchunk.Size), // South
        //    //        new Vector3(neighborPosition.X, neighborPosition.Y, neighborPosition.Z + Subchunk.Size),  // North
        //    //    ];

        //    //    foreach (Vector3 neighboursNeighbourPosition in neighboursNeighbours)
        //    //        if (Chunks.TryGetValue(neighboursNeighbourPosition, out Chunk value) && HasAllNeighbours(neighboursNeighbourPosition))
        //    //            value.HasUpdatedBuffers = false;
        //    //}
        //}
    }

    private static void GenerateBuffers()
    {
        foreach (Chunk item in Chunks.Values)
        {
            if (!item.HasUpdatedBuffers)
                WorldGenThread.EnqueueTask(item.GenerateSubchunkBuffers);
        }
    }

    public static void DrawChunks(Effect effect)
    {
        // Get the camera's frustum
        BoundingFrustum frustum = new(MineDirtGame.Camera.View * MineDirtGame.Camera.Projection);

        foreach (var chunk in Chunks.Values)
        {
            // Create a bounding box for the subchunk
            BoundingBox chunkBoundingBox = new(
                chunk.Position - new Vector3(Subchunk.Size, Chunk.Height, Subchunk.Size),
                chunk.Position + new Vector3(Subchunk.Size, Chunk.Height, Subchunk.Size)
            );

            // Check if the bounding box is inside the frustum
            if (frustum.Contains(chunkBoundingBox) != ContainmentType.Disjoint)
            {
                // Only draw subchunks within the frustum
                chunk.Draw(effect);
            }
        }
    }

    public static bool TryGetBlock(Vector3 subchunk, int blockIndex, out Block block)
    {
        if (Chunks.TryGetValue(new Vector3(subchunk.X, 0, subchunk.Z), out Chunk chunk))
        {
            if (chunk.Subchunks.TryGetValue(subchunk, out Subchunk subchunkValue))
            {
                block = subchunkValue.Blocks[blockIndex];

                return true;
            }

            block = default;
            return true;
        }

        block = default;
        return true;
    }

    public static bool TryGetBlock(Vector3 position, out Block block)
    {
        if (Chunks.TryGetValue(position.ToChunkPosition(), out Chunk chunk))
        {
            if (
                chunk.Subchunks.TryGetValue(
                    position.ToSubchunkPosition(),
                    out Subchunk subchunkValue
                )
            )
            {
                block = subchunkValue.Blocks[position.ToSubchunkRelativePosition().ToIndex()];

                return true;
            }

            block = default;
            return true;
        }

        block = default;
        return true;
    }
}
