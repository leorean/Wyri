using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Main;
using Wyri.Types;

namespace Wyri.Objects.Levels.Enemies
{
    public class Enemy1 : Obstacle
    {
        int shootTimeout;
        int maxShootTimeout;

        float angle = 0;
        float targetAngle = 0;
        (bool, float) rayCast;

        enum State
        {
            Idle,
            Target,
            Shoot
        }

        State s;
        
        public Enemy1(Vector2 position, Room room) : base(position, new RectF(-3, -2, 6, 10), room)
        {
            maxShootTimeout = 3 * 60;
            shootTimeout = maxShootTimeout;

            s = State.Idle;
        }

        public override void Update()
        {
            base.Update();

            switch (s)
            {
                case State.Idle:
                    shootTimeout = Math.Min(shootTimeout + 3, maxShootTimeout);

                    var ang = M.VectorToAngle(MainGame.Player.Center - Center);
                    rayCast = this.RayCast(MainGame.Player, ang, 1, 80);
                    if (rayCast.Item1)
                    {
                        angle = ang;
                        s = State.Target;
                    }
                    break;
                case State.Target:
                    shootTimeout = Math.Max(shootTimeout - 1, 0);
                    angle = M.VectorToAngle(MainGame.Player.Center - Center);
                    rayCast = this.RayCast(MainGame.Player, angle, 1, 80);
                    if (!rayCast.Item1)
                        s = State.Idle;
                    if (shootTimeout == 0)
                    {
                        s = State.Shoot;
                    }
                    break;
                case State.Shoot:
                    break;
                default:
                    break;
            }

            //angle = M.VectorToAngle(MainGame.Player.Center - Center);
            //rayCast = this.RayCast(MainGame.Player, angle, 1, 80);

            //if (rayCast.Item1)
            //{
            //    shootTimeout = Math.Max(shootTimeout - 1, 0);
            //}
            //else
            //{
            //    shootTimeout = Math.Min(shootTimeout + 3, maxShootTimeout);
            //}
        }

        public override void Draw(SpriteBatch sb)
        {
            sb.Draw(GameResources.Enemy1[0], Position, null, Color.White, 0, new Vector2(8), Vector2.One, SpriteEffects.None, G.D_ENEMY);
            if (s == State.Target)
            {
                var rs = 1f - (float)shootTimeout / (float)maxShootTimeout;

                float r = 255;
                float g = 255 - rs * 255;
                float b = 255 - rs * 255;
                float a = 127 + rs * 128;

                var color = new Color((byte)r, (byte)g, (byte)b, (byte)a);

                sb.DrawLine(Center, MainGame.Player.Center, color, G.D_EFFECT);
                sb.Draw(GameResources.Enemy1[2], Position + new Vector2(M.LengthDirX(angle) * rayCast.Item2, M.LengthDirY(angle) * rayCast.Item2), null, Color.White, 0, new Vector2(8), Vector2.One, SpriteEffects.None, G.D_EFFECT);
            }            


            base.Draw(sb);
        }
    }
}
