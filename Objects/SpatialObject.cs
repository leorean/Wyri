using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Types;

namespace Wyri.Objects
{
    public abstract class SpatialObject : Object
    {
        public Vector2 Position { get; set; }
        public Rectangle BoundingBox { get; set; }
        
        public Vector2 Offset { get; set; }

        public float X => Position.X;
        public float Y => Position.Y;
        public float Left => Position.X - BoundingBox.Width * .5f + Offset.X;
        public float Top => Position.Y - BoundingBox.Height * .5f + Offset.Y;
        public float Right => Position.X + BoundingBox.Width * .5f + Offset.X;
        public float Bottom => Position.Y + BoundingBox.Height * .5f + Offset.Y;

        public SpatialObject(Vector2 position, Rectangle boundingBox)
        {
            Position = position;
            BoundingBox = boundingBox;            
        }
    }
}
