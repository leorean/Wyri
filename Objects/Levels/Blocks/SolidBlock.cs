using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wyri.Objects.Levels.Blocks
{
    public class SolidBlock : RoomObject
    {

        public SolidBlock(Vector2 position, Rectangle boundingBox, Room room) : base(position, boundingBox, room)
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
