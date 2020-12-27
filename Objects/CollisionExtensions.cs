using System;
using System.Collections.Generic;
using System.Text;

namespace Wyri.Objects
{
    public static class CollisionExtensions
    {
        public static List<T> CollisionBounds<T>(this SpatialObject self, int offX = 0, int offY = 0) where T : SpatialObject
        {
            var detected = new List<T>();

            foreach (var o in ObjectController.FindActive<T>())
            {
                if (o == self)
                    continue;

                if (((self.Right + offX) >= o.Left || (self.Left + offX) <= o.Right)
                    && ((self.Bottom + offY) >= o.Top || (self.Top + offY) <= o.Bottom))
                {
                    detected.Add(o);
                }
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
                if (o.Left >= x && o.Right <= x
                    && o.Top >= y && o.Bottom <= y)
                {
                    detected.Add(o);
                }
            }

            return detected;
        }

        /*public static bool CollisionPoint<T>(this SpatialObject self, T other, float x, float y) where T : SpatialObject
        {
            if ((x >= other.Left || x <= other.Right)
                && (y >= other.Top || y <= other.Bottom))
            {
                return true;
            }

            return false;
        }*/
    }
}
