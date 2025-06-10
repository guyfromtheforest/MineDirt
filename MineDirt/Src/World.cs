using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MineDirt.Src;

public static class World
{
    public static ConcurrentDictionary<Vector3, Chunk> Chunks = [];
    private static IOrderedEnumerable<KeyValuePair<Vector3, Chunk>> sortedChunks; 
    private static readonly Vector3 chunkCenterModifier = new(Chunk.Width / 2, 0, Chunk.Width / 2);

    public static short RenderDistance { get; private set; } = 16;

    public static long VertexCount => Chunks.Values.ToList().Sum(chunk => chunk.VertexCount);
    public static long IndexCount => Chunks.Values.ToList().Sum(chunk => chunk.IndexCount);

    public static TaskProcessor WorldMeshThreadPool = new(); // Number of threads to use for world generation and mesh building
    private const int ChunksPerFrame = 4; // Number of chunks to process per frame

    public static void Initialize() { }

    private static Vector3 lastCameraChunkPosition = new(0, -1, 0);

    // public static bool done = false;

    private static Queue<Vector3> chunkLoadQueue = new();

    public static void ReloadChunks()
    {
        lastCameraChunkPosition = new(0, -1, 0);
        Chunks.Clear();
    }

    public static void BreakBlock(Vector3 position)
    {
        if (Chunks.TryGetValue(position.ToChunkPosition(), out Chunk chunk))
        {
            int index = position.ToChunkRelativePosition().ToIndex();
            if (chunk.Blocks[index].Type == BlockType.Air)
                return;
            
            chunk.Blocks[index] = default;
            chunk.BlockCount--;
            chunk.GenerateBuffers();

            if (chunk.TryGetBlockChunkNeighbours(index, out List<Vector3> chunkNbPositions))
            {
                foreach (Vector3 chunkNbPos in chunkNbPositions)
                {
                    if (Chunks.TryGetValue(chunkNbPos, out Chunk chunkNb))
                    {
                        chunkNb.GenerateBuffers();
                    }
                }
            }
        }
    }

    public static void PlaceBlock(Vector3 position, Block block)
    {
        if(position.Y > Chunk.Height || position.Y < 0)
            return;

        if (Chunks.TryGetValue(position.ToChunkPosition(), out Chunk chunk))
        {
            int index = position.ToChunkRelativePosition().ToIndex();
            if (chunk.Blocks[index].Type != BlockType.Air)
                return; 

            chunk.Blocks[index] = block;
            chunk.BlockCount++;
            chunk.GenerateBuffers();

            if (chunk.TryGetBlockChunkNeighbours(index, out List<Vector3> chunkNbPositions))
            {
                foreach (Vector3 chunkNbPos in chunkNbPositions)
                {
                    if (Chunks.TryGetValue(chunkNbPos, out Chunk chunkNb))
                    {
                        chunkNb.GenerateBuffers();
                    }
                }
            }
        }
    }

    public static void UpdateChunks()
    {
        // Process chunks from the queue, limited by ChunksPerFrame
        for (int i = 0; i < ChunksPerFrame && chunkLoadQueue.Count > 0; i++)
        {
            Vector3 chunkToLoad = chunkLoadQueue.Dequeue();
            AddChunk(chunkToLoad);
        }

        Vector3 cameraPosition = MineDirtGame.Camera.Position;
        Vector3 cameraChunkPosition = cameraPosition.ToChunkPosition();

        // HashSet for efficient chunk presence checks
        HashSet<Vector3> chunksToKeep = new();

        // Don't update the chunks if the camera hasn't moved to a new chunk
        if (cameraChunkPosition == lastCameraChunkPosition)
            return;

        float renderDistanceSquared =
            RenderDistance * RenderDistance * Chunk.Width * Chunk.Width;

        cameraPosition.Y = 0;

        for (int r = 0; r <= RenderDistance; r++)
        {
            for (int x = -r; x <= r; x++)
            {
                for (int z = -r; z <= r; z++)
                {
                    Vector3 chunkPosition = new(
                        (int)(Math.Floor(cameraPosition.X / Chunk.Width) + x) * Chunk.Width,
                        0, // Assuming Y is always 0 for simplicity
                        (int)(Math.Floor(cameraPosition.Z / Chunk.Width) + z) * Chunk.Width
                    );

                    Vector3 chunkCenter = new(
                        chunkPosition.X + (Chunk.Width / 2),
                        0,
                        chunkPosition.Z + (Chunk.Width / 2)
                    );

                    float distanceSquared = Vector3.DistanceSquared(cameraPosition, chunkCenter);

                    // If the chunk is within the render distance (in squared distance to avoid sqrt)
                    if (distanceSquared <= renderDistanceSquared + 1)
                    {
                        chunksToKeep.Add(chunkPosition);

                        // Queue new chunks for addition if they don't already exist
                        if (!Chunks.ContainsKey(chunkPosition) && !chunkLoadQueue.Contains(chunkPosition))
                            chunkLoadQueue.Enqueue(chunkPosition);
                    }
                }
            }
        }

        // Remove chunks outside the render distance
        foreach (Vector3 item in Chunks.Keys)
            if (!chunksToKeep.Contains(item))
                Chunks.Remove(item, out _);

        lastCameraChunkPosition = cameraChunkPosition;
    }

