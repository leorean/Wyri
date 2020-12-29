using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Main;
using Wyri.Types;

namespace Wyri.Objects.Levels
{
    public class SavePoint : RoomObject
    {
        Animation animation;

        bool touched;
        float t;
        float yo;
        int touchedTimer;

        public void SaveHere()
        {
            if (touched)
                return;

            MainGame.SaveGame.Abilities = MainGame.Player.Abilities;
            MainGame.SaveGame.Position = Position + new Vector2(8, 0);

            SaveManager.Save(MainGame.SaveGame);

            t = 0;
            touchedTimer = 3 * 60;
            touched = true;
        }

        public SavePoint(Vector2 position, Room room) : base(position, new RectF(4, 4, 8, 8), room)
        {
            animation = new Animation(GameResources.Save, 0, 6, .2f, true);        
        }
        public override void Update()
        {
            animation.Update();
            animation.AnimationSpeed = touched ? .33f : .17f;

            if (!touched)
            {
                t = (t + .1f) % (2 * (float)Math.PI);
                yo = (float)Math.Sin(t);
            }
            else
            {                
                if (touchedTimer > 0)
                {
                    touchedTimer = Math.Max(touchedTimer - 1, 0);
                    t = Math.Max(t - .1f, -5);
                }
                else
                {
                    t = Math.Min(t + .1f, 0);
                    if (t == 0)
                    {
                        touched = false;
                    }
                }
                yo = t;
            }

        }

        public override void Draw(SpriteBatch sb)
        {            
            animation.Draw(sb, Position + new Vector2(0, yo), Vector2.Zero, Vector2.One, Color.White, 0, G.D_PLAYER - .001f);
        }
        
    }
}
