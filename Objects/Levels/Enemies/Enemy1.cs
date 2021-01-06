using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Wyri.Main;
using Wyri.Types;
using Wyri.Util;

namespace Wyri.Objects.Levels.Enemies
{
    public class Enemy1 : Obstacle
    {
        int prepareShootTimeout;
        int maxPrepareShootTimeout;

        float angle = 0;
        (bool, float) rayCast;
        Vector2 offvec = new Vector2(0, -4);
        float crossHairTimeout, maxCrossHairTimeout;

        float tDistortionDist;
        float tDistortionAng;

        Vector2 target;
        Vector2 newTarget;
        float distance = 80;
        float lineAlpha, hairAlpha;

        enum State
        {
            Idle,
            Target,
            PrepareShoot,
            Shoot
        }

        float frame;
        int shots;
        int shotTimeout;

        Vector2 center;

        State state;
        
        public Enemy1(Vector2 position, Room room) : base(position, new RectF(-3, -2, 6, 10), room)
        {
            maxPrepareShootTimeout = 2 * 60;
            maxCrossHairTimeout = 30;
            crossHairTimeout = maxCrossHairTimeout;
            prepareShootTimeout = maxPrepareShootTimeout;
            state = State.Idle;
        }

        public override void Update()
        {
            base.Update();

            center = Center + new Vector2(-.5f, -1.5f);

            var ang = M.VectorToAngle(MainGame.Player.Center - center + offvec);            
            var rc = this.RayCast(MainGame.Player, ang, 1, 255);
            bool inRange = M.Euclidean(center, MainGame.Player.Center) <= distance;

            rc = (rc.Item1, Math.Min(rc.Item2, distance));

            if (MainGame.Player.State == PlayerState.Dead)
            {
                rc = (false, 0);
            }

            switch (state)
            {
                case State.Idle:
                    frame = Math.Max(frame - .2f, 0);
                    hairAlpha = 3;
                    lineAlpha = 0;
                    prepareShootTimeout = maxPrepareShootTimeout;
                    crossHairTimeout = maxCrossHairTimeout;
                    if (rc.Item1 && inRange)
                    {                        
                        rayCast = rc;
                        angle = ang;
                        state = State.Target;
                    }
                    break;
                case State.Target:
                    frame = Math.Min(frame + .2f, 3.9f);
                    rayCast = rc;
                    target = center + new Vector2(M.LengthDirX(angle) * rayCast.Item2, M.LengthDirY(angle) * rayCast.Item2);
                    if (!rc.Item1 || !inRange)
                    {
                        newTarget = target;

                        prepareShootTimeout = Math.Min(prepareShootTimeout + 3, maxPrepareShootTimeout);
                        if (prepareShootTimeout == maxPrepareShootTimeout)
                            state = State.Idle;
                    }
                    else
                    {      
                        prepareShootTimeout = Math.Max(prepareShootTimeout - 1, 0);                        
                        angle = ang;
                        tDistortionDist = (float)prepareShootTimeout / (float)maxPrepareShootTimeout * 4;
                        tDistortionAng = (tDistortionAng + 12) % 360;
                        var distx = M.LengthDirX(tDistortionAng) * tDistortionDist;
                        var disty = M.LengthDirY(tDistortionAng) * tDistortionDist;
                        newTarget = target + new Vector2(distx, disty);
                    }

                    lineAlpha = 1 - (float)prepareShootTimeout / (float)maxPrepareShootTimeout;
                    if (prepareShootTimeout == 0)
                    {
                        state = State.PrepareShoot;
                    }
                    break;
                case State.PrepareShoot:
                    angle = ang;
                    crossHairTimeout = Math.Max(crossHairTimeout - 1, 0);
                    if (crossHairTimeout == 0)
                    {                        
                        state = State.Shoot;
                        shots = 3;
                    }
                    break;
                case State.Shoot:
                    hairAlpha = Math.Max(hairAlpha - .1f, 0);
                    lineAlpha = Math.Max(lineAlpha - .1f, 0);

                    if (shots > 0)
                    {
                        shotTimeout = Math.Max(shotTimeout - 1, 0);
                        if (shotTimeout == 0)
                        {
                            var bullet = new Bullet(center, Room);
                            bullet.Angle = M.VectorToAngle(newTarget - center);

                            shots = Math.Max(shots - 1 , 0);
                            shotTimeout = 10;
                        }
                    }
                    else
                    {
                        state = State.Idle;
                    }                   
                    break;
            }
        }

        public override void Draw(SpriteBatch sb)
        {
            sb.Draw(GameResources.Enemy1[(int)Math.Floor(frame)], Position, null, Color.White, 0, new Vector2(8), Vector2.One, SpriteEffects.None, G.D_ENEMY);

            var rs = 1f - (float)prepareShootTimeout / (float)maxPrepareShootTimeout;

            float r = 255;
            float g = 255 - rs * 255;
            float b = 255 - rs * 255;
            float a = rs * 255;

            var color = new Color((int)r, (int)g, (int)b);
            color = new Color(color, lineAlpha);
            var hairColor = new Color(Color.White, hairAlpha);

            if (state != State.Idle)
            {

                sb.DrawLine(center, newTarget, color, G.D_EFFECT);
                sb.DrawRectangle(newTarget + new RectF(-1.5f, -1.5f, 3, 3), new Color(color, 2 * lineAlpha), false, G.D_EFFECT);

                if (state != State.Target)
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
