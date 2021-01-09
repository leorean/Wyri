using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Wyri.Types;
using Wyri.Util;
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

        List<(string, bool, Color, int)> texts;
        float scale;

        int i;
        int length;

        int page = 0;

        public Action OnFinished;

        public MessageBox(float x, float y, string input)
        {
            texts = new List<(string, bool, Color, int)>();

            var rows = input.Split('|');

            foreach(var row in rows)
            {
                string text = "";
                bool center = false;
                Color color = Color.White;
                int spd = 3;
                
                if (row.Contains('[') && row.Contains(']'))
                {
                    var optionString = row.Substring(row.IndexOf('['), row.IndexOf(']') + 1).Replace("[", "").Replace("]", "");
                    var options = optionString.Split(',');                    
                    foreach (var option in options)
                    {
                        var o = option.Split(':');
                        var key = o[0];
                        var val = o[1];

                        switch (key)
                        {
                            case "color":
                                color = Colors.FromHex(val);
                                break;
                            case "center":
                                center = bool.Parse(val);
                                break;
                            case "spd":
                                spd = int.Parse(val);
                                break;
                        }
                    }
                    text = row.Replace(optionString, "").Replace("[", "").Replace("]", "");
                }
                else
                {
                    text = row;
                }

                texts.Add((text, center, color, spd));
            }

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

                    length = texts[page].Item1.Length;

                    if ((MainGame.Ticks % texts[page].Item4) == 0)
                        i = Math.Min(i + 1, length);
                    if (i == length)
                    {
                        if (InputController.IsKeyPressed(Keys.A, KeyState.Pressed) || InputController.IsKeyPressed(Keys.S, KeyState.Pressed))
                        {
                            if (page < texts.Count - 1)
                            {
                                i = 0;
                                page++;
                            }
                            else
                            {
                                state = State.FadeOut;
                            }
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
                var text = texts[page].Item1;
                var t = text.Substring(0, i);

                var tx = 0f;
                var ty = 0f;
                if (texts[page].Item2)
                {
                    tx = .5f * (224 - GameResources.Font.MeasureString(t).X * .25f) - 4;
                    ty = .5f * (48 - GameResources.Font.MeasureString(t).Y * .25f) - 4;
                }
                sb.DrawString(GameResources.Font, t, new Vector2(x + 2 + tx, y + 2 + ty), texts[page].Item3, 0, Vector2.Zero, .25f, SpriteEffects.None, G.D_UI + .0001f);
                if (i == length)
                {   
                    float a = Math.Abs((float)Math.Sin(MainGame.Ticks * .04f) % (float)(2 * Math.PI)) * 2;
                    sb.Draw(GameResources.MessageBox, new Vector2(x + 128 - 24, y + 40), new Rectangle(0, 48, 16, 16), new Color(Color.White, a), 0, Vector2.Zero, Vector2.One, SpriteEffects.None, G.D_UI + .0001f);
                }
            }
        }
    }
}
