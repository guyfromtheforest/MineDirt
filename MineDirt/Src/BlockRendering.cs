using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MineDirt.Src
{
    public static class BlockRendering
    {
        public static readonly byte[] Indices = [0, 2, 3, 0, 3, 1];

        // This lookup table stores the 8 corner positions of a unit cube.
        private static readonly Vector3[] CornerOffsets =
        [
            new(0f, 1f, 0f), // 0: Top-Left-Front
            new(1f, 1f, 0f), // 1: Top-Right-Front
            new(0f, 0f, 0f), // 2: Bottom-Left-Front
            new(1f, 0f, 0f), // 3: Bottom-Right-Front
            new(0f, 1f, 1f), // 4: Top-Left-Back
            new(1f, 1f, 1f), // 5: Top-Right-Back
            new(0f, 0f, 1f), // 6: Bottom-Left-Back
            new(1f, 0f, 1f)  // 7: Bottom-Right-Back
        ];

        // This table defines which 4 corners make up each of the 6 faces.
        // The order is important for correct triangle winding (culling).
        // Front/Back/Left/Right are wound counter-clockwise when viewed from outside.
        private static readonly int[][] FaceCorners =
        [
            [ 0, 1, 2, 3 ], // Face 0: Front (+Z in standard right-hand coords, but let's stick to your names)
            [ 5, 4, 7, 6 ], // Face 1: Back
            [ 4, 0, 6, 2 ], // Face 2: Left
            [ 1, 5, 3, 7 ], // Face 3: Right
            [ 4, 5, 0, 1 ], // Face 4: Top
            [ 2, 3, 6, 7 ]  // Face 5: Bottom
        ];

        // Let's store texture coordinates in a cleaner way.
        private static Dictionary<BlockType, Vector2[][]> _textures = new();

        public static QuantizedVertex[] GetFaceVertices(BlockType blockType, byte faceIndex, Vector3 blockWorldPos)
        {
            var vertices = new QuantizedVertex[4];

            // Get the indices of the 4 corners for the requested face
            int[] corners = FaceCorners[faceIndex];

            // Get the correct texture coordinates for this face
            Vector2[] texCoords = GetFaceTexture(blockType, faceIndex);

            // Adjust Y-position for water blocks
            if (blockType == BlockType.Water)
            {
                blockWorldPos.Y -= 0.1f;
            }

            // Create the 4 vertices
            for (int i = 0; i < 4; i++)
            {
                // Calculate final vertex position in world space
                Vector3 finalPos = blockWorldPos + CornerOffsets[corners[i]];

                // Get the lighting value (can be made more complex later)
                float light = GetLightForFace(faceIndex);

                // Create the vertex with correct WORLD position
                vertices[i] = new QuantizedVertex(finalPos, texCoords[i], light);
            }

            return vertices;
        }

        // Helper to get lighting
        private static float GetLightForFace(byte faceIndex)
        {
            // Simple lighting based on face direction
            return faceIndex switch
            {
                4 => 1.0f,  // Top
                0 => 0.9f,  // Front
                1 => 0.9f,  // Back
                2 => 0.8f,  // Left
                3 => 0.8f,  // Right
                5 => 0.7f,  // Bottom
                _ => 1.0f
            };
        }

        // Helper to select the right texture for a face
        private static Vector2[] GetFaceTexture(BlockType blockType, byte faceIndex)
        {
            Vector2[][] blockTextures = _textures[blockType];

            // Map texture to the faces
            // [0]=Side, [1]=Top, [2]=Bottom
            if (blockTextures.Length == 3)
            {
                return faceIndex switch
                {
                    4 => blockTextures[1], // Top face
                    5 => blockTextures[2], // Bottom face
                    _ => blockTextures[0]  // Side faces
                };
            }
            // [0]=Side, [1]=Top/Bottom
            if (blockTextures.Length == 2)
            {
                return faceIndex is 4 or 5 ? blockTextures[1] : blockTextures[0];
            }
            // [0]=All faces
            if (blockTextures.Length == 1)
            {
                return blockTextures[0];
            }

            // 6 unique textures
            return blockTextures[faceIndex];
        }

        public static void Load(BlockType blockType, byte[] textureAtlasIndices)
        {
            if (_textures.ContainsKey(blockType)) return;

            var texList = new List<Vector2[]>();
            foreach (var index in textureAtlasIndices)
            {
                texList.Add(GetTextureCoordinatesFromAtlas(index));
            }
            _textures.Add(blockType, [.. texList]);
        }

        private static Vector2[] GetTextureCoordinatesFromAtlas(int textureIndex, int textureWidth = 16, int textureHeight = 16, int atlasWidth = 256)
        {
            int row = textureIndex / (atlasWidth / textureWidth);
            int col = textureIndex % (atlasWidth / textureWidth);

            float uMin = col * (float)textureWidth / atlasWidth;
            float vMin = row * (float)textureHeight / atlasWidth;
            float uMax = uMin + (float)textureWidth / atlasWidth;
            float vMax = vMin + (float)textureHeight / atlasWidth;

            // Return the 4 UV coords for the corners of a quad
            return
            [
                new(uMin, vMin), // Top-Left
                new(uMax, vMin), // Top-Right
                new(uMin, vMax), // Bottom-Left
                new(uMax, vMax)  // Bottom-Right
            ];
        }
    }
}