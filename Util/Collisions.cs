using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Objects;
using Wyri.Types;

namespace Wyri
{
    public static class Collisions
    {
        public static List<T> CollisionBounds<T>(this SpatialObject self, int offX = 0, int offY = 0) where T : SpatialObject
        {
            var detected = new List<T>();

            foreach (var o in ObjectController.FindActive<T>())
            {
                if (o == self)
                    continue;
                
                if ((o.Position + o.BBox).Intersects(self.Position + self.BBox + new Vector2(offX, offY)))
                    detected.Add(o);
            }

            return detected;
        }

        public static bool CollisionBounds<T>(this SpatialObject self, T other, int offX = 0, int offY = 0) where T : SpatialObject
        {
            if (((self.Right + offX) >= other.Left || (self.Left + offX) <= other.Right)
                    && ((self.Bottom + offY) >= other.Top || (self.Top + offY) <= other.Bottom))
            {
                return true;
            }
            return false;
        }

        public static List<T> CollisionPoint<T>(this SpatialObject self, float x, float y) where T : SpatialObject
        {
            var detected = new List<T>();

            foreach (var o in ObjectController.FindActive<T>())
            {
                if (o == self)
                    continue;

                if (x >= o.Left && x <= o.Right
                    && y >= o.Top && y <= o.Bottom)
                {
                    detected.Add(o);
                }
            }

            return detected;
        }

        public static List<T> CollisionPoint<T>(float x, float y) where T : SpatialObject
        {
            var detected = new List<T>();

            foreach (var o in ObjectController.FindActive<T>())
            {
                if (o.Right >= x && o.Left <= x
                    && o.Bottom >= y && o.Top <= y)
                {
                    detected.Add(o);
                }
            }

            return detected;
        }

        public static Tile TileAt(float x, float y, string layer)
        {
            var grid = MainGame.Map.LayerData[layer];

            var tx = M.Div(x, G.T);
            var ty = M.Div(y, G.T);

            return grid[tx, ty];
        }

        public static bool CollisionSolidTile(this SpatialObject o, float x, float y)
        {
            var grid = MainGame.Map.LayerData["FG"];

            for (float i = M.Div(o.Left, G.T) - G.T; i < M.Div(o.Right, G.T) + G.T; i++)
            {
                for (float j = M.Div(o.Top, G.T) - G.T; j < M.Div(o.Bottom, G.T) + G.T; j++)
                {
                    var t = grid[(int)i, (int)j];
                    if (t == null || !t.IsSolid)
                        continue;

                    var tileRect = new RectF(i * G.T, j * G.T, G.T, G.T);
                    if ((o.BBox + new Vector2(o.X + x, o.Y + y)).Intersects(tileRect))
                        return true;
                }
            }

            return false;
        }
    }
}
