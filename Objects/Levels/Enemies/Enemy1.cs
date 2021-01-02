﻿using Microsoft.Xna.Framework;
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
        float distance = 144;
        float lineAlpha, hairAlpha;

        enum State
        {
            Idle,
            Target,
            PrepareShoot,
            Shoot
        }

        int shots;
        int shotTimeout;

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

            var ang = M.VectorToAngle(MainGame.Player.Center - Center + offvec);            
            var rc = this.RayCast(MainGame.Player, ang, 1, distance);

            if (MainGame.Player.State == PlayerState.Dead)
            {
                rc = (false, 0);
            }

            switch (state)
            {
                case State.Idle:
                    hairAlpha = 3;
                    lineAlpha = 0;
                    prepareShootTimeout = maxPrepareShootTimeout;
                    crossHairTimeout = maxCrossHairTimeout;
                    if (rc.Item1)
                    {                        
                        rayCast = rc;
                        angle = ang;
                        state = State.Target;
                    }
                    break;
                case State.Target:
                    rayCast = rc;
                    target = Center + new Vector2(M.LengthDirX(angle) * rayCast.Item2, M.LengthDirY(angle) * rayCast.Item2);
                    if (!rc.Item1)
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
                            var bullet = new Bullet(Center, Room);
                            bullet.Angle = M.VectorToAngle(newTarget - Center);

                            shots = Math.Max(shots - 1 , 0);
                            shotTimeout = 20;
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
            sb.Draw(GameResources.Enemy1[0], Position, null, Color.White, 0, new Vector2(8), Vector2.One, SpriteEffects.None, G.D_ENEMY);

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
                
                sb.DrawLine(Center, newTarget, color, G.D_EFFECT);

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
