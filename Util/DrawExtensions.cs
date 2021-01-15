using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wyri.Util
{
    public static class DrawExtensions
    {
        public static Color GetPixel(this Color[] colors, int x, int y, int width)
        {
            return colors[x + (y * width)];
        }
        public static Color[] GetPixels(this Texture2D texture)
        {
            Color[] colors1D = new Color[texture.Width * texture.Height];
            texture.GetData(colors1D);
            return colors1D;
        }
    }
}
