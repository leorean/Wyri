using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Main;
using Wyri.Objects.Levels.Effects;
using Wyri.Types;
using Wyri.Util;

namespace Wyri.Objects.Levels
{
    public class SavePoint : RoomObject
    {
        Animation animation;

        float t;
        float yo;
        int stayTimer;

        float a;
        bool canSave = false;

        enum State
        {
            Default,
            SaveUp,
            SaveDown
        }

        private State s;

        public void SaveHere()
        {
            if (!canSave)
                return;

            s = State.SaveUp;

            for(var i = 0; i < 5; i++)
            {
                var eff = new AnimationEffect(new Vector2(Center.X - 8 + RND.Next * 16, Center.Y - 8 + RND.Next * 16), 0, MainGame.Camera.Room);
                eff.Delay = i * 12;
            }

            MainGame.Save(Position + new Vector2(8, 0));

            t = 0;
            canSave = false;
        }

        public SavePoint(Vector2 position, Room room) : base(position, new RectF(4, 4, 8, 8), room)
        {
            animation = new Animation(GameResources.Save, 1, 6, .2f, true);

        }
        public override void Update()
        {
            animation.Update();
            animation.AnimationSpeed = s == State.Default ? .17f : .33f;

            if (!canSave && !this.CollisionBounds(MainGame.Player))
            {
                canSave = true;
            }

            switch (s)
            {
                case State.Default:
                    t = (t + .1f) % (2 * (float)Math.PI);
                    yo = (float)Math.Sin(t) - 1;
                    stayTimer = 30;
                    a = .5f;
                    break;
                case State.SaveUp:
                    a = Math.Min(a + .01f, 1);
                    t = Math.Max(t - .1f, -5);
                    if (t == -5)
                    {
                        stayTimer = Math.Max(stayTimer - 1, 0);
                        if (stayTimer == 0)
                            s = State.SaveDown;
                    }
                    yo = t;
                    break;
                case State.SaveDown:
                    a = Math.Max(a - .01f, .5f);
                    t = Math.Min(t + .1f, 0);
                    if (t == 0)
                        s = State.Default;
                    yo = t;
                    break;
            }

        }

        public override void Draw(SpriteBatch sb)
        {
            sb.Draw(GameResources.Save[0], Position + new Vector2(0, 0), null, new Color(Color.White, a), 0, Vector2.Zero, Vector2.One, SpriteEffects.None, G.D_FG + .001f);
            animation.Draw(sb, Position + new Vector2(0, yo), Vector2.Zero, Vector2.One, Color.White, 0, G.D_PLAYER - .001f);
        }
        
    }
}
