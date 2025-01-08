using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

    private static Vector3 lastCameraChunkPosition = new(0, -1, 0);

    // public static bool done = false;

    public static void UpdateChunks()
    {
        //Vector3[] positions = [new(0, 0, 0), new(16, 0, 0), new(0, 0, 16), new(16, 0, 16)];

        //if (!done)
        //    foreach (Vector3 position in positions)
        //        AddChunk(position);

        //done = true;
        //return;

        Vector3 cameraPosition = MineDirtGame.Camera.Position;
        Vector3 cameraChunkPosition = cameraPosition.ToChunkPosition();

        // HashSet for efficient chunk presence checks
        HashSet<Vector3> chunksToKeep = new();

        // Dont update the chunks if the camera hasn't moved to a new chunk
        if (cameraChunkPosition == lastCameraChunkPosition)
            return;

        float renderDistanceSquared =
            RenderDistance * RenderDistance * Subchunk.Size * Subchunk.Size;

        cameraPosition.Y = 0;

        for (int x = -RenderDistance; x <= RenderDistance; x++)
        {
            for (int z = -RenderDistance; z <= RenderDistance; z++)
            {
                Vector3 chunkPosition = new(
                    (int)(Math.Floor(cameraPosition.X / Subchunk.Size) + x) * Subchunk.Size,
                    0, // Assuming Y is always 0 for simplicity
                    (int)(Math.Floor(cameraPosition.Z / Subchunk.Size) + z) * Subchunk.Size
                );

                Vector3 chunkCenter = new(
                    chunkPosition.X + (Subchunk.Size / 2),
                    0,
                    chunkPosition.Z + (Subchunk.Size / 2)
                );

                float distanceSquared = Vector3.DistanceSquared(cameraPosition, chunkCenter);

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
        foreach (Vector3 item in Chunks.Keys)
            if (!chunksToKeep.Contains(item))
                Chunks.Remove(item, out _);

        lastCameraChunkPosition = cameraChunkPosition;
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
        ];

        foreach (Vector3 item in neighbors)
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

        foreach (Chunk chunk in Chunks.Values)
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
            return false;
        }

        block = default;
        return false;
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
            return false;
        }

        block = default;
        return false;
    }
}
