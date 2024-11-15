﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Types;

namespace Wyri.Objects
{
    public abstract class SpatialObject : Object
    {
        public string ID
        {
            get
            {
                return $"{GetType().Name}_{Math.Floor(originalPosition.X)}_{Math.Floor(originalPosition.Y)}";
            }
        }

        private Vector2 originalPosition;

        public Vector2 Position { get; set; }
        public RectF BBox { get; set; }

        public Vector2 Center { get => new Vector2((Left + Right) *.5f, (Top + Bottom) * .5f); }
        
        public float X
        {
            get { return Position.X; }
            set { Position = new Vector2(value, Y); }
        }
        public float Y
        {
            get { return Position.Y; }
            set { Position = new Vector2(X, value); }
        }
        public float Left => X + BBox.x;
        public float Top => Y + BBox.y;
        public float Right => X + BBox.x + BBox.w;
        public float Bottom => Y + BBox.y + BBox.h;

        public SpatialObject(Vector2 position, RectF boundingBox)
        {
            originalPosition = position;
            Position = position;
            BBox = boundingBox;        
        }
    }
}
