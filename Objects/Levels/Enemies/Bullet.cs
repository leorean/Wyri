using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Wyri.Main;
using Wyri.Objects.Levels.Effects;
using Wyri.Types;

namespace Wyri.Objects.Levels.Enemies
{
    public class Bullet : Obstacle, IDestroyOnRoomChange
    {
        float speed = 0;
        public float Angle { get; set; } = 0;

        public Bullet(Vector2 position, Room room) : base(position, new Types.RectF(-1, -1, 2, 2), room)
        {
        }

        public override void Update()
        {
            base.Update();

            speed = Math.Min(speed + .3f, 6);

            var xVel = M.LengthDirX(Angle) * speed;
            var yVel = M.LengthDirY(Angle) * speed;
            
            if (!(Position + BBox + new Vector2(xVel,yVel)).Intersects(Room.Position + Room.BBox))
            {
                Destroy();
            }

            var triggerBlock = this.CollisionPoint<TriggerBlock>(X + .5f * xVel, Y + .5f * yVel).FirstOrDefault();
            if (triggerBlock != null)
            {
                triggerBlock.On = !triggerBlock.On;
                Room.SwitchState = !Room.SwitchState;
            }

            if (this.CollisionSolidTile(xVel * .5f, yVel * .5f) || triggerBlock != null)
            {
                new AnimationEffect(Position, 2, Room);
                Destroy();
            }
            else
            {
                X += xVel;
                Y += yVel;
            }
        }

        public override void Draw(SpriteBatch sb)
        {
            //sb.DrawRectangle(Position + BBox, Color.Red, false, .8f);
            sb.Draw(GameResources.Projectiles[0], Position, null, Color.White, M.DegToRad(Angle), new Vector2(4), Vector2.One, SpriteEffects.None, G.D_EFFECT);
        }
    }
}
