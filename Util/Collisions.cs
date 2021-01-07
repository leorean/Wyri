using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wyri.Objects;
using Wyri.Types;

namespace Wyri
{
    public static class Collisions
    {
        public static List<T> CollisionBounds<T>(this SpatialObject self, float offX = 0, float offY = 0) where T : SpatialObject
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

        public static bool CollisionBounds(this SpatialObject self, SpatialObject other, float offX = 0, float offY = 0)
        {
            return (other.Position + other.BBox).Intersects(self.Position + self.BBox + new Vector2(offX, offY));
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

        public static bool CollisionPoint(this SpatialObject self, SpatialObject other, float x, float y)
        {
            if (M.In(x, other.Left, other.Right)
                    && M.In(y, other.Top, other.Bottom))
                return true;

            return false;
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

        public static double LengthDirX(double degAngle)
        {
            var rad = (degAngle / 360f) * 2 * Math.PI;
            return Math.Cos(rad);
        }

        public static double LengthDirY(double degAngle)
        {
            var rad = (degAngle / 360f) * 2 * Math.PI;
            return Math.Sin(rad);
        }

        public static (T, float) RayCast<T>(this SpatialObject obj, float degAngle, float n = 1, float maxDist = 9999) where T : SpatialObject
        {
            var pos = obj.Center;
            var kx = M.LengthDirX(degAngle);
            var ky = M.LengthDirY(degAngle);

            for (float i = 0; i < maxDist; i += n)
            {
                var t = TileAt(pos.X + kx * i, pos.Y + ky * i, "FG");
                if (t != null || t.IsSolid)
                {
                    return (default(T), i);
                }

                var collision = obj.CollisionBounds<T>(pos.X + kx * i, pos.Y + ky * i).FirstOrDefault();
                if (collision != null)
                {
                    return (collision, i);
                }                
            }
            return (default(T), 0);
        }

        public static (bool, float) RayCast(this SpatialObject obj, SpatialObject other, float degAngle, float n = 1, float maxDist = 9999)
        {
            var pos = obj.Center;
            var kx = M.LengthDirX(degAngle);
            var ky = M.LengthDirY(degAngle);

            for (float i = 0; i < maxDist; i += n)
            {
                var t = TileAt(pos.X + kx * i, pos.Y + ky * i, "FG");
                if (t != null && t.IsSolid)
                {
                    return (false, i);
                }

                if (obj.CollisionPoint(other, pos.X + kx * i, pos.Y + ky * i))
                {
                    return (true, i);
                }
            }

            return (false, 0);
        }

        public static (SpatialObject, float) RayCast(this SpatialObject obj, int degAngle, float n = 1, float maxDist = 9999) => RayCast<SpatialObject>(obj, degAngle, n, maxDist);

        public static (bool, float) RayCastTile(this SpatialObject obj, int degAngle, float n = 1, float maxDist = 9999)
        {
            var pos = obj.Center;
            var kx = M.LengthDirX(degAngle);
            var ky = M.LengthDirY(degAngle);

            for (float i = 0; i < maxDist; i += n)
            {
                var t = TileAt(pos.X + kx * i, pos.Y + ky * i, "FG");                
                if (t != null && t.IsSolid)
                {
                    return (true, i);
                }
            }
            return (false, 0);
        }

        public static Tile TileAt(float x, float y, string layer)
        {
            var grid = MainGame.Map.LayerData[layer];

            var tx = M.Div(x, G.T);
            var ty = M.Div(y, G.T);

            return grid[tx, ty];
        }

        public static bool CollisionSolidTile(this SpatialObject o, float offX, float offY, bool includePlatformTiles = false)
        {
            var grid = MainGame.Map.LayerData["FG"];

            for (float i = M.Div(o.Left + offX, G.T) - G.T; i < M.Div(o.Right + offX, G.T) + G.T; i++)
            {
                for (float j = M.Div(o.Top + offX, G.T) - G.T; j < M.Div(o.Bottom + offX, G.T) + G.T; j++)
                {
                    var t = grid[(int)i, (int)j];
                    if (t == null)
                        continue;

                    if (includePlatformTiles)
                    {
                        if (t.Type != TileType.Platform && !t.IsSolid)
                            continue;
                    }
                    else
                    {
                        if (!t.IsSolid)
                            continue;
                    }

                    var tileRect = new RectF(i * G.T, j * G.T, G.T, G.T);
                    if ((o.BBox + new Vector2(o.X + offX, o.Y + offY)).Intersects(tileRect))
                        return true;
                }
            }

            return false;
        }
    }
}