    private static void AddChunk(Vector3 position)
    {
        // Add the new chunk without generating buffers
        Chunk newChunk = new(position);
        Chunks.TryAdd(position, newChunk);

        // Add neighboring chunks
        Vector3[] neighbors =
        [
            new Vector3(position.X - Chunk.Width, 0, position.Z), // West
            new Vector3(position.X + Chunk.Width, 0, position.Z), // East
            new Vector3(position.X, 0, position.Z - Chunk.Width), // South
            new Vector3(position.X, 0, position.Z + Chunk.Width), // North
        ];

        foreach (Vector3 item in neighbors)
        {
            if (Chunks.TryGetValue(item, out Chunk chunk))
            {
                WorldMeshThreadPool.EnqueueTask(chunk.GenerateBuffers);
            }
        }
    }

    public static void DrawChunksOpaque(Effect effect)
    {
        // Get the camera's frustum
        BoundingFrustum frustum = new(MineDirtGame.Camera.View * MineDirtGame.Camera.Projection);
        sortedChunks = Chunks.OrderByDescending(item => Vector3.DistanceSquared(MineDirtGame.Camera.Position, item.Key + chunkCenterModifier));

        foreach (var chunk in sortedChunks)
        {
            // Create a bounding box for the subchunk
            BoundingBox chunkBoundingBox = new(
                chunk.Value.Position - new Vector3(Chunk.Width, Chunk.Height, Chunk.Width),
                chunk.Value.Position + new Vector3(Chunk.Width, Chunk.Height, Chunk.Width)
            );

            // Check if the bounding box is inside the frustum
            if (frustum.Contains(chunkBoundingBox) != ContainmentType.Disjoint)
            {
                // Only draw subchunks within the frustum
                chunk.Value.DrawOpaque(effect);
            }
        }
    }

    public static void DrawChunksTransparent(Effect effect)
    {
        // Get the camera's frustum
        BoundingFrustum frustum = new(MineDirtGame.Camera.View * MineDirtGame.Camera.Projection);

        // Chunks are already sorted from drawing the opaque blocks
        foreach (var chunk in sortedChunks)
        {
            // Create a bounding box for the subchunk
            BoundingBox chunkBoundingBox = new(
                chunk.Value.Position - new Vector3(Chunk.Width, Chunk.Height, Chunk.Width),
                chunk.Value.Position + new Vector3(Chunk.Width, Chunk.Height, Chunk.Width)
            );

            // Check if the bounding box is inside the frustum
            if (frustum.Contains(chunkBoundingBox) != ContainmentType.Disjoint)
            {
                // Only draw subchunks within the frustum
                chunk.Value.DrawTransparent(effect);
            }
        }
    }

    public static bool TryGetBlock(Vector3 position, out Block block)
    {
        if(position.Y < 0 || position.Y >= Chunk.Height)
        {
            block = default;
            return false;
        }

        if (Chunks.TryGetValue(position.ToChunkPosition(), out Chunk chunk))
        {
            block = chunk.Blocks[position.ToChunkRelativePosition().ToIndex()];
            return true;
        }

        block = default;
        return false;
    }
}
