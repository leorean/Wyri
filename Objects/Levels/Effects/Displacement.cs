using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wyri.Objects.Levels.Effects
{
    public class Displacement : SpatialObject
    {
        private Texture2D buffer;

        public Displacement(Vector2 position) : base(position, new Types.RectF(0, 0, 8, 8))
        {

        }
        public override void Update()
        {
            buffer = MainGame.LastBuffer as Texture2D;
        }
        public override void Draw(SpriteBatch sb)
        {
            if (buffer != null)
            {
                //sb.Draw(buffer, Position, new Rectangle(0,0,8 * 4,8 * 4), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 1);
            }
        }
    }
}
