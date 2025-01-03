using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineDirt.Src;
public class Block
{
    public byte[] Indices => indices;
    public BlockType BlockType { get; private set; }
    public Vector3 Position { get; private set; }

    public static byte[] indices =
    [
        // Front face
        0, 1, 2, 2, 1, 3,
        // Back face
        4, 6, 5, 5, 6, 7,
        // Left face
        8, 9, 10, 10, 9, 11,
        // Right face
        12, 13, 14, 14, 13, 15,
        // Top face
        16, 17, 18, 18, 17, 19,
        // Bottom face
        20, 21, 22, 22, 21, 23
    ];

    public static Dictionary<BlockType, Vector2[][]> textures = [];

    public VertexPositionTexture[] Vertices;

    // [0] = Front, Back, Left, Right, Top, Bottom
    // [0] = Front, Back, Left, Right, [1] = Top, Bottom
    // [0] = Front, Back, Left, Right, [1] = Top, [2] = Bottom
    // [0] = Front, [1] = Back, [2] = Left, [3] = Right, [4] = Top, [5] = Bottom]
    public Block(BlockType blockType, byte[] textureAtlasIndices, Vector3 pos)
    {
        BlockType = blockType;
        Position = pos;

        if (!textures.ContainsKey(blockType))
        {
            // Get texture coordinates for each face (front, back, left, right, top, bottom)
            if (textureAtlasIndices.Length == 1)
                textures.Add(blockType,
                [
                    GetTextureCoordinates(textureAtlasIndices[0]), // Front face
                    GetTextureCoordinates(textureAtlasIndices[0]), // Back face
                    GetTextureCoordinates(textureAtlasIndices[0]), // Left face
                    GetTextureCoordinates(textureAtlasIndices[0]), // Right face
                    GetTextureCoordinates(textureAtlasIndices[0]), // Top face
                    GetTextureCoordinates(textureAtlasIndices[0])  // Bottom face
                ]);

            if (textureAtlasIndices.Length == 2)
                textures.Add(blockType,
                [
                    GetTextureCoordinates(textureAtlasIndices[0]), // Front face
                    GetTextureCoordinates(textureAtlasIndices[0]), // Back face
                    GetTextureCoordinates(textureAtlasIndices[0]), // Left face
                    GetTextureCoordinates(textureAtlasIndices[0]), // Right face
                    GetTextureCoordinates(textureAtlasIndices[1]), // Top face
                    GetTextureCoordinates(textureAtlasIndices[1])  // Bottom face
                ]);

            if (textureAtlasIndices.Length == 3)
                textures.Add(blockType,
                [
                    GetTextureCoordinates(textureAtlasIndices[0]), // Front face
                    GetTextureCoordinates(textureAtlasIndices[0]), // Back face
                    GetTextureCoordinates(textureAtlasIndices[0]), // Left face
                    GetTextureCoordinates(textureAtlasIndices[0]), // Right face
                    GetTextureCoordinates(textureAtlasIndices[1]), // Top face
                    GetTextureCoordinates(textureAtlasIndices[2])  // Bottom face
                ]);

            if (textureAtlasIndices.Length == 6)
                textures.Add(blockType,
                [
                    GetTextureCoordinates(textureAtlasIndices[0]), // Front face
                    GetTextureCoordinates(textureAtlasIndices[1]), // Back face
                    GetTextureCoordinates(textureAtlasIndices[2]), // Left face
                    GetTextureCoordinates(textureAtlasIndices[3]), // Right face
                    GetTextureCoordinates(textureAtlasIndices[4]), // Top face
                    GetTextureCoordinates(textureAtlasIndices[5])  // Bottom face
                ]);
        }

        Vertices = [
            // Front face (using the side texture)
            new VertexPositionTexture(new Vector3(Position.X - 0.5f, Position.Y + 0.5f, Position.Z - 0.5f), textures[BlockType][0][0]), // top-left
            new VertexPositionTexture(new Vector3(Position.X + 0.5f, Position.Y + 0.5f, Position.Z - 0.5f), textures[BlockType][0][1]),  // top-right
            new VertexPositionTexture(new Vector3(Position.X - 0.5f, Position.Y - 0.5f, Position.Z - 0.5f), textures[BlockType][0][2]), // bottom-left
            new VertexPositionTexture(new Vector3(Position.X + 0.5f, Position.Y - 0.5f, Position.Z - 0.5f), textures[BlockType][0][3]),  // bottom-right

            // Back face (using the side texture)
            new VertexPositionTexture(new Vector3(Position.X - 0.5f, Position.Y + 0.5f, Position.Z + 0.5f), textures[BlockType][1][0]),  // top-left
            new VertexPositionTexture(new Vector3(Position.X + 0.5f, Position.Y + 0.5f, Position.Z + 0.5f), textures[BlockType][1][1]),   // top-right
            new VertexPositionTexture(new Vector3(Position.X - 0.5f, Position.Y - 0.5f, Position.Z + 0.5f), textures[BlockType][1][2]),  // bottom-left
            new VertexPositionTexture(new Vector3(Position.X + 0.5f, Position.Y - 0.5f, Position.Z + 0.5f), textures[BlockType][1][3]),   // bottom-right

            // Left face (using the side texture)
            new VertexPositionTexture(new Vector3(Position.X - 0.5f, Position.Y + 0.5f, Position.Z - 0.5f), textures[BlockType][2][0]),  // top-left
            new VertexPositionTexture(new Vector3(Position.X - 0.5f, Position.Y + 0.5f, Position.Z + 0.5f), textures[BlockType][2][1]),   // top-right
            new VertexPositionTexture(new Vector3(Position.X - 0.5f, Position.Y - 0.5f, Position.Z - 0.5f), textures[BlockType][2][2]),  // bottom-left
            new VertexPositionTexture(new Vector3(Position.X - 0.5f, Position.Y - 0.5f, Position.Z + 0.5f), textures[BlockType][2][3]),   // bottom-right

            // Right face (using the side texture)
            new VertexPositionTexture(new Vector3(Position.X + 0.5f, Position.Y + 0.5f, Position.Z - 0.5f), textures[BlockType][3][0]),  // top-left
            new VertexPositionTexture(new Vector3(Position.X + 0.5f, Position.Y + 0.5f, Position.Z + 0.5f), textures[BlockType][3][1]),   // top-right
            new VertexPositionTexture(new Vector3(Position.X + 0.5f, Position.Y - 0.5f, Position.Z - 0.5f), textures[BlockType][3][2]),  // bottom-left
            new VertexPositionTexture(new Vector3(Position.X + 0.5f, Position.Y - 0.5f, Position.Z + 0.5f), textures[BlockType][3][3]),   // bottom-right

            // Top face (using the top texture)
            new VertexPositionTexture(new Vector3(Position.X - 0.5f, Position.Y + 0.5f, Position.Z - 0.5f), textures[BlockType][4][0]),  // top-left
            new VertexPositionTexture(new Vector3(Position.X + 0.5f, Position.Y + 0.5f, Position.Z - 0.5f), textures[BlockType][4][1]),   // top-right
            new VertexPositionTexture(new Vector3(Position.X - 0.5f, Position.Y + 0.5f, Position.Z + 0.5f), textures[BlockType][4][2]),   // bottom-left
            new VertexPositionTexture(new Vector3(Position.X + 0.5f, Position.Y + 0.5f, Position.Z + 0.5f), textures[BlockType][4][3]),    // bottom-right

            // Bottom face (using the bottom texture)
            new VertexPositionTexture(new Vector3(Position.X - 0.5f, Position.Y - 0.5f, Position.Z - 0.5f), textures[BlockType][5][0]), // top-left
            new VertexPositionTexture(new Vector3(Position.X + 0.5f, Position.Y - 0.5f, Position.Z - 0.5f), textures[BlockType][5][1]),  // top-right
            new VertexPositionTexture(new Vector3(Position.X - 0.5f, Position.Y - 0.5f, Position.Z + 0.5f), textures[BlockType][5][2]),  // bottom-left
            new VertexPositionTexture(new Vector3(Position.X + 0.5f, Position.Y - 0.5f, Position.Z + 0.5f), textures[BlockType][5][3])    // bottom-right
        ];
    }

    public static Vector2[] GetTextureCoordinates(int textureIndex, int textureWidth = 16, int textureHeight = 16, int atlasWidth = 256)
    {
        // Calculate the texture coordinates based on the index and size
        int row = textureIndex / (atlasWidth / textureWidth);
        int col = textureIndex % (atlasWidth / textureWidth);

        float uMin = col * textureWidth / (float)atlasWidth;
        float vMin = row * textureHeight / (float)atlasWidth;
        float uMax = uMin + textureWidth / (float)atlasWidth;
        float vMax = vMin + textureHeight / (float)atlasWidth;

        return
        [
            new Vector2(uMin, vMin), // Top-left
            new Vector2(uMax, vMin), // Top-right
            new Vector2(uMin, vMax), // Bottom-left
            new Vector2(uMax, vMax), // Bottom-right
        ];
    }
}
