using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wyri
{
    public static class M
    {
        public static int Div(double v1, double v2)
        {
            return Floor(v1 / v2);
        }

        public static int Floor<T>(T value)
        {
            return (int)System.Math.Floor((double)(object)value);
        }

        public static float Clamp(float value, float min, float max)
        {
            return Math.Min(Math.Max(value, min), max);
        }
    }
}
