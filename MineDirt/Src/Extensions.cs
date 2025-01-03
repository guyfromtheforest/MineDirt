using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineDirt.Src;
public static class Extensions
{
    public static Vector3 ToChunkPosition(this Vector3 position) => new(
            (int)Math.Floor(position.X / Subchunk.Size) * Subchunk.Size,
            0,
            (int)Math.Floor(position.Z / Subchunk.Size) * Subchunk.Size
        );

    public static Vector3 ToSubchunkPosition(this Vector3 position) => new(
            (int)Math.Floor(position.X / Subchunk.Size) * Subchunk.Size,
            (int)Math.Floor(position.Y / Subchunk.Size) * Subchunk.Size,
            (int)Math.Floor(position.Z / Subchunk.Size) * Subchunk.Size
        );

}
