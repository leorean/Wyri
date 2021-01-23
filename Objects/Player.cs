using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Wyri.Main;
using Wyri.Objects.Levels;
using Wyri.Objects.Levels.Effects;
using Wyri.Types;
using Wyri.Util;

namespace Wyri.Objects
{
    public enum PlayerState
    {
        Idle,
        Walk,
        Jump,
        Climb,
        Dead,
        StandUp,
        GotItem,
        Hover
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
        DIVE = 1,
        DOUBLE_JUMP = 2,
        WALL_GRAB = 4,
        JETPACK = 8,
        MAP = 16,
        COMPASS = 32,
        DRILL = 64,
        CARD_A = 128,
        CARD_B = 256,
        CARD_C = 512
    }

    public static class PlayerExtensions
    {
        public static PlayerDirection Reverse(this PlayerDirection dir) => (PlayerDirection)(-(int)dir);
    }

    public class Player : SpatialObject, IStayActive
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

        public PlayerAbility Abilities => MainGame.SaveGame.Abilities;

        public PlayerDirection Direction { get; set; } = PlayerDirection.Right;

        public Dictionary<PlayerState, Animation> AnimationState { get; } = new Dictionary<PlayerState, Animation>();

        private float yGrav, xMax, yMax;

        public float XVel { get; set; }
        public float YVel { get; set; }

        private bool onGround, inWater;

        bool jumped;
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

        float depth = G.D_PLAYER;
        float deadTimer = 2 * 60;
        float standUpTimer = 2 * 60;
        float getUpTimer = 2 * 60;
        bool somethingPressedOnce = false;

        const int maxOxygen = 4 * 60;
        int oxygen = maxOxygen;
        float oxygenAlpha = 0;

        int gotItemPostTimer;
        int gotItemTimer;
        const int maxGotItemTimer = 45;
        const int maxGotItemPostTimer = 90;        
        Item gotItem;
        private MessageBox itemMsgBox;

        private int grabTimer, wasOnWallTimer;

        private int hoverPower;
        const float maxHoverPower = 60;
        private int hoverTimeout, hoverButtonTimeout;
        private float hoverAlpha;

        public bool ControlsEnabled { get; set; } = true;

        private int leftTimer, rightTimer;
        const int maxDirectionGrabTimer = 25;

        private Drill drill;
        private Vector2 drillTargetPosition;
        
        public Player(Vector2 position) : base(position, new RectF(-3, -4, 6, 12))
        {
            AnimationState.Add(PlayerState.Idle, new Animation(GameResources.Player, 0, 4, .1f));
            AnimationState.Add(PlayerState.Walk, new Animation(GameResources.Player, 8, 6, .2f));
            AnimationState.Add(PlayerState.Jump, new Animation(GameResources.Player, 16, 5, .3f, false));
            AnimationState.Add(PlayerState.Climb, new Animation(GameResources.Player, 24, 4, .1f));
            AnimationState.Add(PlayerState.Dead, new Animation(GameResources.Player, 32, 8, .2f, false));
            AnimationState.Add(PlayerState.StandUp, new Animation(GameResources.Player, 40, 8, .15f, false));
            AnimationState.Add(PlayerState.GotItem, new Animation(GameResources.Player, 48, 1, 0, false));
            AnimationState.Add(PlayerState.Hover, new Animation(GameResources.Player, 56, 1, 0, false));

            oxygen = maxOxygen;

            //Abilities |= PlayerAbility.DOUBLE_JUMP;
            //Abilities |= PlayerAbility.WALL_GRAB;
            //Abilities |= PlayerAbility.MAP;
        }


        private void HandlePlatforms()
        {
            if (YVel < -yGrav)
                return;

            var grid = MainGame.Map.LayerData["FG"];

            for (var i = -1; i < 2; i++)
            {

                float tx = M.Div(X, G.T) + i;
                float ty = M.Div(Y + G.T + YVel, G.T);

                var t = grid[(int)tx, (int)ty];
                if (t == null || t.Type != TileType.Platform)
                    continue;

                tx *= G.T;
                ty *= G.T;

                if (tx > Right || tx + G.T < Left)
                    continue;

                if (Bottom > ty - 4 && Bottom < ty + 1)
                {
                    YVel = -yGrav;
                    Y = ty - G.T;
                    onGround = true;
                    return;
                }
            }
        }

