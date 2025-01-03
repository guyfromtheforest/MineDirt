using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineDirt.Src;

public enum BlockType
{
    Dirt,
    Grass,
    Cobblestone,
    Bedrock,
    Stone
}   

public static class Blocks
{
    // [0] = Front, Back, Left, Right, Top, Bottom
    // [0] = Front, Back, Left, Right, [1] = Top, Bottom
    // [0] = Front, Back, Left, Right, [1] = Top, [2] = Bottom
    // [0] = Front, [1] = Back, [2] = Left, [3] = Right, [4] = Top, [5] = Bottom]

    public static Block Dirt(Vector3 pos) => new(BlockType.Dirt, [2], pos);
    public static Block Grass(Vector3 pos) => new(BlockType.Grass, [1, 0, 2], pos);
    public static Block Cobblestone(Vector3 pos) => new(BlockType.Cobblestone, [3], pos);
    public static Block Bedrock(Vector3 pos) => new(BlockType.Bedrock, [4], pos);
    public static Block Stone(Vector3 pos) => new(BlockType.Stone, [5], pos);
}
