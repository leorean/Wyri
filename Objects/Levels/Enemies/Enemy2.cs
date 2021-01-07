﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Main;
using Wyri.Types;
using Wyri.Util;

namespace Wyri.Objects.Levels.Enemies
{
    public class Enemy2 : Obstacle
    {
        enum State
        {
            Idle,
            Walk,
            Run            
        }

        enum Direction { Left = -1, Right = 1 }

        private Direction direction;
        private State state;

        private float t;
        private float animSpd;
        private float xVel;
        float s, maxAnimSpd, animAmpl;

        int waitTimer;
        bool changeDirection;

        public Enemy2(Vector2 position, Room room) : base(position, new RectF(-6, -4, 12, 10), room)
        {            
            state = State.Idle;
            direction = RND.Choose(Direction.Left, Direction.Right);
            waitTimer = 30 + RND.Int(60);
        }

        public override void Update()
        {
            base.Update();

            waitTimer = Math.Max(waitTimer - 1, 0);

            var rc = this.RayCast(MainGame.Player, (direction == Direction.Left) ? 180 : 0, 1, 48);

            if (rc.Item1)
            {
                state = State.Run;
            }

            if (state == State.Idle)
            {
                animAmpl = 1;
                maxAnimSpd = 2;
                animSpd = .75f;
                xVel = Math.Sign(xVel) * Math.Max(Math.Abs(xVel) - .025f, 0);
                if (waitTimer == 0)
                {
                    waitTimer = 120 + RND.Int(60);
                    if (changeDirection)
                    {
                        direction = direction == Direction.Left ? Direction.Right : Direction.Left;
                        changeDirection = false;
                    }
                    else
                    {
                        direction = RND.Choose(Direction.Left, Direction.Right);
                    }
                    state = State.Walk;
                }
            }
            else if (state == State.Walk)
            {
                animAmpl = 1;
                maxAnimSpd = .5f;
                animSpd = Math.Min(animSpd + .02f, maxAnimSpd);
                xVel = Math.Sign((int)direction) * Math.Min(Math.Abs(xVel) + .01f, .35f);
                if (waitTimer == 0)
                {
                    waitTimer = 60 + RND.Int(90);
                    state = State.Idle;
                }
            }
            else if (state == State.Run)
            {
                animAmpl = 2;
                waitTimer = 2 * 60;
                maxAnimSpd = 3;
                animSpd = maxAnimSpd;
                xVel = Math.Sign((int)direction) * Math.Min(Math.Abs(xVel) + .05f, 1.25f);
            }

            s = animSpd / maxAnimSpd;

            X += xVel;

            var groundTile = this.CollisionSolidTile((int)direction * 16, BBox.h, true);
            if (!this.CollisionSolidTile(xVel + Math.Sign(xVel) * 16, 0) && groundTile)
            {                
                
            }
            else
            {
                xVel = Math.Sign((int)direction) * Math.Max(Math.Abs(xVel) - .12f, 0);

                if (Math.Abs(xVel) == 0)
                {
                    changeDirection = true;
                    animSpd = 0;
                    if (state != State.Idle)
                    {
                        state = State.Idle;
                        waitTimer = 1 * 60;
                    }
                }
            }            

            t = (t + .2f * s * animAmpl);
            if (t >= 90000) t = 0;
        }