        private bool OnWallEdge()
        {
            var grid = MainGame.Map.LayerData["FG"];
            float tx = M.Div(X - Math.Sign((int)Direction) * 4f, G.T);
            float ty = M.Div(Y - 4, G.T);
            var t = grid[(int)tx, (int)ty];

            return t == null || !t.IsSolid;
        }

        private bool OnWall()
        {
            if (inWater || onGround)
                return false;

            if (Left <= MainGame.Camera.Room.X + 4 || Right >= MainGame.Camera.Room.X + MainGame.Camera.Room.Width - 4)
                return false;

            var grid = MainGame.Map.LayerData["FG"];
            float tx = M.Div(X + Math.Sign((int)Direction) * 4f, G.T);
            float ty = M.Div(Y + 2, G.T);
            var t = grid[(int)tx, (int)ty];

            float txground = M.Div(X, G.T);
            float tyground = M.Div(Y + G.T + 2, G.T);

            var tground = grid[(int)txground, (int)tyground];
            if (tground != null && tground.IsSolid)
                return false;

            if (t != null && t.IsSolid)
            {
                return true;
            }
            return false;
        }

        private void ResetJumps()
        {
            jumps = maxJumps;
            jumpPowerTimer = maxJumpPowerTimer;            
        }

        
        public void SetCameraRoom()
        {
            var room = Collisions.CollisionPoint<Room>(X, Y).FirstOrDefault();
            if (room != null)
            {
                if (room != MainGame.Camera.Room)
                {
                    MainGame.Camera.Position = Position;
                    var effects = ObjectController.FindAll<IDestroyOnRoomChange>();
                    foreach(var e in effects)
                        e.Destroy();                
                }
                if (!MainGame.SaveGame.VisitedRooms.Contains(room.ID))
                {
                    MainGame.SaveGame.VisitedRooms.Add(room.ID);
                }
                MainGame.Camera.Room = room;                
            }
        }

