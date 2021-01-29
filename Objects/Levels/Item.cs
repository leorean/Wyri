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
        public bool IsTaken { get; private set; }
        float t;
        int effectTimeout;
        public string Text { get; private set; }

        public Item(Vector2 position, int type, Room room) : base(position, new Types.RectF(-4, -4, 8, 8), room)
        {
            Type = type;
            if (MainGame.SaveGame.Items.Contains(ID))
            {
                IsTaken = true;
                Destroy();
            }
        }

        public void Take()
        {
            if (IsTaken)
                return;
            IsTaken = true;

            var gc1 = GameResources.CollectabledisplayColor.ToHex();
            var gc2 = GameResources.ItemDisplayColor.ToHex();
            var gs = 5;
            
            switch (Type)
            {
                case 0:                    
                    Text = $"[color:{gc1},center:true,spd:{gs}]GOT A ~QUANTUM FRAGMENT~!";
                    if (MainGame.SaveGame.Collected == 0)
                    {
                        //Text += $"|[color:{gc2}]You need to find enough of them to go\n~back in time~!";
                        Text += $"|[color:{gc2}]Collect them all so you can\nfinally fix your ~time capsule~!";
                    }
                    MainGame.SaveGame.Collected++;
                    break;
                case 1:
                    MainGame.SaveGame.Abilities |= PlayerAbility.WALL_GRAB;
                    Text = $"[color:{gc2},center:true,spd:{gs}]GOT THE ~GECKO GLOVES~!|[color:{gc2}]You can now perform wall jumps!\nPress and hold ~LEFT~ or ~RIGHT~ while jumping\ntowards a wall.\nPress ~DOWN~ to slide down walls.";
                    break;
                case 2:
                    MainGame.SaveGame.Abilities |= PlayerAbility.MAP;
                    Text = $"[color:{gc2},center:true,spd:{gs}]GOT THE ~MAP SENSOR~!|[color:{gc2}]Press ~W~ to view the map.";
                    break;
                case 3:
                    MainGame.SaveGame.Abilities |= PlayerAbility.COMPASS;
                    Text = $"[color:{gc2},center:true,spd:{gs}]GOT THE ~COMPASS MODULE~!|The map now displays item locations.";
                    break;
                case 4:
                    MainGame.SaveGame.Abilities |= PlayerAbility.DOUBLE_JUMP;
                    Text = $"[color:{gc2},center:true,spd:{gs}]GOT THE ~SPARK BOOTS~!|Now you can perform a jump in mid-air.";
                    break;
                case 5:
                    MainGame.SaveGame.Abilities |= PlayerAbility.JETPACK;
                    Text = $"[color:{gc2},center:true,spd:{gs}]GOT THE ~JET PACK~!|[color:{gc2}]Press and hold ~UP~ or ~DOWN~ to hover.";
                    break;
                case 6:
                    MainGame.SaveGame.Abilities |= PlayerAbility.DRILL;
                    Text = $"[color:{gc2},center:true,spd:{gs}]GOT THE ~DRILL~!|[color:{gc2}]Press and hold ~S~ to drill certain blocks.";
                    break;
                case 7:
                    MainGame.SaveGame.Abilities |= PlayerAbility.DIVE;
                    Text = $"[color:{gc2},center:true,spd:{gs}]GOT THE ~GILLS~!|These allow you to breathe anywhere.";
                    break;
                case 8:
                    MainGame.SaveGame.Abilities |= PlayerAbility.CARD_A;
                    Text = $"[color:{gc2},center:true,spd:{gs}]GOT ~ACCESS CARD -A-~!|This will give you access to new areas.";
                    break;
                case 9:
                    MainGame.SaveGame.Abilities |= PlayerAbility.CARD_B;
                    Text = $"[color:{gc2},center:true,spd:{gs}]GOT ~ACCESS CARD -B-~!|This will give you access to new areas.";
                    break;
                case 10:
                    MainGame.SaveGame.Abilities |= PlayerAbility.CARD_C;
                    Text = $"[color:{gc2},center:true,spd:{gs}]GOT ~ACCESS CARD -C-~!|This will give you access to new areas.";
                    break;
                default:
                    throw new NotImplementedException("Type not implemented!");
            }
        }

        public override void Update()
        {
            if (IsTaken)
                return;

            t = (float)Math.Sin((MainGame.Ticks * .045f) % (2 * Math.PI)) * 1.5f;

            if (effectTimeout == 0)
            {
                new AnimationEffect(new Vector2(Center.X - 8 + RND.Next * 16, Center.Y - 8 + RND.Next * 16), 0, Room);
                effectTimeout = 20;
            }
            else
            {
                effectTimeout = Math.Max(effectTimeout - 1, 0);
            }
        }

        public override void Draw(SpriteBatch sb)
        {
            sb.Draw(GameResources.Items[Type], Position - new Vector2(8) + new Vector2(0, t), null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, G.D_ITEM);
        }
    }
}
