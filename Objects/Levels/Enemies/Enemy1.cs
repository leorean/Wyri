using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Main;
using Wyri.Types;
using Wyri.Util;

namespace Wyri.Objects.Levels.Enemies
{
    public class Enemy1 : Obstacle
    {
        int shootTimeout;
        int maxShootTimeout;

        float angle = 0;
        (bool, float) rayCast;
        Vector2 offvec = new Vector2(0, -4);
        float crossHairTimeout, maxCrossHairTimeout;

        Vector2 target;
        Vector2 newTarget;
        float distance = 144;
        float lineAlpha = 0;

        enum State
        {
            Idle,
            Target,
            PrepareShoot,
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
            var rc = this.RayCast(MainGame.Player, ang, 1, distance);

            switch (s)
            {
                case State.Idle:
                    shootTimeout = maxShootTimeout;
                    crossHairTimeout = maxCrossHairTimeout;
                    if (rc.Item1)
                    {                        
                        rayCast = rc;
                        angle = ang;
                        s = State.Target;
                    }
                    break;
                case State.Target:
                    if (!rc.Item1)
                    {
                        rayCast = rc;
                        shootTimeout = Math.Min(shootTimeout + 3, maxShootTimeout);
                        if (shootTimeout == maxShootTimeout)
                            s = State.Idle;
                    }
                    else
                    {
                        lineAlpha = Math.Min(lineAlpha + .1f, 1);
                        shootTimeout = Math.Max(shootTimeout - 1, 0);
                        rayCast = rc;
                        angle = ang;
                    }
                    if (shootTimeout == 0)
                    {
                        s = State.PrepareShoot;
                    }
                    break;
                case State.PrepareShoot:
                    angle = ang;
                    crossHairTimeout = Math.Max(crossHairTimeout - 1, 0);
                    if (crossHairTimeout == 0)
                    {                        
                        s = State.Shoot;
                    }
                    break;
                case State.Shoot:
                    {
                        lineAlpha = Math.Max(lineAlpha - .1f, 0);
                    }
                    break;
                default:
                    break;
            }

            target = Center + new Vector2(M.LengthDirX(angle) * rayCast.Item2, M.LengthDirY(angle) * rayCast.Item2);

            var rndx = RND.Next * (float)shootTimeout / (float)maxShootTimeout * 8;
            var rndy = RND.Next * (float)shootTimeout / (float)maxShootTimeout * 8;

            newTarget = target + new Vector2(rndx, rndy);

        }

        public override void Draw(SpriteBatch sb)
        {
            sb.Draw(GameResources.Enemy1[0], Position, null, Color.White, 0, new Vector2(8), Vector2.One, SpriteEffects.None, G.D_ENEMY);

            var rs = 1f - (float)shootTimeout / (float)maxShootTimeout;

            float r = 255;
            float g = 255 - rs * 255;
            float b = 255 - rs * 255;
            float a = rs * 255;

            var color = new Color((int)r, (int)g, (int)b);
            color = new Color(color, lineAlpha);
            var hairColor = Color.White;

            if (s != State.Idle)
            {                
                
                sb.DrawLine(Center, newTarget, color, G.D_EFFECT);

                if (s != State.Target)
                {
                    var off = -4 + (float)crossHairTimeout / (float)maxCrossHairTimeout * 8;
                    sb.Draw(GameResources.Crosshair[0], newTarget - new Vector2(4) + new Vector2(-off, -off), null, hairColor, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, G.D_EFFECT);
                    sb.Draw(GameResources.Crosshair[1], newTarget - new Vector2(4) + new Vector2(off, -off), null, hairColor, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, G.D_EFFECT);
                    sb.Draw(GameResources.Crosshair[2], newTarget - new Vector2(4) + new Vector2(-off, off), null, hairColor, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, G.D_EFFECT);
                    sb.Draw(GameResources.Crosshair[3], newTarget - new Vector2(4) + new Vector2(off, off), null, hairColor, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, G.D_EFFECT);
                }
            }

            base.Draw(sb);
        }
    }
}
