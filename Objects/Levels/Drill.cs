using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wyri.Main;
using Wyri.Objects.Levels.Effects;
using Wyri.Types;

namespace Wyri.Objects.Levels
{
    public class Drill : RoomObject, IStayActive
    {
        public float Angle { get; set; }
        private float drawAngle;
        public bool IsAlive { get; private set; } = true;

        int drillTimeout;
        Animation drillAnimation;

        public Drill(Vector2 position, Room room) : base(position, new Types.RectF(-4, -7, 8, 14), room)
        {
            drillAnimation = new Animation(GameResources.Drill, 0, 4, .4f);
        }

        public bool IsDrilling { get; private set; }

        public override void Update()
        {
            var tiles = this.CollisionTiles(0, 0);

            foreach(var t in tiles)
            {
                if (t.Item1.Type == TileType.DestroyBlock)
                {

                    t.Item1.IsSolid = false;
                    t.Item1.IsVisible = false;

                    var tx = M.Div(X, G.T);
                    var ty = M.Div(Y, G.T);
                    new TextureBurstEmitter(GameResources.Tiles[t.Item1.ID], t.Item2 + new Vector2(4, 4), new Vector2(1), Room);

                }
            }

            if (tiles.Where(t => t.Item1.Type == TileType.DestroyBlock).ToList().Count > 0)
            {
                drillTimeout = 10;                
            }

            IsDrilling = drillTimeout > 0;            
            drillTimeout = Math.Max(drillTimeout - 1, 0);

            if (MainGame.Player.State == PlayerState.Dead)
                Destroy();

            //if (!M.In(X, Room.X + 4, Room.X + Room.Width - 4) || !M.In(Y, Room.Y + 4, Room.Y + Room.Height - 4))
            //{                
            //    Destroy();
            //}
        }

        public override void Destroy()
        {
            IsAlive = false;
            base.Destroy();
        }

        public override void Draw(SpriteBatch sb)
        {
            if (Angle != 90 && Angle != 270)
                drawAngle = M.VectorToAngle(new Vector2(M.LengthDirX(Angle) * 3 + MainGame.Player.XVel, M.LengthDirY(Angle) * 3 + MainGame.Player.YVel));
            else
                drawAngle = Angle;

            drillAnimation.Update();
            drillAnimation.Draw(sb, Position, new Vector2(8), Vector2.One, Color.White, M.DegToRad(drawAngle), G.D_FG - .001f);            
        }
    }
}
