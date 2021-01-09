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
