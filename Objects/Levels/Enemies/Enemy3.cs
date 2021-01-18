using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        float spd = .5f;

        private Direction direction;
        private Direction prevDirection;

        public Enemy3(Vector2 position, Direction dir, Room room) : base(position, new Types.RectF(-4, -4, 8, 8), room)
        {
            direction = dir;
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

            //var tile = Collisions.TileAt(M.Div(X + xVel, G.T) * G.T + 4, M.Div(Y + yVel, G.T) * G.T + 4, "FG");
            var tile = this.CollisionTiles(xVel, yVel, false).FirstOrDefault().Item1;

            if (tile != null)
            {
                if (tile.IsSolid)
                {
                    if (direction == Direction.Left) direction = Direction.Right;
                    else if (direction == Direction.Right) direction = Direction.Left;
                    else if (direction == Direction.Up) direction = Direction.Down;
                    else if (direction == Direction.Down) direction = Direction.Up;
                }
                else
                {
                    if (tile.Type == Types.TileType.Move_Up) direction = Direction.Up;
                    else if (tile.Type == Types.TileType.Move_Down) direction = Direction.Down;
                    else if (tile.Type == Types.TileType.Move_Left) direction = Direction.Left;
                    else if (tile.Type == Types.TileType.Move_Right) direction = Direction.Right;
                }
            }

            if (direction != prevDirection)
            {
                X = M.Div(X, G.T) * G.T + 4;
                Y = M.Div(Y, G.T) * G.T + 4;
            }

            X += xVel;
            Y += yVel;

            prevDirection = direction;
        }

        public override void Draw(SpriteBatch sb)
        {
            sb.DrawRectangle(Position + BBox, Color.Red, false, .8f);            
        }
    }
}
 