using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Types;

namespace Wyri.Objects.Levels
{
    public abstract class RoomObject : SpatialObject
    {
        public Room Room { get; }
        
        public RoomObject (Vector2 position, RectF boundingBox, Room room) : base(position, boundingBox)
        {
            Room = room;
            room.AddObject(this);
        }

        public override void Destroy()
        {
            Room.RemoveObject(this);
            base.Destroy();
        }
    }
}
