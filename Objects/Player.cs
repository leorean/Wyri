using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        Climb
    }

    public enum PlayerDirection
    {
        Left = -1,
        Right = 1        
    }

    // add: flags |= flag
    // remove: flags &= ~flag
    // toggle: flags ^= flag

    [Flags]
    public enum PlayerAbility
    {
        NONE = 0,
        SWIM = 1,
        DOUBLE_JUMP = 2,
        CLIMB = 4,
        GLIDE = 8        
    }

    public static class PlayerExtensions
    {
        public static PlayerDirection Reverse(this PlayerDirection dir) => (PlayerDirection)(-(int)dir);
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

        public PlayerAbility Abilities { get; set; } = PlayerAbility.NONE;

        public PlayerDirection Direction { get; set; } = PlayerDirection.Right;

        public Dictionary<PlayerState, Animation> AnimationState { get; } = new Dictionary<PlayerState, Animation>();

        private float xVel, yVel, yGrav, xMax, yMax;

        private bool onGround, jumped, inWater;

        private int jumps;
        private int maxJumps;
        private float jumpPowerTimer = 10;
        private float maxJumpPowerTimer = 10;

        const float yGravAir = .12f;
        const float yGravWater = .01f;
        const float xVelMaxAir = 3;
        const float yVelMaxAir = 3;
        const float xVelMaxWater = .75f;
        const float yVelMaxWater = .5f;

        public Player(Vector2 position) : base(position, new RectF(-3, -4, 6, 12))
        {
            DrawOffset = new Vector2(8f, 8f);
            AnimationState.Add(PlayerState.Idle, new Animation(GameResources.Player, 0, 4, .1f));
            AnimationState.Add(PlayerState.Walk, new Animation(GameResources.Player, 6, 12, .2f));
            AnimationState.Add(PlayerState.Jump, new Animation(GameResources.Player, 12, 16, .3f, false));            
            AnimationState.Add(PlayerState.Climb, new Animation(GameResources.Player, 18, 22, .1f));

            //Abilities |= PlayerAbility.DOUBLE_JUMP;
        }

        
        private void HandlePlatforms()
        {
            if (yVel < -yGrav)
                return;

            var grid = MainGame.Map.LayerData["FG"];

            for (var i = -1; i < 2; i++)
            {

                float tx = M.Div(X, G.T) + i;
                float ty = M.Div(Y + G.T + yVel, G.T);

                var t = grid[(int)tx, (int)ty];
                if (t == null || t.Type != TileType.Platform)
                    continue;

                tx *= G.T;
                ty *= G.T;

                if (tx > Right || tx + G.T < Left)
                    continue;

                if (Bottom > ty - 4 && Bottom < ty + 1)
                {
                    yVel = -yGrav;
                    Y = ty - G.T;
                    onGround = true;
                    return;
                }
            }
        }

        private bool OnWall()
        {
            if (inWater)
                return false;

            var grid = MainGame.Map.LayerData["FG"];
            float tx = M.Div(X + Math.Sign((int)Direction) * 4f, G.T);
            float ty = M.Div(Y + 2, G.T);
            var t = grid[(int)tx, (int)ty];

            float txg = M.Div(X, G.T);
            float tyg = M.Div(Y + G.T + 2, G.T);

            var tg = grid[(int)txg, (int)tyg];
            if (tg != null && tg.IsSolid)
                return false;

            if (t != null && t.IsSolid)
            {
                return true;
            }
            return false;
        }

        private void ResetJumps()
        {
            jumped = false;
            jumps = maxJumps;
            jumpPowerTimer = maxJumpPowerTimer;
            jumpFromClimb = false;
        }

        private bool jumpFromClimb;
        private int grabTimer;

        public void SetCameraRoom()
        {
            var room = Collisions.CollisionPoint<Room>(X, Y).FirstOrDefault();
            if (room != null)
            {
                MainGame.Camera.Room = room;
            }
        }

        public override void Update()
        {
            // input

            var kLeft = InputController.IsKeyPressed(Keys.Left);
            var kRight = InputController.IsKeyPressed(Keys.Right);
            var kLeftPressed = InputController.IsKeyPressed(Keys.Left, KeyState.Pressed);
            var kRightPressed = InputController.IsKeyPressed(Keys.Right, KeyState.Pressed);
            var kLeftReleased = InputController.IsKeyPressed(Keys.Left, KeyState.Released);
            var kRightReleased = InputController.IsKeyPressed(Keys.Right, KeyState.Released);
            var kUp = InputController.IsKeyPressed(Keys.Up);
            var kDown = InputController.IsKeyPressed(Keys.Down);
            var kUpPressed = InputController.IsKeyPressed(Keys.Up, KeyState.Pressed);
            var kDownPressed = InputController.IsKeyPressed(Keys.Down, KeyState.Pressed);
            var kJumpPressed = InputController.IsKeyPressed(Keys.A, KeyState.Pressed);
            var kJumpHolding = InputController.IsKeyPressed(Keys.A, KeyState.Holding);
            var kJumpReleased = InputController.IsKeyPressed(Keys.A, KeyState.Released);

            if (InputController.IsKeyPressed(Keys.LeftShift, KeyState.Holding))
            {
                if (kLeftPressed)
                    X -= 32 * G.T;
                if (kRightPressed)
                    X += 32 * G.T;
                if (kUpPressed)
                    Y -= 18 * G.T;
                if (kDownPressed)
                    Y += 18 * G.T;
            }

            if (Abilities.HasFlag(PlayerAbility.DOUBLE_JUMP))
            {
                maxJumps = 2;
            }
            else
            {
                maxJumps = 1;
            }

            if (State != PlayerState.Climb)
            {

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
                        if (xVel > -1.2f)
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
                        if (xVel < 1.2f)
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
            }

            if (State == PlayerState.Jump)
            {
                if (yVel < 0 && AnimationState[State].Frame >= 14)
                    AnimationState[State].Frame = 14;
                if (yVel >= 0 && AnimationState[State].Frame < 15)
                    AnimationState[State].Frame = 15;


                if (OnWall() && ((kLeft && Direction == PlayerDirection.Left) || (kRight && Direction == PlayerDirection.Right)))
                {
                    State = PlayerState.Climb;
                }                
            }

            var waterTile = Collisions.TileAt(X, Y + 4, "WATER");

            inWater = waterTile != null;
            yGrav = inWater ? yGravWater : yGravAir;
            xMax = inWater ? xVelMaxWater : xVelMaxAir;
            yMax = inWater ? yVelMaxWater : yVelMaxAir;

            onGround = yVel >= 0 && this.CollisionSolidTile(0, yGrav);

            HandlePlatforms();

            if (onGround)
            {
                ResetJumps();
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

            if (State == PlayerState.Climb)
            {
                if (!((kLeft && Direction == PlayerDirection.Left) || (kRight && Direction == PlayerDirection.Right)) && !kDown && !kJumpHolding)
                    grabTimer = Math.Max(grabTimer - 1, 0);
                else
                    grabTimer = 20;

                if (!OnWall() || grabTimer == 0)
                {
                    jumpFromClimb = false;
                    State = PlayerState.Jump;
                }
                xVel = 0;

                if (kDown)
                {
                    yVel = Math.Min(yVel + .2f, .5f);
                }
                else
                {
                    yVel = -yGrav;
                }

                if (kDown)
                    AnimationState[State].Reset();

                if ((Direction == PlayerDirection.Right && kLeft) || (Direction == PlayerDirection.Left && kRight)
                    || kJumpPressed)
                {
                    Direction = Direction.Reverse();

                    if ((kLeft && Direction == PlayerDirection.Left) || (kRight && Direction == PlayerDirection.Right))
                    {
                        xVel = 1.5f * Math.Sign((int)Direction);
                        yVel = -2f;
                    }
                    else
                    {
                        xVel = .5f * Math.Sign((int)Direction);
                        yVel = -2.5f;
                    }

                    ResetJumps();
                    jumpFromClimb = true;
                    if (Abilities.HasFlag(PlayerAbility.DOUBLE_JUMP))
                    {
                        jumps = 1;
                    }
                    else
                    {
                        jumps = 0;
                        jumped = true;
                    }

                    State = PlayerState.Jump;
                    AnimationState[State].Frame = 15;
                }
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
                if (kJumpReleased && !jumpFromClimb)
                {
                    if (jumps > 1)
                    {
                        // todo: double-jump effect

                        jumps = Math.Max(jumps - 1, 0);
                        jumpPowerTimer = maxJumpPowerTimer;
                        jumped = false;
                    }
                    else
                    {
                        jumped = true;
                        jumpPowerTimer = maxJumpPowerTimer;
                    }
                }
            }

            if (inWater)
            {
                ResetJumps(); 
            }

            // logic

            AnimationState[State].Update();

            SetCameraRoom();

            // movement & collision

            yVel += yGrav;

            xVel = Math.Sign(xVel) * Math.Min(Math.Abs(xVel), xMax);
            yVel = Math.Sign(yVel) * Math.Min(Math.Abs(yVel), yMax);

            //var tcolx = CollisionExtensions.TileAt(X + xVel, Y);
            //var tcoly = CollisionExtensions.TileAt(X, Y + yVel);

            if (!this.CollisionSolidTile(xVel, 0))// || (tcolx != null && !tcolx.IsOn))
            {
                X += xVel;
            }
            else
            {
                xVel = 0;
            }
            if (!this.CollisionSolidTile(0, yVel))// || (tcoly != null && !tcoly.IsOn))
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