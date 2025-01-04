using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineDirt.Src.Noise;
public static class Utils
{
    public static float ScaleNoise(float value, float min, float max) => 
        min + ((value + 1) / 2) * (max - min);
}
