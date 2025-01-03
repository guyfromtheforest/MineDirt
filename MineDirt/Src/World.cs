using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MineDirt.Src;
public static class World
{
    public static Dictionary<Vector3, Chunk> Chunks = [];
    public static short RenderDistance { get; private set; } = 16;

    public static long VertexCount => Chunks.Values.ToList().Sum(chunk => chunk.VertexCount);
    public static long IndexCount => Chunks.Values.ToList().Sum(chunk => chunk.IndexCount);

    public static void Initialize()
    {

    }

    public static void UpdateChunks()
    {
        Vector3 cameraPosition = MineDirtGame.Camera.Position;
        int chunkSize = Subchunk.Size; // Assuming Subchunk.Size is 16
        float renderDistanceSquared = RenderDistance * RenderDistance * chunkSize * chunkSize;  // Square of the radius to avoid sqrt calculation

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

                // If the chunk is within the render distance (in squared distance to avoid sqrt)
                if (distanceSquared <= renderDistanceSquared)
                {
                    chunksToKeep.Add(chunkPosition);

                    // Add new chunk if it doesn't already exist
                    AddChunk(chunkPosition);
                }
            }
        }

        // Remove chunks outside the render distance
        foreach (var item in Chunks.Keys)
            if (!chunksToKeep.Contains(item))
                Chunks.Remove(item);

        //Vector3[] positions = [
        //    new(0, 0, 0),
        //    new(16, 0, 0),
        //    new(0, 0, 16),
        //    new(16, 0, 16),
        //];

        //foreach (var position in positions)
        //    AddChunk(position);

        GenerateBuffers();
    }

    private static void AddChunk(Vector3 position)
    {
        // Check if the chunk already exists
        if (!Chunks.ContainsKey(position))
        {
            // Add the new chunk without generating buffers
            Chunk newChunk = new(position);
            Chunks.Add(position, newChunk);

            // Add neighboring chunks
            Vector3[] neighbors = [
                new Vector3(position.X - Subchunk.Size, position.Y, position.Z), // West
                    new Vector3(position.X + Subchunk.Size, position.Y, position.Z), // East
                    new Vector3(position.X, position.Y, position.Z - Subchunk.Size), // South
                    new Vector3(position.X, position.Y, position.Z + Subchunk.Size),  // North
                ];

            foreach (Vector3 neighborPosition in neighbors)
            {
                Chunk neighbourChunk;

                if (!Chunks.ContainsKey(neighborPosition))
                {
                    // Add the neighboring chunk without generating buffers
                    neighbourChunk = new(neighborPosition);
                    neighbourChunk.HasUpdatedBuffers = true; // Skip buffer generation for neighbors
                    Chunks.Add(neighborPosition, neighbourChunk);
                }
                else
                {
                    neighbourChunk = Chunks[neighborPosition];
                }

                if (HasAllNeighbours(neighborPosition))
                {
                    neighbourChunk.HasUpdatedBuffers = false;

                    Vector3[] neighboursNeighbours = [
                        new Vector3(neighborPosition.X - Subchunk.Size, neighborPosition.Y, neighborPosition.Z), // West
                        new Vector3(neighborPosition.X + Subchunk.Size, neighborPosition.Y, neighborPosition.Z), // East
                        new Vector3(neighborPosition.X, neighborPosition.Y, neighborPosition.Z - Subchunk.Size), // South
                        new Vector3(neighborPosition.X, neighborPosition.Y, neighborPosition.Z + Subchunk.Size),  // North
                    ];

                    foreach (Vector3 neighboursNeighbourPosition in neighboursNeighbours)
                        if (Chunks.TryGetValue(neighboursNeighbourPosition, out Chunk value))
                            value.HasUpdatedBuffers = false;
                }
            }
        }
    }

    private static void GenerateBuffers()
    {
        foreach (Chunk item in Chunks.Values)
        {
            if (!item.HasUpdatedBuffers)
                item.GenerateSubchunkBuffers();
        }
    }

    private static bool HasAllNeighbours(Vector3 position)
    {
        Vector3[] neighbors = [
            new Vector3(position.X - Subchunk.Size, position.Y, position.Z), // West
            new Vector3(position.X + Subchunk.Size, position.Y, position.Z), // East
            new Vector3(position.X, position.Y, position.Z - Subchunk.Size), // South
            new Vector3(position.X, position.Y, position.Z + Subchunk.Size),  // North
        ];

        foreach (Vector3 neighborPosition in neighbors)
            if (!Chunks.ContainsKey(neighborPosition))
                return false;

        return true;
    }

    public static void DrawChunks(BasicEffect effect)
    {
        // Get the camera's frustum
        BoundingFrustum frustum = new(effect.View * effect.Projection);

        foreach (var chunk in Chunks.Values)
        {
            // Create a bounding box for the subchunk
            BoundingBox subchunkBoundingBox = new(
                chunk.Position - new Vector3(Subchunk.Size, Chunk.Height, Subchunk.Size),
                chunk.Position + new Vector3(Subchunk.Size, Chunk.Height, Subchunk.Size)
            );

            // Check if the bounding box is inside the frustum
            if (frustum.Contains(subchunkBoundingBox) != ContainmentType.Disjoint)
            {
                // Only draw subchunks within the frustum
                chunk.Draw(effect);
            }
        }
    }
}