        public override void Update()
        {
            // input

            var kLeft = InputController.IsKeyPressed(Keys.Left) && ControlsEnabled;
            var kRight = InputController.IsKeyPressed(Keys.Right) && ControlsEnabled;
            var kLeftPressed = InputController.IsKeyPressed(Keys.Left, KeyState.Pressed) && ControlsEnabled;
            var kRightPressed = InputController.IsKeyPressed(Keys.Right, KeyState.Pressed) && ControlsEnabled;
            var kLeftReleased = InputController.IsKeyPressed(Keys.Left, KeyState.Released) && ControlsEnabled;
            var kRightReleased = InputController.IsKeyPressed(Keys.Right, KeyState.Released) && ControlsEnabled;
            var kUp = InputController.IsKeyPressed(Keys.Up) && ControlsEnabled;
            var kDown = InputController.IsKeyPressed(Keys.Down) && ControlsEnabled;
            var kUpPressed = InputController.IsKeyPressed(Keys.Up, KeyState.Pressed) && ControlsEnabled;
            var kDownPressed = InputController.IsKeyPressed(Keys.Down, KeyState.Pressed) && ControlsEnabled;
            var kJumpPressed = InputController.IsKeyPressed(Keys.A, KeyState.Pressed) && ControlsEnabled;
            var kJumpHolding = InputController.IsKeyPressed(Keys.A, KeyState.Holding) && ControlsEnabled;
            var kJumpReleased = InputController.IsKeyPressed(Keys.A, KeyState.Released) && ControlsEnabled;
            var kAction = InputController.IsKeyPressed(Keys.S, KeyState.Holding) && ControlsEnabled;
            var kActionPressed = InputController.IsKeyPressed(Keys.S, KeyState.Pressed) && ControlsEnabled;
            //var kAction2 = InputController.IsKeyPressed(Keys.D, KeyState.Holding) && ControlsEnabled;
            //var kAction2Pressed = InputController.IsKeyPressed(Keys.D, KeyState.Pressed) && ControlsEnabled;

            leftTimer = Math.Max(leftTimer - 1, 0);
            rightTimer = Math.Max(rightTimer - 1, 0);

            if (kLeftPressed) leftTimer = maxDirectionGrabTimer;
            if (kRightPressed) rightTimer = maxDirectionGrabTimer;

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

            maxJumps = Abilities.HasFlag(PlayerAbility.DOUBLE_JUMP) ? 2 : 1;

            AnimationState[State].Update();
            SetCameraRoom();

            if (MainGame.Camera.Room == null)
                return;

            // logic

            if (State == PlayerState.StandUp)
            {
                standUpTimer = Math.Max(standUpTimer - 1, 0);
                if (standUpTimer == 0)
                {
                    if (kJumpPressed)
                        State = PlayerState.Jump;
                    
                    if (!somethingPressedOnce)
                    {
                        if (getUpTimer == 0)
                        {
                            if (InputController.IsAnyKeyPressed())
                                somethingPressedOnce = true;

                            if (AnimationState[State].Frame >= 5) AnimationState[State].Frame = 5;
                        }
                        else
                        {
                            getUpTimer = Math.Max(getUpTimer - 1, 0);
                            if (AnimationState[State].Frame >= 3) AnimationState[State].Frame = 3;
                        }
                    }

                    if (AnimationState[State].IsDone)
                        State = PlayerState.Idle;
                }
                else
                {
                    AnimationState[State].Reset();
                }
            }

            var obstacle = this.CollisionBounds<Obstacle>().FirstOrDefault();
            if (obstacle != null)
            {
                State = PlayerState.Dead;
            }

            var waterTile = Collisions.TileAt(X, Y + 4, "WATER");

            if ((!inWater && waterTile != null) || (inWater && waterTile == null))
                new WaterSplashEmitter(new Vector2(X, Bottom), MainGame.Camera.Room);

            inWater = waterTile != null;

            if (State != PlayerState.Dead && State != PlayerState.StandUp)
            {
                var savePoint = this.CollisionBounds<SavePoint>().FirstOrDefault();
                if (savePoint != null)
                {
                    savePoint.SaveHere();
                }

                var item = this.CollisionBounds<Item>().FirstOrDefault();
                if (item != null && !item.IsTaken)
                {
                    YVel = 0;
                    gotItem = item;                    
                    gotItemTimer = maxGotItemTimer;
                    gotItemPostTimer = maxGotItemPostTimer;
                    State = PlayerState.GotItem;
                    gotItem.Take();
                    ControlsEnabled = false;
                    return;
                }

                for (var i = MainGame.Camera.Room.X; i < MainGame.Camera.Room.X + MainGame.Camera.Room.Width; i += G.T)
                {
                    for (var j = MainGame.Camera.Room.Y; j < MainGame.Camera.Room.Y + MainGame.Camera.Room.Height; j += G.T)
                    {
                        var t = Collisions.TileAt(i, j, "FG");
                        if (t == null)
                            continue;                        

                        if (M.Euclidean(Center, new Vector2(i + 4, j + 4)) > 32)
                        {
                            if (!t.IsSolid)
                            {
                                if (t.Type == TileType.Card_A)
                                {
                                    new AnimationEffect(new Vector2(i + 4, j + 4), 4, MainGame.Camera.Room);
                                    t.IsSolid = true;
                                    t.IsVisible = true;
                                }
                            }
                        }
                        else
                        {
                            if (Abilities.HasFlag(PlayerAbility.CARD_A) && t.Type == TileType.Card_A)
                            {
                                if (t.IsSolid)
                                {
                                    new AnimationEffect(new Vector2(i + 4, j + 4), 4, MainGame.Camera.Room);
                                }

                                t.IsSolid = false;
                                t.IsVisible = false;
                            }
                        }
                    }
                }

                if (inWater && !Abilities.HasFlag(PlayerAbility.DIVE))
                {
                    var underWater = Collisions.TileAt(X, Y - 4, "WATER") != null;
                    if (underWater)
                    {
                        oxygen = Math.Max(oxygen - 1, 0);
                        if (oxygen == 0)
                        {                            
                            ControlsEnabled = false;
                            if (onGround)
                            {
                                State = PlayerState.Dead;
                                return;
                            }
                        }
                    }
                    else
                    {
                        oxygen = Math.Min(oxygen + 2, maxOxygen);
                    }
                }
                else
                {
                    oxygen = Math.Min(oxygen + 2, maxOxygen);
                }

                if (State != PlayerState.Climb && State != PlayerState.GotItem)
                {

                    if (kLeft && !kRight)
                    {
                        Direction = PlayerDirection.Left;
                        if (onGround)
                        {
                            State = PlayerState.Walk;
                            XVel = Math.Max(XVel - .16f, -1.2f);
                        }
                        else
                        {
                            if (state != PlayerState.Hover)
                                State = PlayerState.Jump;
                            if (XVel > -1.2f)
                                XVel = Math.Max(XVel - .06f, -1.2f);
                        }
                    }
                    if (kRight && !kLeft)
                    {
                        Direction = PlayerDirection.Right;
                        if (onGround)
                        {
                            State = PlayerState.Walk;
                            XVel = Math.Min(XVel + .16f, 1.2f);
                        }
                        else
                        {
                            if (state != PlayerState.Hover)
                                State = PlayerState.Jump;
                            if (XVel < 1.2f)
                                XVel = Math.Min(XVel + .06f, 1.2f);
                        }
                    }

                    if ((!kLeft && !kRight) || (kLeft && kRight))
                    {
                        if (onGround)
                        {
                            XVel *= .6f;
                            if (Math.Abs(XVel) < .15f)
                            {
                                XVel = 0;
                                State = PlayerState.Idle;
                            }
                        }
                        else
                        {
                            XVel *= .9f;
                        }
                    }
                }

                if (State == PlayerState.Jump)
                {
                    if (YVel < 0 && AnimationState[State].Frame >= 2)
                        AnimationState[State].Frame = 2;
                    if (YVel >= 0 && AnimationState[State].Frame < 3)
                        AnimationState[State].Frame = 3;                    
                }

                if (State == PlayerState.Jump || State == PlayerState.Hover)
                {
                    if (Abilities.HasFlag(PlayerAbility.WALL_GRAB))
                    {
                        //if (OnWall() && ((leftTimer > 0 && Direction == PlayerDirection.Left) || (rightTimer > 0 && Direction == PlayerDirection.Right)))
                        if (OnWall() && ((kLeft && Direction == PlayerDirection.Left) || (kRight && Direction == PlayerDirection.Right)) && !kJumpHolding)
                        {
                            State = PlayerState.Climb;
                        }
                    }
                }


                yGrav = inWater ? yGravWater : yGravAir;
                xMax = inWater ? xVelMaxWater : xVelMaxAir;
                yMax = inWater ? yVelMaxWater : yVelMaxAir;

                onGround = YVel >= 0 && this.CollisionSolidTile(0, yGrav);

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
                    
                    if (kDown)
                        wasOnWallTimer = 10;

                    if (!((kLeft && Direction == PlayerDirection.Left) || (kRight && Direction == PlayerDirection.Right)) && !kDown && !kAction)
                        grabTimer = Math.Max(grabTimer - 1, 0);
                    else
                    {
                        grabTimer = 20;
                        leftTimer = maxDirectionGrabTimer;
                        rightTimer = maxDirectionGrabTimer;
                    }

                    if (!OnWall() || grabTimer == 0)
                    {
                        ResetJumps();
                        State = PlayerState.Jump;
                        AnimationState[State].Frame = 2;
                    }
                    XVel = 0;

                    if (Direction == PlayerDirection.Left)
                        X = M.Div(X, G.T) * G.T + 3.5f;
                    if (Direction == PlayerDirection.Right)
                        X = M.Div(X, G.T) * G.T + 4f;

                    if (kDown)
                    {
                        YVel = Math.Min(YVel + .2f, .5f);
                    }
                    else
                    {
                        YVel = -yGrav;
                    }

                    if (kDown)
                        AnimationState[State].Reset();

                    if ((Direction == PlayerDirection.Right && kLeft) || (Direction == PlayerDirection.Left && kRight)
                        || kJumpPressed)
                    {
                        Direction = Direction.Reverse();

                        if ((kLeft && Direction == PlayerDirection.Left) || (kRight && Direction == PlayerDirection.Right))
                        {
                            XVel = 1.5f * Math.Sign((int)Direction);
                            YVel = -2f;
                        }
                        else
                        {                            
                            XVel = (OnWallEdge() ? 0 : .5f) * Math.Sign((int)Direction);
                            YVel = -1.5f;
                        }

                        ResetJumps();
                        if (!kJumpPressed)
                            jumps = Math.Max(jumps - 1, 0);
                        kJumpPressed = false;

                        State = PlayerState.Jump;
                        AnimationState[State].Frame = 3;
                    }
                }
                else
                {
                    wasOnWallTimer = Math.Max(wasOnWallTimer - 1, 0);
                }

                if (kJumpPressed && jumps == 1 && Abilities.HasFlag(PlayerAbility.DOUBLE_JUMP))
                    new AnimationEffect(new Vector2(X, Bottom), 1, MainGame.Camera.Room);

                if (kJumpReleased)
                    jumped = false;

                if (kJumpHolding)
                {
                    if (jumps > 0)
                    {
                        jumpPowerTimer = Math.Max(jumpPowerTimer - 1, 0);
                        if (jumpPowerTimer == 0)
                        {
                            jumped = true;
                        }
                        if (!jumped)
                        {
                            YVel = -2f;
                            State = PlayerState.Jump;
                            onGround = false;
                        }
                    }
                }
                else
                {
                    if (kJumpReleased && !OnWall())
                    {
                        jumps = Math.Max(jumps - 1, 0);
                        jumpPowerTimer = maxJumpPowerTimer;
                    }
                }

                if (Abilities.HasFlag(PlayerAbility.JETPACK))
                {
                    if ((State == PlayerState.Jump || State == PlayerState.Walk || State == PlayerState.Idle) && (kUp || kDown) && !kJumpHolding && drill == null)
                    {
                        if (!inWater && !onGround && wasOnWallTimer == 0)
                        {
                            if (hoverPower > 5 && YVel >= 0)
                            {                                
                                YVel = -yGrav;
                                State = PlayerState.Hover;
                            }
                        }
                    }

                    if (kUp || kDown)
                    {
                        hoverButtonTimeout = 5;
                    }
                }

                hoverButtonTimeout = Math.Max(hoverButtonTimeout - 1, 0);

                if (state == PlayerState.Hover)
                {
                    if (hoverPower % 3 == 0)
                    {
                        var o = (hoverPower % 6 == 0) ? -3 : 3;                        
                        var eff = new AnimationEffect(new Vector2(X + o, Bottom), 3, MainGame.Camera.Room);
                        eff.Depth = depth - .0001f;
                        eff.XVel = -XVel * .1f;
                        eff.YVel = .5f + .5f * YVel;
                    }

                    yGrav = 0;
                    if (kUp && !kDown)
                    {
                        YVel = Math.Max(YVel - .03f, -1.5f);
                    } else if (kDown && !kUp)
                    {
                        YVel = Math.Min(YVel + .03f, 1.5f);
                    } else
                    {
                        YVel *= .8f;                        
                    }
                    
                    hoverTimeout = 30;
                    hoverPower = Math.Max(hoverPower - 1, 0);

                    if (hoverButtonTimeout == 0 || hoverPower == 0 || drill != null)
                        state = PlayerState.Jump;
                }
                else
                {
                    hoverTimeout = Math.Max(hoverTimeout - 1, 0);
                    if (hoverTimeout == 0)
                    {
                        hoverPower = (int)Math.Min(hoverPower + 2, maxHoverPower);
                    }
                }

                /* DRILL */
                if (Abilities.HasFlag(PlayerAbility.DRILL))
                {
                    if (kActionPressed)
                    {
                        if (drill == null)
                        {
                            drill = new Drill(Position, MainGame.Camera.Room);                            
                            if (Direction == PlayerDirection.Right)
                                drill.Angle = 0;
                            if (Direction == PlayerDirection.Left)
                                drill.Angle = 180;
                            if (kUp)
                                drill.Angle = 270;
                            if (kDown)
                                drill.Angle = 90;
                        }
                    }

                    if (drill != null && drill.IsAlive)
                    {
                        if (!drill.IsDrilling)
                        {
                            if (drill.Angle != 90 && drill.Angle != 270)
                            {
                                if (Direction == PlayerDirection.Right)
                                    drill.Angle = 0;
                                if (Direction == PlayerDirection.Left)
                                    drill.Angle = 180;
                            }
                            else
                            {
                                if (kRight)
                                    drill.Angle = 0;
                                if (kLeft)
                                    drill.Angle = 180;
                            }

                            if (kUp)
                                drill.Angle = 270;
                            else if (kDown)
                                drill.Angle = 90;
                            else
                            {
                                if (Direction == PlayerDirection.Right)
                                    drill.Angle = 0;
                                if (Direction == PlayerDirection.Left)
                                    drill.Angle = 180;
                            }
                        }

                        if (drill.IsDrilling)
                        {
                            ResetJumps();
                            hoverPower = (int)maxHoverPower;

                            State = PlayerState.Jump;
                            AnimationState[State].Frame = YVel < 0 ?  2 : 4;

                            XVel = M.LengthDirX(drill.Angle) * 2f;
                            YVel = M.LengthDirY(drill.Angle) * 2f;
                            if (drill.Angle == 90)
                                YVel = 3f;
                            yGrav = 0;
                        }

                        int drillOffY = (drill.Angle == 270) ? -2 : ((drill.Angle == 90) ? 1 : 0);

                        if (!onGround)
                            drillOffY += 2;

                        drill.Position += new Vector2(XVel, YVel);

                        drillTargetPosition = Position + new Vector2(XVel, YVel + drillOffY) + new Vector2(M.LengthDirX(drill.Angle) * 8, M.LengthDirY(drill.Angle) * 8);
                        drill.Position = drill.Position + new Vector2((drillTargetPosition.X - drill.X) / 3f, (drillTargetPosition.Y - drill.Y) / 3f);
                        if (!drill.IsDrilling && !kAction)
                        {
                            drill?.Destroy();
                            drill = null;
                        }

                    }
                    else
                    {
                        drill?.Destroy();
                        drill = null;
                    }
                }


                if (inWater)
                {
                    ResetJumps();
                }

                if (State == PlayerState.GotItem)
                {
                    XVel *= .5f;
                    if (gotItem != null)
                    {
                        gotItem.Position = Position + new Vector2(.5f * (int)Direction, -5 - 8 * (1 - (float)gotItemTimer / (float)(maxGotItemTimer)));
                        gotItemTimer = Math.Max(gotItemTimer - 1, 0);
                        if (gotItemTimer == 0 && itemMsgBox == null)
                        {
                            itemMsgBox = new MessageBox(16, 16, gotItem.Text);                            
                            itemMsgBox.OnFinished += () =>
                            {
                                ControlsEnabled = true;
                                state = PlayerState.Idle;
                                MainGame.Camera.Flash();
                                MainGame.SaveGame.Items.Add(gotItem.ID);
                                for (var i = 0; i < 8; i++)
                                {
                                    var eff = new AnimationEffect(new Vector2(gotItem.Center.X - 8 + RND.Next * 16, gotItem.Center.Y - 8 + RND.Next * 16), 0, gotItem.Room);
                                    eff.Delay = i * 8;
                                }
                                gotItem.Destroy();
                                gotItem = null;
                                itemMsgBox = null;
                            };
                        }
                    }
                    else
                    {
                        gotItemPostTimer = Math.Max(gotItemPostTimer - 1, 0);
                    }                    
                }

                // movement & collision

                YVel += yGrav;

                XVel = Math.Sign(XVel) * Math.Min(Math.Abs(XVel), xMax);
                YVel = Math.Sign(YVel) * Math.Min(Math.Abs(YVel), yMax);

                if (!this.CollisionSolidTile(XVel, 0) && this.CollisionPoint<Room>(Left + XVel, Y).Count > 0 && this.CollisionPoint<Room>(Right + XVel, Y).Count > 0)
                {
                    X += XVel;
                }
                else
                {
                    XVel = 0;
                }
                if (!this.CollisionSolidTile(0, YVel) && this.CollisionPoint<Room>(X, Top + YVel).Count > 0 && this.CollisionPoint<Room>(X, Bottom + YVel).Count > 0)
                {
                    Y += YVel;
                }
                else
                {
                    if (YVel >= 0)
                    {
                        Y = M.Div(Y + YVel + yGrav, (float)G.T) * G.T;
                        if (State == PlayerState.Jump && drill == null)
                        {
                            State = PlayerState.Idle;
                        }
                    }

                    YVel = 0;
                }
            }
            else
            {                
                if (State == PlayerState.Dead)
                {
                    depth = G.D_PLAYER_DEAD;
                    deadTimer = Math.Max(deadTimer - 1, 0);

                    if (deadTimer == 110)
                    {
                        //var eff = new TextureBurstEmitter(GameResources.Player[32], Position, new Vector2(1.25f, 1f), .1f, 60);
                        var eff = new TextureBurstEmitter(GameResources.Player[32], Position, new Vector2(1.3f, 1f), .1f, 60);
                        eff.Depth = G.D_PLAYER_DEAD + .0001f;
                    }

                    if (deadTimer > 110)
                        AnimationState[State].Frame = 0;

                    if (deadTimer == 0)
                    {
                        MainGame.ReloadLevel();
                    }
                }
            }
        }

        public override void Draw(SpriteBatch sb)
        {
            AnimationState[State].Draw(sb, Position, new Vector2(8), new Vector2((int)Direction, 1), Color.White, 0, depth);

            if (oxygen < maxOxygen && State != PlayerState.Dead)
                oxygenAlpha = 2;
            else
                oxygenAlpha = Math.Max(oxygenAlpha - .1f, 0);

            if (oxygenAlpha > 0)
            {
                var o = (float)Math.Floor((1 - ((float)oxygen) / ((float)maxOxygen)) * 16);
                var top = o;
                var bottom = 16 - o;
                //var px = -8;
                //var py = -24;
                var px = -16;
                var py = -16;

                sb.Draw(GameResources.Oxygen[0], Position + new Vector2(px, py), null, new Color(Color.White, oxygenAlpha), 0, Vector2.Zero, Vector2.One, SpriteEffects.None, G.D_FG + .001f);
                sb.Draw(GameResources.Oxygen[1], Position + new Vector2(px, py + (int)o), new Rectangle(0, (int)top, 16, (int)bottom), new Color(Color.White, oxygenAlpha), 0, Vector2.Zero, Vector2.One, SpriteEffects.None, G.D_FG + .001f);
                if (o > 1 && o < 15)
                {
                    sb.Draw(GameResources.Oxygen[2], Position + new Vector2(px, py + o), null, new Color(Color.White, oxygenAlpha), 0, Vector2.Zero, Vector2.One, SpriteEffects.None, G.D_FG + .002f);
                }
            }
            
            if (Abilities.HasFlag(PlayerAbility.JETPACK) && State != PlayerState.Dead)
            {
                if (hoverPower < maxHoverPower)
                {
                    hoverAlpha = Math.Min(hoverAlpha + .05f, 2);
                }
                else
                {
                    hoverAlpha = Math.Max(hoverAlpha - .1f, 0);
                }

                if (hoverAlpha > 0)
                {
                    Vector2 off = new Vector2(-.5f);
                    float p = hoverPower / maxHoverPower;
                    sb.DrawLine(Position + new Vector2(-6, -10) + off, Position + new Vector2(-6 + 12, -10) + off, new Color(Color.Black, hoverAlpha * .25f), G.D_FG + .001f);
                    if (p > 0)
                        sb.DrawLine(Position + new Vector2(-6, -10) + off, Position + new Vector2(-6 + 12 * p, -10) + off, new Color(Color.White, hoverAlpha * p), G.D_FG + .002f);
                }
            }

            //sb.DrawRectangle(Position + BBox, Color.White, false, G.D_PLAYER + .001f);
            //sb.DrawPixel(X, Y, Color.Red, G.D_PLAYER + .001f);
            //sb.DrawPixel(Center.X, Center.Y, Color.GreenYellow, G.D_PLAYER + .001f);
        }
    }
}