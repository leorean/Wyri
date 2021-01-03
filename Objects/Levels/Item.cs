using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Main;
using Wyri.Objects.Levels.Effects;
using Wyri.Util;

namespace Wyri.Objects.Levels
{
    public class Item : RoomObject
    {
        public int Type { get; }

        bool taken;

        float t;

        public Item(Vector2 position, int type, Room room) : base(position, new Types.RectF(-4, -4, 8, 8), room)
        {
            Type = type;
            if (MainGame.SaveGame.Items.Contains(ID))
            {
                taken = true;
                Destroy();
            }
        }

        public void Take()
        {
            if (taken)
                return;
            taken = true;
            MainGame.SaveGame.Items.Add(ID);

            MainGame.Camera.Flash();
            for (var i = 0; i < 15; i++)
            {
                var eff = new AnimationEffect(new Vector2(Center.X - 8 + RND.Next * 16, Center.Y - 8 + RND.Next * 16), 0, MainGame.Camera.Room);
                eff.Delay = i * 12;
            }

            if (Type == 0) MainGame.Player.Abilities |= PlayerAbility.WALL_GRAB;
            if (Type == 1) MainGame.Player.Abilities |= PlayerAbility.MAP;
            if (Type == 2) MainGame.Player.Abilities |= PlayerAbility.COMPASS;

            Destroy();
        }

        public override void Draw(SpriteBatch sb)
        {
            sb.Draw(GameResources.Items[Type], Position - new Vector2(8)+ new Vector2(0, t), null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, G.D_ITEM);            
        }

        public override void Update()
        {
            t = (float)Math.Sin((MainGame.Ticks * .03f) % (2 * Math.PI)) * 2;
        }
    }
}
