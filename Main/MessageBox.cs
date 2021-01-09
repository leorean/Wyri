using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Wyri.Types;
using Object = Wyri.Objects.Object;

namespace Wyri.Main
{
    public class MessageBox : Object, IStayActive
    {
        enum State
        {
            FadeIn,
            Showing,
            FadeOut
        }
        private State state;

        float _x => MainGame.Camera.ViewX;
        float _y => MainGame.Camera.ViewY;
        float offx, offy;
        float x => _x + offx;
        float y => _y + offy;

        string text;
        float scale;

        int i;
        int length;

        public Action OnFinished;

        public MessageBox(string text, float x, float y)
        {
            this.text = text;
            length = text.Length;
            offx = x;
            offy = y;
        }

        public override void Update()
        {
            switch (state)
            {
                case State.FadeIn:
                    scale = Math.Min(scale + .05f, 1);
                    if (scale == 1)
                        state = State.Showing;
                    break;
                case State.Showing:
                    if ((MainGame.Ticks % 4) == 0)
                        i = Math.Min(i + 1, length);
                    if (i == length)
                    {
                        if (InputController.IsKeyPressed(Keys.A))
                        {
                            state = State.FadeOut;
                        }
                    }
                    break;
                case State.FadeOut:
                    scale = Math.Max(scale - .05f, 0);
                    if (scale == 0)
                    {
                        OnFinished?.Invoke();
                        Destroy();
                    }
                    break;
            }
        }

        public override void Draw(SpriteBatch sb)
        {
            var drawOffset = new Vector2(112, 24);
            sb.Draw(GameResources.MessageBox, new Vector2(x, y) + drawOffset, new Rectangle(0, 0, 224, 48), Color.White, 0, drawOffset, new Vector2(scale), SpriteEffects.None, G.D_UI);
            if (state == State.Showing)
            {
                var t = text.Substring(0, i);
                sb.DrawString(GameResources.Font, t, new Vector2(x + 2, y + 2), Color.White, 0, Vector2.Zero, .25f, SpriteEffects.None, G.D_UI + .0001f);

                if (i == length)
                {
                    float a = Math.Abs((float)Math.Sin(MainGame.Ticks * .03f) % (float)(2 * Math.PI));
                    sb.Draw(GameResources.MessageBox, new Vector2(x + 128 - 24, y + 40), new Rectangle(0, 48, 16, 16), new Color(Color.White, a), 0, Vector2.Zero, Vector2.One, SpriteEffects.None, G.D_UI + .0001f);
                }
            }
        }
    }
}
