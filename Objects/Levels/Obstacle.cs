using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Types;

namespace Wyri.Objects.Levels
{
    public abstract class Obstacle : RoomObject
    {
        public Obstacle(Vector2 position, RectF boundingBox, Room room) : base(position, boundingBox, room)
        {

        }

        public override void Draw(SpriteBatch sb)
        {
            //sb.DrawRectangle(Position + BBox, Color.Red, false, .8f);
        }
        public override void Update() { }        
    }

    public class SpikeCorner : Obstacle
    {
        public SpikeCorner(Vector2 position, Room room) : base(position, new RectF(2, 2, 4, 4), room) { }
    }

    public class SpikeUp : Obstacle
    {
        public SpikeUp(Vector2 position, Room room) : base(position, new RectF(1, 5, 6, 3), room) { }
    }

    public class SpikeDown : Obstacle
    {
        public SpikeDown(Vector2 position, Room room) : base(position, new RectF(1, 0, 6, 3), room) { }
    }

    public class SpikeLeft : Obstacle
    {
        public SpikeLeft(Vector2 position, Room room) : base(position, new RectF(5, 1, 3, 6), room) { }
    }

    public class SpikeRight : Obstacle
    {
        public SpikeRight(Vector2 position, Room room) : base(position, new RectF(0, 1, 3, 6), room) { }
    }
}
