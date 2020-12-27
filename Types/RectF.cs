using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wyri.Types
{
    public struct RectF
    {
        public readonly float x;
        public readonly float y;
        public readonly float w;
        public readonly float h;

        public RectF(float x, float y, float w, float h)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
        }

        public override bool Equals(object obj)
        {
            if (obj is RectF other)
            {
                return other.x == x && other.y == y & other.w == w & other.h == h;
            }
            return false;
        }

        public bool Intersects(RectF b)
        {
            /*
            return (Math.Abs(x - b.x) * 2 < (w + b.w)) &&
                (Math.Abs(y - b.y) * 2 < (h + b.h));
            */
            return (Math.Abs((x + w / 2) - (b.x + b.w / 2)) * 2 < (w + b.w)) &&
                (Math.Abs((y + h / 2) - (b.y + b.h / 2)) * 2 < (h + b.h));
        }

        public override int GetHashCode() => base.GetHashCode();

        public static implicit operator Rectangle(RectF f) => new Rectangle((int)f.x, (int)f.y, (int)f.w, (int)f.h);
        public static explicit operator RectF(Rectangle f) => new RectF(f.X, f.Y, f.Width, f.Height);

        public static RectF operator+ (RectF a, RectF b)
        {
            return new RectF(a.x + b.x, a.y + b.y, a.w + b.w, a.h + b.h);
        }
        public static RectF operator +(RectF a, Vector2 b)
        {
            return new RectF(a.x + b.X, a.y + b.Y, a.w, a.h);
        }
        public static RectF operator +(Vector2 b, RectF a)
        {
            return new RectF(a.x + b.X, a.y + b.Y, a.w, a.h);
        }
        public static RectF operator -(RectF a, RectF b)
        {
            return new RectF(a.x - b.x, a.y - b.y, a.w - b.w, a.h - b.h);
        }
        public static RectF operator -(RectF a, Vector2 b)
        {
            return new RectF(a.x - b.X, a.y - b.Y, a.w, a.h);
        }
        public static RectF operator -(Vector2 b, RectF a)
        {
            return new RectF(a.x - b.X, a.y - b.Y, a.w, a.h);
        }
    }
}
