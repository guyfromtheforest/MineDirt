using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineDirt.Src;
public static class Blocks
{
    // [0] = Front, Back, Left, Right, Top, Bottom
    // [0] = Front, Back, Left, Right, [1] = Top, Bottom
    // [0] = Front, Back, Left, Right, [1] = Top, [2] = Bottom
    // [0] = Front, [1] = Back, [2] = Left, [3] = Right, [4] = Top, [5] = Bottom]

    public static Block Dirt(Vector3 pos) => new(pos, [2]);
    public static Block Grass(Vector3 pos) => new(pos, [1, 0, 2]);
    public static Block Cobblestone(Vector3 pos) => new(pos, [3]);
    public static Block Bedrock(Vector3 pos) => new(pos, [4]);
    public static Block Stone(Vector3 pos) => new(pos, [5]);
}
