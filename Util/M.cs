using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wyri
{
    public static class M
    {
        public static float Sin(float x) => (float)Math.Sin(x);
        
        public static float Cos(float x) => (float)Math.Cos(x);

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

        public static bool In(this float x, float min, float max)
        {
            return x >= min && x <= max;
        }

        public static double Euclidean(Point p1, Point p2) => Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));

        public static float Euclidean(Vector2 p1, Vector2 p2) => (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));

        public static double Euclidean(float p1x, float p1y, float p2x, float p2y) => Math.Sqrt(Math.Pow(p1x - p2x, 2) + Math.Pow(p1y - p2y, 2));

        public static float DegToRad(float degAngle)
        {
            return (float)Math.PI * degAngle / 180.0f;
        }

        public static float RadToDeg(float radAngle)
        {
            return (float)(radAngle * (180.0f / Math.PI));
        }

        public static float LengthDirX(float degAngle)
        {
            var rad = (degAngle / 360f) * 2 * Math.PI;
            return (float)Math.Cos(rad);
        }

        public static float LengthDirY(float degAngle)
        {
            var rad = (degAngle / 360f) * 2 * Math.PI;
            return (float)Math.Sin(rad);
        }

        public static float VectorToAngle(this Vector2 vector, bool inRadiant = false)
        {
            var rad = (float)Math.Atan2(vector.Y, vector.X);

            if (inRadiant)
                return rad;

            return RadToDeg(rad);
        }
    }
}
