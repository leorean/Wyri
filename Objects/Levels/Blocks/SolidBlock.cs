using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Types;

namespace Wyri.Objects.Levels.Blocks
{
    public class SolidBlock : RoomObject
    {
        public SolidBlock(Vector2 position, RectF boundingBox, Room room) : base(position, boundingBox, room)
        {

        }

        public override void Draw(SpriteBatch sb)
        {
            throw new NotImplementedException();
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }
    }
}
