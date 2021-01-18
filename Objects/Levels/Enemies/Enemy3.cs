using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wyri.Main;
using Wyri.Types;

namespace Wyri.Objects.Levels.Enemies
{
    public class Enemy3 : Obstacle
    {
        public enum Direction
        {
            Left = -1,
            Right = 1,
            Up = -2,
            Down = 2
        }

        float xVel, yVel;
        float txVel, tyVel;
        const float spd = .5f;
        const float acc = .02f;

        private Direction direction;
        private Direction prevDirection;

        Animation animation;

        public Enemy3(Vector2 position, Direction dir, Room room) : base(position, new Types.RectF(-4, -4, 8, 8), room)
        {
            direction = dir;
            animation = new Animation(GameResources.Enemy3, 0, 4, .4f);
        }

        public override void Update()
        {

            switch (direction)
            {
                case Direction.Left:
                    xVel = -spd;
                    yVel = 0;
                    break;
                case Direction.Right:
                    xVel = spd;
                    yVel = 0;
                    break;
                case Direction.Up:
                    xVel = 0;
                    yVel = -spd;
                    break;
                case Direction.Down:
                    xVel = 0;
                    yVel = spd;
                    break;
            }
            var tile = this.CollisionTiles(0, 0, false).Where(t => t.Item1.IsSolid).FirstOrDefault().Item1;

            if (tile != null)
            {
                if (tile.IsSolid)
                {
                    if (direction == Direction.Left) direction = Direction.Right;
                    else if (direction == Direction.Right) direction = Direction.Left;
                    else if (direction == Direction.Up) direction = Direction.Down;
                    else if (direction == Direction.Down) direction = Direction.Up;
                    xVel *= -1;
                    yVel *= -1;
                    txVel = xVel;
                    tyVel = yVel;
                    if (direction != prevDirection)
                    {
                        X = M.Div(X, G.T) * G.T + 4;
                        Y = M.Div(Y, G.T) * G.T + 4;
                    }
                }
                else
                {                        
                    if (tile.Type == Types.TileType.Move_Up) direction = Direction.Up;
                    else if (tile.Type == Types.TileType.Move_Down) direction = Direction.Down;
                    else if (tile.Type == Types.TileType.Move_Left) direction = Direction.Left;
                    else if (tile.Type == Types.TileType.Move_Right) direction = Direction.Right;

                    //if ((int)prevDirection == -(int)direction)
                    //{
                    //    txVel = 0;
                    //    tyVel = 0;
                    //}
                }
            }
            

            txVel += (xVel - txVel) / 8f;
            tyVel += (yVel - tyVel) / 8f;

            X += txVel;
            Y += tyVel;

            prevDirection = direction;

            animation.Update();
        }

        public override void Draw(SpriteBatch sb)
        {            
            animation.Draw(sb, Position, new Vector2(16), new Vector2(Math.Sign((int)direction), 1), Color.White, 0, G.D_ENEMY);
        }
    }
}
 