using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Main;
using Wyri.Types;

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
        private float spd;
        private float xVel;
        float s, smax;

        int waitTimer;

        public Enemy2(Vector2 position, Room room) : base(position, new RectF(-8, -8, 16, 16), room)
        {            
            state = State.Walk;
            direction = Direction.Right;
        }

        public override void Update()
        {
            base.Update();            

            if (state == State.Idle)
            {
                smax = 2;
                spd = Math.Max(spd - .1f, .75f);

                waitTimer = Math.Max(waitTimer - 1, 0);
                if (waitTimer == 0)
                {
                    direction = direction == Direction.Left ? Direction.Right : Direction.Left;
                    state = State.Walk;
                }
            }
            else if (state == State.Walk)
            {
                smax = .5f;
                spd = Math.Min(spd + .04f, smax);
            }
            else if (state == State.Run)
            {
                smax = 2;
                spd = Math.Min(spd + .1f, smax);
            }

            s = spd / smax;
            xVel = Math.Sign((int)direction) * spd;

            if (!this.CollisionSolidTile(xVel, 0))
            {                
                X += xVel;
            }
            else
            {
                spd = 0;
                if (state != State.Idle)
                {
                    state = State.Idle;
                    waitTimer = 1 * 60;
                }                
            }            

            t = (t + .2f * s);
            if (t >= 90000) t = 0;
        }

        public override void Draw(SpriteBatch sb)
        {
            var headOffset = new Vector2(0, state == State.Idle ? M.Sin((t + 1) % (float)(2 * Math.PI)) : 0);
            var legOffset = new Vector2(0, state == State.Idle ? M.Sin((t) % (float)(2 * Math.PI)) : 0);

            var body = -(t + .5f) % (float)(2 * Math.PI);
            var head = -(t) % (float)(2 * Math.PI);
            var leg1 = -(t + 1) % (float)(2 * Math.PI);
            var leg2 = -(t + 2) % (float)(2 * Math.PI);
            var leg3 = -(t + 3) % (float)(2 * Math.PI);
            var leg4 = -(t + 2) % (float)(2 * Math.PI);

            var bodyAngle = M.DegToRad(-1 + M.Sin(body) * 2) * Math.Sign((int)direction);
            var leg1Angle = M.DegToRad(-2 + M.Sin(leg1) * 4) * Math.Sign((int)direction);
            var leg2Angle = M.DegToRad(-2 + M.Sin(leg2) * 4) * Math.Sign((int)direction);
            var leg3Angle = M.DegToRad(-2 + M.Sin(leg3) * 4) * Math.Sign((int)direction);
            var leg4Angle = M.DegToRad(-2 + M.Sin(leg4) * 4) * Math.Sign((int)direction);
            // body
            sb.Draw(GameResources.Enemy2[0], Position + (int)direction * new Vector2((float)M.Sin(body) * .5f, M.Cos(body)) * s, null, Color.White, bodyAngle, new Vector2(8), new Vector2((int)direction, 1), SpriteEffects.None, G.D_ENEMY);
            // head
            sb.Draw(GameResources.Enemy2[1], Position + new Vector2((int)direction * 5, -5) + (int)direction * new Vector2((float)M.Sin(head) * .3f, M.Cos(head) * .4f) * s + headOffset, null, Color.White, 0, new Vector2(8), new Vector2((int)direction, 1), SpriteEffects.None, G.D_ENEMY + .00002f);
            // front right leg = leg1
            sb.Draw(GameResources.Enemy2[2], Position + new Vector2((int)direction * 7, 1) + (int)direction * new Vector2((float)M.Sin(leg1), M.Cos(leg1) * 1.5f + 1) * s + legOffset, null, Color.White, leg1Angle, new Vector2(8), new Vector2((int)direction, 1), SpriteEffects.None, G.D_ENEMY - .00001f);
            // front left leg = leg2
            sb.Draw(GameResources.Enemy2[2], Position + new Vector2((int)direction * 0, 1) + (int)direction * new Vector2((float)M.Sin(leg2), M.Cos(leg2) * 1.5f + 1) * s + legOffset, null, Color.White, leg2Angle, new Vector2(8), new Vector2((int)direction, 1), SpriteEffects.None, G.D_ENEMY + .00001f);
            // back left leg = leg3
            sb.Draw(GameResources.Enemy2[3], Position + new Vector2((int)direction * -7, 1) + (int)direction * new Vector2((float)M.Sin(leg3), M.Cos(leg3) * 1.5f + 1) * s + legOffset, null, Color.White, leg3Angle, new Vector2(8), new Vector2((int)direction, 1), SpriteEffects.None, G.D_ENEMY + .00001f);
            // back right leg = leg4
            sb.Draw(GameResources.Enemy2[4], Position + new Vector2((int)direction * -3, 1) + (int)direction * new Vector2((float)M.Sin(leg4), M.Cos(leg4) * 1.5f + 1) * s + legOffset, null, Color.White, leg4Angle, new Vector2(8), new Vector2((int)direction, 1), SpriteEffects.None, G.D_ENEMY - .00001f);

            base.Draw(sb);
        }

    }
}
