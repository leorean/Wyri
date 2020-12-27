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
        Jump
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

        private float xVel, yVel, yGrav;

        const float yGravAir = .1f;
        const float yGravWater = .01f;

        public Player(Vector2 position) : base(position, new RectF(-4, -3, 8, 10))
        {
            DrawOffset = new Vector2(7f, 7f);            
            AnimationState.Add(PlayerState.Idle, new Animation(GameResources.Player, 0, 4, .1f));
            AnimationState.Add(PlayerState.Walk, new Animation(GameResources.Player, 6, 12, .2f));
            AnimationState.Add(PlayerState.Jump, new Animation(GameResources.Player, 12, 12, 0));
        }

        public override void Update()
        {
            // input

            var kLeft = InputController.IsKeyPressed(Keys.Left);
            var kRight = InputController.IsKeyPressed(Keys.Right);

            if (kLeft)
            {
                Direction = PlayerDirection.Left;
                State = PlayerState.Walk;
                xVel = -.5f;
            }
            if (kRight)
            {
                Direction = PlayerDirection.Right;
                State = PlayerState.Walk;
                xVel = .5f;
            }

            if (!kLeft && !kRight)
            {
                State = PlayerState.Idle;
                xVel = 0;
            }

            // logic

            AnimationState[State].Update();

            var room = CollisionExtensions.CollisionPoint<Room>(X, Y).FirstOrDefault();
            if (room != null)
            {
                if (room != MainGame.Camera.Room)
                {
                    MainGame.Camera.Room = room;
                }
            }

            // movement & collision

            yVel += yGrav;

            if (!CollisionTile(xVel, 0))
            {
                X += xVel;
            }
            else
            {
                xVel = 0;
            }
            if (!CollisionTile(0, yVel))
            {
                Y += yVel;
            }
            else
            {
                yVel = 0;
            }

        }

        /*
         *         public T CollisionTile<T>(float x, float y, int layer = -1)
        {
            int tx = MathUtil.Div(x, Globals.T);
            int ty = MathUtil.Div(y, Globals.T);

            if (layer == -1)
                layer = GameMap.FG_INDEX;

            var tile = LayerData[layer].Get(tx, ty);

            if (typeof(T) == typeof(bool))
            {
                if (tile != null && tile.TileOptions.Solid)
                    return (T)(object)true;

                return (T)(object)false;
            }
            else if (typeof(T) == typeof(Tile))
            {
                return (T)(object)tile;
            }

            return default(T);
        }
         */


        bool CollisionTile (float xo, float yo)
        {
            var grid = MainGame.Map.LayerData["FG"];
            
            for (float i = M.Div(Left, G.T) - G.T; i < M.Div(Right, G.T) + G.T; i++)
            {
                for (float j = M.Div(Top, G.T) - G.T; j < M.Div(Bottom, G.T) + G.T; j++)
                {
                    var t = grid[(int)i, (int)j];                    
                    if (t == null || !t.IsSolid)
                        continue;

                    var tileRect = new RectF(i * G.T, j * G.T, G.T, G.T);
                    if ((BBox + new Vector2(Center.X + xo, Center.Y + yo)).Intersects(tileRect))
                        return true;
                }
            }

            return false;
        }

        public override void Draw(SpriteBatch sb)
        {
            AnimationState[State].Draw(sb, Position, DrawOffset, new Vector2((int)Direction, 1), Color.White, 0, G.D_PLAYER);
            
            sb.DrawRectangle(Position + BBox, Color.White, false, G.D_PLAYER + .001f);
            sb.DrawPixel(X, Y, Color.Red, G.D_PLAYER + .001f);
            sb.DrawPixel(Center.X, Center.Y, Color.GreenYellow, G.D_PLAYER + .001f);            
        }
    }
}
