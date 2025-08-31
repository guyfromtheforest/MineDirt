using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;

namespace MineDirt.Src;

public struct QuantizedVertex : IVertexType
{
    public float Packed0;
    public float Packed1;
    public float Packed2; //used only for AO for now, in future it can be used for lighting

    public QuantizedVertex(Vector3 blockPos, int textureIndex, int lightLevel, int cornerID, int faceIndex, float AO = 0f)
    {
        int packedUV = (textureIndex << 3) | faceIndex;

        Packed0 = packedUV;

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

        Packed1 = (float)packed;
        Packed2 = AO;
    }

    public static readonly VertexDeclaration VertexDeclaration = new(
        new VertexElement(0, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 0),
        new VertexElement(4, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1),
        new VertexElement(8, VertexElementFormat.Single, VertexElementUsage.Color, 0)
    );

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
}