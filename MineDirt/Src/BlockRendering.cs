using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MineDirt.Src
{
    public static class BlockRendering
    {
        public static readonly byte[] Indices = [0, 2, 3, 0, 3, 1];

        private static readonly int[][] FaceCorners =
        [
            [2,3,0,1], // Front 
            [7,6,5,4], // Back
            [6,2,4,0], // Left
            [3,7,1,5], // Right
            [6,7,2,3], // Top
            [0,1,4,5], // Bottom
        ];

        private static Dictionary<BlockType, int[]> _textures = new();

        public static QuantizedVertex[] GetFaceVertices(BlockType blockType, byte faceIndex, Vector3 blockLocalPos)
        {
            var vertices = new QuantizedVertex[4];
            int[] corners = FaceCorners[faceIndex];
            int textureIndex = GetFaceTextureIndex(blockType, faceIndex);
            
            for (int i = 0; i < 4; i++)
            {
                Vector3 finalPos = blockLocalPos; 
                int light = GetLightForFace(faceIndex);

                vertices[i] = new QuantizedVertex(finalPos, textureIndex, light, corners[i], faceIndex);
            }

            return vertices;
        }

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

        private static int GetFaceTextureIndex(BlockType blockType, byte faceIndex)
        {
            int[] blockTextures = _textures[blockType];

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
            _textures.Add(blockType, [.. textureAtlasIndices]);
        }
    }
}