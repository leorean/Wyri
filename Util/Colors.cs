using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wyri.Util
{
    public static class Colors
    {
        public static string ToHex(this Color c)
        {
            return c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        public static Color FromHex(string hexCode)
        {
            hexCode = hexCode.Replace("#", "");

            try
            {
                if (hexCode.Length == 6)
                {

                    var r = int.Parse(hexCode.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    var g = int.Parse(hexCode.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                    var b = int.Parse(hexCode.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

                    return new Color(r, g, b);
                }

                if (hexCode.Length == 8)
                {
                    var a = int.Parse(hexCode.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    var r = int.Parse(hexCode.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                    var g = int.Parse(hexCode.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                    var b = int.Parse(hexCode.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                    return new Color(r, g, b, a);
                }
            }
            catch (Exception) { }
            throw new ArgumentException($"Color code '{hexCode}' is invalid!");
        }
    }
}
