using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wyri.Main;
using Wyri.Objects.Levels;
using Wyri.Types;

namespace Wyri.Objects
{
    public enum PlayerState
    {
        Idle,
        Walk,
        Jump,        
    }

    public enum PlayerDirection
    {
        Left = -1,
        Right = 1
    }

    public class Player : SpatialObject
    {
        private PlayerState state = PlayerState.Idle;
        public PlayerState State
        {
            get
            {
                return state;
            }
            set
            {
                bool resetAnim = false;
                if (state != value)
                    resetAnim = true;
                state = value;
                if (resetAnim)
                    AnimationState[state].Reset();
            }
        }

        public PlayerDirection Direction { get; set; } = PlayerDirection.Right;

        public Dictionary<PlayerState, Animation> AnimationState { get; } = new Dictionary<PlayerState, Animation>();

        private float xVel, yVel, yGrav, xMax, yMax;

        private bool onGround, jumped, inWater;

        const float yGravAir = .12f;
        const float yGravWater = .01f;
        const float xMaxAir = 3;
        const float yMaxAir = 3;
        const float xMaxWater = .75f;
        const float yMaxWater = .5f;

        public Player(Vector2 position) : base(position, new RectF(-3, -4, 6, 12))
        {
            DrawOffset = new Vector2(8f, 8f);
            AnimationState.Add(PlayerState.Idle, new Animation(GameResources.Player, 0, 4, .1f));
            AnimationState.Add(PlayerState.Walk, new Animation(GameResources.Player, 6, 12, .2f));
            AnimationState.Add(PlayerState.Jump, new Animation(GameResources.Player, 12, 16, .3f, false));
        }

        float jumpPowerTimer = 10;

        public override void Update()
        {
            // input

            var kLeft = InputController.IsKeyPressed(Keys.Left);
            var kRight = InputController.IsKeyPressed(Keys.Right);
            var kJumpPressed = InputController.IsKeyPressed(Keys.A, KeyState.Pressed);
            var kJumpHolding = InputController.IsKeyPressed(Keys.A, KeyState.Holding);
            var kJumpReleased = InputController.IsKeyPressed(Keys.A, KeyState.Released);

            if (kLeft && !kRight)
            {
                Direction = PlayerDirection.Left;
                if (onGround)
                {
                    State = PlayerState.Walk;
                    xVel = Math.Max(xVel - .16f, -1.2f);
                }
                else
                {
                    State = PlayerState.Jump;
                    xVel = Math.Max(xVel - .06f, -1.2f);
                }
            }
            if (kRight && !kLeft)
            {
                Direction = PlayerDirection.Right;
                if (onGround)
                {
                    State = PlayerState.Walk;
                    xVel = Math.Min(xVel + .16f, 1.2f);
                }
                else
                {
                    State = PlayerState.Jump;
                    xVel = Math.Min(xVel + .06f, 1.2f);
                }                
            }

            if ((!kLeft && !kRight) || (kLeft && kRight))
            {
                if (onGround)
                {
                    xVel *= .6f;
                    if (Math.Abs(xVel) < .15f)
                    {
                        xVel = 0;
                        State = PlayerState.Idle;
                    }
                }
                else
                {
                    xVel *= .9f;
                }
            }

            if (State == PlayerState.Jump)
            {
                if (yVel < 0 && AnimationState[State].Frame >= 14)
                    AnimationState[State].Frame = 14;
                if (yVel >= 0 && AnimationState[State].Frame < 15)
                    AnimationState[State].Frame = 15;
            }

            //onPlatform

            var waterTile = Collisions.TileAt(X, Y + 4, "WATER");

            inWater = waterTile != null;
            yGrav = inWater ? yGravWater : yGravAir;
            xMax = inWater ? xMaxWater : xMaxAir;
            yMax = inWater ? yMaxWater : yMaxAir;

            onGround = yVel >= 0 && this.CollisionRectTile(0, yGrav);

            var platformTile = Collisions.TileAt(X, Y + G.T + yVel, "FG");
            var platformTileAbove = Collisions.TileAt(X, Y + yVel, "FG");
            var platformTileBelow = Collisions.TileAt(X, Y + yVel + 2, "FG");

            if (platformTile != null && platformTile.IsPlatform && asdf)
            {
                if (yVel > -yGrav)
                {
                    yVel = -yGrav;
                    onGround = true;
                }
            }

            if (onGround)
            {
                jumped = false;
                if (State == PlayerState.Jump)
                {
                    State = PlayerState.Idle;
                }
            }
            else
            {
                if (State == PlayerState.Idle || State == PlayerState.Walk)
                    State = PlayerState.Jump;
            }

            if (inWater)
            {
                jumped = false;
                jumpPowerTimer = 10;
            }

            if (kJumpHolding)
            {
                jumpPowerTimer = Math.Max(jumpPowerTimer - 1, 0);
                
                if (jumpPowerTimer == 0)
                {
                    jumped = true;
                }
                if (!jumped)
                {
                    yVel = -2f;
                    State = PlayerState.Jump;
                    onGround = false;
                }
            }
            else
            {
                if (kJumpReleased)
                {
                    jumped = true;
                    jumpPowerTimer = 10;
                }
            }

            // logic

            AnimationState[State].Update();

            var room = Collisions.CollisionPoint<Room>(X, Y).FirstOrDefault();
            if (room != null)
            {
                if (room != MainGame.Camera.Room)
                {
                    MainGame.Camera.Room = room;
                }
            }

            

            // movement & collision

            yVel += yGrav;

            xVel = Math.Sign(xVel) * Math.Min(Math.Abs(xVel), xMax);
            yVel = Math.Sign(yVel) * Math.Min(Math.Abs(yVel), yMax);

            //var tcolx = CollisionExtensions.TileAt(X + xVel, Y);
            //var tcoly = CollisionExtensions.TileAt(X, Y + yVel);

            if (!this.CollisionRectTile(xVel, 0))// || (tcolx != null && !tcolx.IsOn))
            {
                X += xVel;
            }
            else
            {
                xVel = 0;
            }
            if (!this.CollisionRectTile(0, yVel))// || (tcoly != null && !tcoly.IsOn))
            {
                Y += yVel;
            }
            else
            {
                if (yVel >= 0)
                {
                    Y = M.Div(Y + yVel + yGrav, (float)G.T) * G.T;
                    if (State == PlayerState.Jump)
                    {                        
                        State = PlayerState.Idle;
                    }
                }

                yVel = 0;
            }

        }

        

        public override void Draw(SpriteBatch sb)
        {
            AnimationState[State].Draw(sb, Position, DrawOffset, new Vector2((int)Direction, 1), Color.White, 0, G.D_PLAYER);
            
            //sb.DrawRectangle(Position + BBox, Color.White, false, G.D_PLAYER + .001f);
            //sb.DrawPixel(X, Y, Color.Red, G.D_PLAYER + .001f);
            //sb.DrawPixel(Center.X, Center.Y, Color.GreenYellow, G.D_PLAYER + .001f);
        }
    }
}