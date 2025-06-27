using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;

namespace MineDirt.Src;

public struct QuantizedVertex : IVertexType
{
    public Vector2 UV;
    public float PackedPosition;

    public QuantizedVertex(Vector3 blockPos, Vector2 uv, int lightLevel, int cornerID)
    {
        UV = uv;

        int bx = (int)blockPos.X & 0xF;    // 4 bits: 0–15
        int bz = (int)blockPos.Z & 0xF;    // 4 bits: 0–15
        cornerID = cornerID & 0x7;         // 3 bits: 0–7
        int by = (int)blockPos.Y & 0xFF;   // 8 bits: 0–255
        lightLevel = lightLevel & 0xF;     // 4 bits: 0–15

        int packed =
              bx
            | (bz << 4)
            | (cornerID << 8)
            | (by << 11)
            | (lightLevel << 19);

        PackedPosition = (float)packed;
    }

    public static readonly VertexDeclaration VertexDeclaration = new(
        new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
        new VertexElement(8, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1) // Use COLOR1 semantic
    );

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
}