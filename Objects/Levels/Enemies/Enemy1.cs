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
        Vector2 target;
        (bool, float) rayCast;
        Vector2 offvec = new Vector2(0, -4);
        float crossHairTimeout, maxCrossHairTimeout;

        enum State
        {
            Idle,
            Target,
            Shoot
        }

        State s;
        
        public Enemy1(Vector2 position, Room room) : base(position, new RectF(-3, -2, 6, 10), room)
        {
            maxShootTimeout = 2 * 60;
            maxCrossHairTimeout = 30;
            crossHairTimeout = maxCrossHairTimeout;
            shootTimeout = maxShootTimeout;


            s = State.Idle;
        }

        public override void Update()
        {
            base.Update();

            var ang = M.VectorToAngle(MainGame.Player.Center - Center + offvec);

            switch (s)
            {
                case State.Idle:
                    crossHairTimeout = maxCrossHairTimeout;
                    var rc = this.RayCast(MainGame.Player, ang, 1, 80);
                    if (rc.Item1)
                    {
                        rayCast = rc;
                        angle = ang;
                        s = State.Target;
                    }
                    break;
                case State.Target:
                    shootTimeout = Math.Max(shootTimeout - 1, 0);
                    rc = this.RayCast(MainGame.Player, ang, 1, 80);
                    if (!rc.Item1)
                    {
                        rayCast = rc;

                        shootTimeout = Math.Min(shootTimeout + 3, maxShootTimeout);
                        if (shootTimeout == maxShootTimeout)
                            s = State.Idle;
                    }
                    else
                    {
                        rayCast = rc;
                        angle = ang;
                    }
                    if (shootTimeout == 0)
                    {
                        s = State.Shoot;
                    }
                    break;
                case State.Shoot:
                    crossHairTimeout = Math.Max(crossHairTimeout - 1, 0);
                    if (crossHairTimeout == 0)
                    {                        
                        shootTimeout = maxShootTimeout;
                        s = State.Idle;
                    }
                    break;
                default:
                    break;
            }

            if (rayCast.Item1)
                target = Center + new Vector2(M.LengthDirX(angle) * rayCast.Item2, M.LengthDirY(angle) * rayCast.Item2);

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
            if (shootTimeout < maxShootTimeout)
            {
                var rs = 1f - (float)shootTimeout / (float)maxShootTimeout;

                float r = 255;
                float g = 255 - rs * 255;
                float b = 255 - rs * 255;
                float a = rs * 255;

                var color = new Color((byte)r, (byte)g, (byte)b, (byte)a);
                var hairColor = (crossHairTimeout < maxCrossHairTimeout) ? Color.White : new Color(color, 1.0f);

                var off = 1 + (float)crossHairTimeout / (float)maxCrossHairTimeout * 3;

                sb.DrawLine(Center, target, color, G.D_EFFECT);
                sb.Draw(GameResources.Crosshair[0], target - new Vector2(4) + new Vector2(-off, -off), null, hairColor, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, G.D_EFFECT);
                sb.Draw(GameResources.Crosshair[1], target - new Vector2(4) + new Vector2(off, -off), null, hairColor, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, G.D_EFFECT);
                sb.Draw(GameResources.Crosshair[2], target - new Vector2(4) + new Vector2(-off, off), null, hairColor, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, G.D_EFFECT);
                sb.Draw(GameResources.Crosshair[3], target - new Vector2(4) + new Vector2(off, off), null, hairColor, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, G.D_EFFECT);
            }


            base.Draw(sb);
        }
    }
}
