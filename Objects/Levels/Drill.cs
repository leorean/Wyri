using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wyri.Main;
using Wyri.Types;

namespace Wyri.Objects.Levels
{
    public class Drill : RoomObject, IDestroyOnRoomChange
    {
        public float Angle { get; set; }
        private float drawAngle;
        public bool IsAlive { get; private set; } = true;

        int drillTimeout;

        public Drill(Vector2 position, Room room) : base(position, new Types.RectF(-4, -7, 8, 14), room)
        {
            
        }

        public bool IsDrilling { get; private set; }

        public override void Update()
        {
            //spd = Math.Min(spd + .2f, 4f);

            //xVel = M.LengthDirX(Angle) * spd;
            //yVel = M.LengthDirY(Angle) * spd;

            //X += xVel;
            //Y += yVel;

            var tiles = this.CollisionTiles(0, 0);

            foreach(var t in tiles)
            {
                if (t.Type == TileType.DestroyBlock)
                {

                    t.IsSolid = false;
                    t.IsVisible = false;
                }
            }

            if (tiles.Where(t => t.Type == TileType.DestroyBlock).ToList().Count > 0)
            {
                drillTimeout = 15;                
            }

            IsDrilling = drillTimeout > 0;            
            drillTimeout = Math.Max(drillTimeout - 1, 0);

            if (!M.In(X, Room.X + 4, Room.X + Room.Width - 4) || !M.In(Y, Room.Y + 4, Room.Y + Room.Height - 4))
            {                
                Destroy();
            }
        }

        public override void Destroy()
        {
            IsAlive = false;
            base.Destroy();
        }

        public override void Draw(SpriteBatch sb)
        {
            //sb.DrawRectangle(Position + BBox, Color.Red, false, 1);

            if (Angle != 90 && Angle != 270)
                drawAngle = M.VectorToAngle(new Vector2(M.LengthDirX(Angle) * 3 + MainGame.Player.XVel, M.LengthDirY(Angle) * 3 + MainGame.Player.YVel));
            else
                drawAngle = Angle;
            
            sb.Draw(GameResources.Drill[0], Position, null, Color.White, M.DegToRad(drawAngle), new Vector2(8), Vector2.One, SpriteEffects.None, G.D_FG + .001f);
        }
    }
}
