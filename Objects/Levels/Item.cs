﻿using Microsoft.Xna.Framework;
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

            var gc = "ffd700";
            var gs = 5;
            
            switch (Type)
            {
                case 0:
                    MainGame.SaveGame.Collected++;
                    Text = $"[color:{gc},center:true,spd:{gs}]Got a time crystal!";
                    break;
                case 1:
                    MainGame.SaveGame.Abilities |= PlayerAbility.WALL_GRAB;
                    Text = $"[color:{gc},center:true,spd:{gs}]Got the grappling gloves!|Use the arrow keys to hold onto\nwalls or slide down.";
                    break;
                case 2:
                    MainGame.SaveGame.Abilities |= PlayerAbility.MAP;
                    Text = $"[color:{gc},center:true,spd:{gs}]Got the map sensor!|Press 'W' to view the map.";
                    break;
                case 3:
                    MainGame.SaveGame.Abilities |= PlayerAbility.COMPASS;
                    Text = $"[color:{gc},center:true,spd:{gs}]Got the compass!|The map now displays all remaining items.";
                    break;
                default:
                    throw new NotImplementedException("Type not implemented!");
            }

            if (Type == 0) MainGame.SaveGame.Collected++;
            if (Type == 1) MainGame.SaveGame.Abilities |= PlayerAbility.WALL_GRAB;
            if (Type == 2) MainGame.SaveGame.Abilities |= PlayerAbility.MAP;
            if (Type == 3) MainGame.SaveGame.Abilities |= PlayerAbility.COMPASS;
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
