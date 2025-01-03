using System;
using System.Reflection.Metadata.Ecma335;

namespace MineDirt.Src;

public class Noise
{
    private readonly int seed;
    private readonly int[] permutation;
    private readonly int[] p;
    private readonly float scale = 0.01f;

    public Noise(int seed)
    {
        // Generate permutation table based on the seed
        Random random = new(seed);
        permutation = new int[256];
        for (int i = 0; i < 256; i++)
            permutation[i] = i;

        // Shuffle the permutation table
        for (int i = 255; i > 0; i--)
        {
            int swapIndex = random.Next(i + 1);
            (permutation[i], permutation[swapIndex]) = (permutation[swapIndex], permutation[i]);
        }

        // Duplicate the table for ease of access
        p = new int[512];
        for (int i = 0; i < 512; i++)
            p[i] = permutation[i % 256];
    }

    public float Generate(float x, float z)
    {
        // Adjust the scale
        x *= scale;
        z *= scale;

        // Find unit grid cell containing the point
        int X = (int)Math.Floor(x) & 255;
        int Z = (int)Math.Floor(z) & 255;

        // Relative x, z of the point within the cell
        x -= (float)Math.Floor(x);
        z -= (float)Math.Floor(z);

        // Compute fade curves for x, z
        float u = Fade(x);
        float v = Fade(z);

        // Hash coordinates of the 4 cube corners
        int aa = p[p[X] + Z];
        int ab = p[p[X] + Z + 1];
        int ba = p[p[X + 1] + Z];
        int bb = p[p[X + 1] + Z + 1];

        // Add blended results from 4 corners of the cube
        float result = Lerp(v,
                            Lerp(u, Grad(aa, x, z), Grad(ba, x - 1, z)),
                            Lerp(u, Grad(ab, x, z - 1), Grad(bb, x - 1, z - 1)));

        // Scale result to [0, 1]
        return (result + 1.0f) / 2.0f;
    }

    private static float Fade(float t)
    {
        // Fade function to smooth the interpolation
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private static float Lerp(float t, float a, float b)
    {
        // Linear interpolation
        return a + t * (b - a);
    }

    private static float Grad(int hash, float x, float z)
    {
        // Calculate gradient based on hash value
        int h = hash & 15;
        float u = h < 8 ? x : z;
        float v = h < 4 ? z : h == 12 || h == 14 ? x : 0;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }
}