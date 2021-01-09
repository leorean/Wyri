using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Wyri.Types;

namespace Wyri.Objects.Levels
{
    public class Room : SpatialObject, IStayActive
    {
        readonly List<Object> objects;
        public List<Object> Objects => objects.ToList();

        public int Width => (int)BBox.w;
        public int Height => (int)BBox.h;

        public bool SwitchState { get; set; } = false;
        public int Background { get; set; } = -1;
        public int Weather { get; set; } = -1;
        public float Darkness { get; set; } = -1;

        public Room(int x, int y, int width, int height) : base(new Vector2(x, y), new RectF(0, 0, width, height))
        {
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

        public override void Destroy()
        {
            foreach(var o in objects.ToList())
            {
                o.Destroy();
            }
            base.Destroy();
        }

        public override void Draw(SpriteBatch sb) { }
        
        public override void Update() { }
    }
}
