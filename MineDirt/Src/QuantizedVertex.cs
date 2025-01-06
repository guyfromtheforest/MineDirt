using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineDirt.Src;
public struct QuantizedVertex : IVertexType
{
    // public byte X, Y, Z;       // Position as bytes (3 bytes)
    // public byte Padding;       // Padding to align to 4 bytes
    //public byte U, V;          // UV as bytes (2 bytes)
    //public byte Unused1, Unused2; // Additional padding or reserved bytes
    Vector3 Position; 
    // uint Position; 
    Vector2 UV; 

    public QuantizedVertex(Vector3 position, Vector2 uv)
    {
        //Vector3 subchunkPos = new(
        //    (int)(position.X % Subchunk.Size),
        //    (int)(position.Y % Subchunk.Size),
        //    (int)(position.Z % Subchunk.Size)
        //);

        //Position = (uint)(
        //    ((uint)subchunkPos.X << 10) |
        //    ((uint)subchunkPos.Y << 5) |
        //    ((uint)subchunkPos.Z)
        //);

        //X = (byte)(position.X % Subchunk.Size);
        //Y = (byte)(position.Y % Subchunk.Size);
        //Z = (byte)(position.Z % Subchunk.Size);

        //X = (byte)(position.X % Subchunk.Size);
        //Y = (byte)(position.Y % Subchunk.Size);
        //Z = (byte)(position.Z % Subchunk.Size);
        //Padding = 0; // Unused padding
        //U = (byte)(uv.X);
        //V = (byte)(uv.Y);
        //Unused1 = 0;
        //Unused2 = 0;

        this.Position = position;
        this.UV = uv;
     }

    //public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
    //    new VertexElement(0, VertexElementFormat.Byte4, VertexElementUsage.Position, 0), // Position (X, Y, Z, Padding)
    //    new VertexElement(4, VertexElementFormat.Byte4, VertexElementUsage.TextureCoordinate, 0) // UV (U, V, Unused1, Unused2)
    //);

    public static readonly VertexDeclaration VertexDeclaration = new(
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
    );

    //public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
    //    new VertexElement(0, VertexElementFormat.Color, VertexElementUsage.Position, 0),
    //    new VertexElement(4, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
    //);

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
}