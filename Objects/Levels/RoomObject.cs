using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wyri.Objects.Levels
{
    public abstract class RoomObject : SpatialObject
    {
        public Room Room { get; }
        
        public RoomObject (Vector2 position, Rectangle boundingBox, Room room) : base(position, boundingBox)
        {
            Room = room;
            room.AddObject(this);
        }

        ~RoomObject()
        {
            Room.RemoveObject(this);            
        }
    }
}
