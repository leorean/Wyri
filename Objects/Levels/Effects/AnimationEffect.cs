using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Main;
using Wyri.Objects;
using Wyri.Types;

namespace Wyri.Objects.Levels.Effects
{
    public class AnimationEffect : RoomObject, IDestroyOnRoomChange
    {
        private Animation animation;

        public int Delay { get; set; } = 0;

        public AnimationEffect(Vector2 position, int type, Room room) : base(position, new RectF(0, 0, 16, 16), room)
        {
            if (type == 00) animation = new Animation(GameResources.Effects, 00, 7, .3f, false);
            if (type == 01) animation = new Animation(GameResources.Effects, 08, 7, .4f, false);
            if (type == 02) animation = new Animation(GameResources.Effects, 16, 8, .3f, false);
        }

        public override void Update()
        {
            Delay = Math.Max(Delay - 1, 0);

            if (Delay > 0)
                return;

            animation.Update();
            if (animation.IsDone)
                Destroy();
        }

        public override void Draw(SpriteBatch sb)
        {
            if (Delay == 0)
                animation.Draw(sb, Position, new Vector2(8), Vector2.One, Color.White, 0, G.D_EFFECT);
        }

    }
}