        public override void Draw(SpriteBatch sb)
        {
            //sb.DrawRectangle(Position + BBox, Color.Red, false, 1);

            var i = (int)direction;

            var headOffset = new Vector2(0, M.Sin((t + 1) % (float)(2 * Math.PI)) * .3f + 2);
            var legOffset = new Vector2(0, state == State.Idle ? Math.Min(M.Sin((t) % (float)(2 * Math.PI)) * .5f, 0) : -1f);

            var body = -(t + .5f) % (float)(2 * Math.PI);
            var head = -(t) % (float)(2 * Math.PI);
            var leg1 = (t + 1 * i) % (float)(2 * Math.PI);
            var leg2 = (t + 2 * i) % (float)(2 * Math.PI);
            var leg3 = (t + 3 * i) % (float)(2 * Math.PI);

            var leg4 = (t + 4 * i) % (float)(2 * Math.PI);
            var leg5 = (t + 5 * i) % (float)(2 * Math.PI);
            var leg6 = (t + 6 * i) % (float)(2 * Math.PI);

            var bodyAngle = M.DegToRad(-1 + M.Sin(body) * 2) * i;
            var leg1Angle = M.DegToRad(-1 + M.Sin(leg1) * 4) * i;
            var leg2Angle = M.DegToRad(-3 + M.Sin(leg2) * 6) * i;
            var leg3Angle = M.DegToRad(-1 + M.Sin(leg3) * 4) * i;
            var leg4Angle = M.DegToRad(-1 + M.Sin(leg4) * 4) * i;
            var leg5Angle = M.DegToRad(-3 + M.Sin(leg5) * 6) * i;
            var leg6Angle = M.DegToRad(-1 + M.Sin(leg6) * 4) * i;

            float dBody = G.D_ENEMY;
            float dHead = G.D_ENEMY - .00001f;
            float dLegFront = G.D_ENEMY+ .00002f;
            float dLegBack = G.D_ENEMY - .00001f;

            // body
            sb.Draw(GameResources.Enemy2[0], Position + new Vector2(i * -1, 0) + i * new Vector2(M.Sin(body) * .5f, M.Cos(body) * .5f) * s, null, Color.White, bodyAngle, new Vector2(8), new Vector2(i, 1), SpriteEffects.None, dBody);
            
            // head
            var headFrame = (state == State.Run) ? 2 : 1;
            sb.Draw(GameResources.Enemy2[headFrame], Position + new Vector2(i * -1, -4) + i * new Vector2(M.Sin(head) * .3f, M.Cos(head) * .4f) * s + headOffset, null, Color.White, bodyAngle, new Vector2(8), new Vector2(i, 1), SpriteEffects.None, dHead);

            // legs
            var leg1Pos = new Vector2(i * 4, 1) + new Vector2(M.Sin(leg1) * -i * .5f, M.Cos(leg1) * 1.5f) * s + legOffset;
            var leg2Pos = new Vector2(i * -2, 1) +  new Vector2(M.Sin(leg2) * -i * .5f, M.Cos(leg2) * 1.5f) * s + legOffset;
            var leg3Pos = new Vector2(i * -7, 1) + new Vector2(M.Sin(leg3) * -i * .2f, M.Cos(leg3) * 1.5f) * s + legOffset;
            
            var leg4Pos = new Vector2(i * 7, 1) + new Vector2(M.Sin(leg4) * -i * .2f, M.Cos(leg4) * 1.5f) * s + legOffset;
            var leg5Pos = new Vector2(i * 1, 1) + new Vector2(M.Sin(leg5) * -i * .2f, M.Cos(leg5) * 1.5f) * s + legOffset;
            var leg6Pos = new Vector2(i * -1, 0) + new Vector2(M.Sin(leg6) * -i * .2f, M.Cos(leg6) * 1.5f) * s + legOffset;

            leg1Pos = new Vector2(leg1Pos.X, Math.Min(leg1Pos.Y, .5f));
            leg2Pos = new Vector2(leg2Pos.X, Math.Min(leg2Pos.Y, .5f));
            leg3Pos = new Vector2(leg3Pos.X, Math.Min(leg3Pos.Y, .5f));
            leg4Pos = new Vector2(leg4Pos.X, Math.Min(leg4Pos.Y, .5f));
            leg5Pos = new Vector2(leg5Pos.X, Math.Min(leg5Pos.Y, .5f));
            leg6Pos = new Vector2(leg6Pos.X, Math.Min(leg6Pos.Y, .5f));

            sb.Draw(GameResources.Enemy2[3], Position + leg1Pos, null, Color.White, leg1Angle, new Vector2(8), new Vector2(i, 1), SpriteEffects.None, dLegFront);
            sb.Draw(GameResources.Enemy2[3], Position + leg2Pos, null, Color.White, leg2Angle, new Vector2(8), new Vector2(i, 1), SpriteEffects.None, dLegFront + .00001f);
            sb.Draw(GameResources.Enemy2[4], Position + leg3Pos, null, Color.White, leg3Angle, new Vector2(8), new Vector2(i, 1), SpriteEffects.None, dLegFront);

            sb.Draw(GameResources.Enemy2[5], Position + leg4Pos, null, Color.White, leg4Angle, new Vector2(8), new Vector2(i, 1), SpriteEffects.None, dLegBack);
            sb.Draw(GameResources.Enemy2[6], Position + leg5Pos, null, Color.White, leg5Angle, new Vector2(8), new Vector2(i, 1), SpriteEffects.None, dLegBack);
            sb.Draw(GameResources.Enemy2[6], Position + new Vector2(i * -3, 1) + leg6Pos, null, Color.White, leg6Angle, new Vector2(8), new Vector2(i, 1), SpriteEffects.None, dLegBack);

            base.Draw(sb);
        }

    }
}