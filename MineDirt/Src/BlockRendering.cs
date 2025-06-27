using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MineDirt.Src
{
    public static class BlockRendering
    {
        public static readonly byte[] Indices = [0, 2, 3, 0, 3, 1];

        private static readonly int[][] FaceCorners =
        [
            [2,3,0,1],
            [7,6,5,4],
            [6,2,4,0],
            [3,7,1,5],
            [6,7,2,3],
            [0,1,4,5],
        ];

        private static Dictionary<BlockType, Vector2[][]> _textures = new();

        public static QuantizedVertex[] GetFaceVertices(BlockType blockType, byte faceIndex, Vector3 blockLocalPos)
        {
            var vertices = new QuantizedVertex[4];
            int[] corners = FaceCorners[faceIndex];
            Vector2[] texCoords = GetFaceTexture(blockType, faceIndex);

            //// Adjust Y-position for water blocks
            //if (blockType == BlockType.Water)
            //{
            //    blockLocalPos.Y -= 0.1f;
            //}

            // Create the 4 vertices
            for (int i = 0; i < 4; i++)
            {
                Vector3 finalPos = blockLocalPos; 
                int light = GetLightForFace(faceIndex);
                vertices[i] = new QuantizedVertex(finalPos, texCoords[i], light, corners[i]);
            }

            return vertices;
        }

        // Helper to get lighting
        private static int GetLightForFace(byte faceIndex)
        {
            // Simple lighting based on face direction
            return faceIndex switch
            {
                4 => 15,  // Top
                0 => 13,  // Front
                1 => 13,  // Back
                2 => 11,  // Left
                3 => 11,  // Right
                5 => 8,  // Bottom
                _ => 1
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