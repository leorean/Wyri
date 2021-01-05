using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Main;

namespace Wyri.Objects.Levels
{
    public class TriggerBlock : RoomObject
    {
        public int Type { get; private set; }
        public int TileID { get; private set; }
        public bool On { get; set; }

        public TriggerBlock(Vector2 position, int tileID, bool on, Room room) : base(position, new Types.RectF(0, 0, 8, 8), room)
        {            
            TileID = on ? tileID - 1 : tileID;
            On = on;
        }

        public override void Update()
        {
            
        }

        public override void Draw(SpriteBatch sb)
        {


            sb.Draw(GameResources.Tiles[TileID + Convert.ToInt32(On)], Position, null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, G.D_FG);
        }
    }
}
