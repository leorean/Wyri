using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
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

        List<(string[], bool, Color, int, Dictionary<int, List<int>>)> textPages;
        float scale;

        int ticks;

        int index;
        List<int> length;
        int curLine;
        int page;
        
        public Action OnFinished;

        public MessageBox(float x, float y, string input)
        {
            textPages = new List<(string[], bool, Color, int, Dictionary<int, List<int>>)>();

            var rows = input.Split('|');

            foreach(var row in rows)
            {
                string text;
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

                var colorMap = new Dictionary<int, List<int>>();
                var lines = text.Split('\n');
                for (int l = 0; l < lines.Length; l++)
                {
                    List<int> colorMapForLine = new List<int>();
                    for (int i = 0; i < lines[l].Length; i++)
                    {
                        if (lines[l][i] == '~')
                        {
                            colorMapForLine.Add(i - colorMapForLine.Count);
                        }                        
                    }
                    colorMap.Add(l, colorMapForLine);
                    lines[l] = lines[l].Replace("~", "");
                }

                textPages.Add((lines, center, color, spd, colorMap));
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
                    
                    length = new List<int>();
                    for(var i = 0; i < textPages[page].Item1.Length; i++)
                    {
                        length.Add(textPages[page].Item1[i].Length);
                    }

                    if (curLine < textPages[page].Item1.Length)
                    {

                        if (index >= textPages[page].Item1[curLine].Length)
                        {
                            curLine++;
                            index = 0;                        
                        }

                        if ((MainGame.Ticks % textPages[page].Item4) == 0)
                            index = Math.Min(index + 1, textPages[page].Item1[curLine].Length);
                    }
                    
                    if (curLine == textPages[page].Item1.Length)
                    {
                        if (InputController.IsKeyPressed(Keys.A, KeyState.Pressed) || InputController.IsKeyPressed(Keys.S, KeyState.Pressed))
                        {
                            if (page < textPages.Count - 1)
                            {
                                curLine = 0;
                                index = 0;
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
                var textLines = textPages[page].Item1;

                if (curLine == textLines.Length)
                {
                    ticks = (ticks + 1) % 9000;
                    float a = (float)Math.Sin(ticks * .04f) % (float)(2 * Math.PI) * 2;
                    sb.Draw(GameResources.MessageBox, new Vector2(x + 128 - 24, y + 40 + a), new Rectangle(0, 48, 16, 16), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, G.D_UI + .0001f);
                }
                else
                {
                    ticks = 0;
                }

                for (int l = 0; l <= curLine; l++)
                {
                    if (l == textLines.Length && l > 0)
                        continue;

                    var curIndex = l == curLine ? index : textLines[l].Length;
                    var t = textLines[l].Substring(0, curIndex);

                    var tx = 0f;
                    var ty = 0f;

                    var yoff = GameResources.Font.MeasureString(t).Y * .25f * l;

                    if (textPages[page].Item2)
                    {
                        tx = .5f * (224 - GameResources.Font.MeasureString(t).X * .25f) - 4;
                        ty = .5f * (48 - GameResources.Font.MeasureString(t).Y * .25f) - 4;
                    }

                    bool inColorMode = false;
                    List<int> cm = textPages[page].Item5[l].ToList();

                    for (var i = 0; i < t.Length; i++)
                    {
                        var chr = t.Substring(i, 1);
                        if (cm.Count > 0)
                        {
                            if (i == cm.First())
                            {
                                inColorMode = !inColorMode;
                                cm.RemoveAt(0);
                            }
                        }

                        var col = inColorMode ? textPages[page].Item3 : Color.White;

                        var blub = GameResources.Font.MeasureString(t.Substring(0, i)).X * .25f;
                        sb.DrawString(GameResources.Font, chr, new Vector2(x + 2 + tx + blub, y + 2 + ty + yoff), col, 0, Vector2.Zero, .25f, SpriteEffects.None, G.D_UI + .0001f);
                    }
                }
            }
        }
    }
}
