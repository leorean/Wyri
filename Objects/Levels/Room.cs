using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Wyri.Objects.Levels
{
    public class Room : SpatialObject
    {
        readonly List<Object> objects;

        public int Width => BoundingBox.Width;
        public int Height => BoundingBox.Height;

        public bool SwitchState { get; set; } = false;

        public Room(int x, int y, int width, int height) : base(new Vector2(x, y), new Rectangle(0, 0, width, height))
        {
            Offset = new Vector2(width * .5f, height * .5f);
            objects = new List<Object>();
        }

        public void AddObject(RoomObject roomObject)
        {
            if (!objects.Contains(roomObject))
                objects.Add(roomObject);
        }

        public void RemoveObject(RoomObject roomObject)
        {
            objects.Remove(roomObject);
        }

        public override void Draw(SpriteBatch sb)
        {
            //
        }

        public override void Update()
        {
            //
        }

        ~Room()
        {
            foreach(var o in objects)
            {
                o.Destroy();
            }
            objects.Clear();
        }
    }
}
