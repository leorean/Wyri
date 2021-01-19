using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Types;
using Wyri.Util;

namespace Wyri.Objects.Levels.Effects
{
    public class TextureBurstParticle : Particle
    {
        float yGrav;

        public TextureBurstParticle(ParticleEmitter emitter, Color color) : base(emitter, (emitter as TextureBurstEmitter).LifeTime)
        {
            var power = (emitter as TextureBurstEmitter).Power;
            yGrav = (emitter as TextureBurstEmitter).YGrav;
            Color = color;
            XVel = -power.X + RND.Next * 2 * power.X;
            YVel = -power.Y - RND.Next * power.Y;
        }

        public override void Update()
        {
            YVel += yGrav;

            XVel *= .95f;
            YVel *= .95f;

            var t = Collisions.TileAt(X, Y, "FG");
            if (t != null && t.IsSolid && LifeTime < MaxLifeTime - 10)
            {
                XVel *= -.5f;
                YVel = 0;
                //LifeTime = Math.Max(LifeTime - 1 , 0);
            }

            Alpha = LifeTime / (float)MaxLifeTime;

            base.Update();
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);            
        }
    }

    public class TextureBurstEmitter : ParticleEmitter, IDestroyOnRoomChange
    {
        private Grid<Color> colors;
        
        public Vector2 Power { get; private set; }
        public float YGrav { get; private set; }
        public int LifeTime { get; private set; }

        public TextureBurstEmitter(Texture2D texture, Vector2 position, Vector2 power, float yGrav, int lifeTime) : base(position, MainGame.Camera.Room)
        {
            Power = power;
            YGrav = yGrav;
            LifeTime = lifeTime;
            SpawnTimeout = 0;
            colors = new Grid<Color>(texture.Width, texture.Height);

            var pixels = texture.GetPixels();
            for (var i = 0; i < pixels.Length; i++)
            {
                colors[i] = pixels[i];
            }

            SpawnRate = 1;
        }

        public override void Update()
        {
            base.Update();
            if (Particles.Count > 0)
            {
                SpawnRate = 0;
            }
            else
            {
                Destroy();
            }
        }

        public override void CreateParticle()
        {
            for(var i = 0; i < colors.Width; i++)
            {
                for (var j = 0; j < colors.Height; j++)
                {
                    var part = new TextureBurstParticle(this, colors[i, j]);
                    part.Position = Position - new Vector2(colors.Width * .5f, colors.Height * .5f) + new Vector2(i, j);
                }
            }
        }
    }
}
