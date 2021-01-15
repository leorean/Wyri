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
        float yGrav = .2f;

        public TextureBurstParticle(ParticleEmitter emitter, Color color) : base(emitter, 40)
        {
            Color = color;
            XVel = -1 + RND.Next * 2;
            YVel = -1 + RND.Next * 1;
        }

        public override void Update()
        {
            base.Update();
            YVel += yGrav;

            XVel *= .95f;
            YVel *= .95f;

            var t = Collisions.TileAt(X, Y, "FG");
            if (t != null && t.IsSolid && LifeTime < 30)
            {
                XVel *= -.5f;
                YVel = 0;
                //LifeTime = Math.Max(LifeTime - 1 , 0);
            }

            Alpha = LifeTime / (float)MaxLifeTime;
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);            
        }
    }

    public class TextureBurstEmitter : ParticleEmitter, IDestroyOnRoomChange
    {
        private Color[] colors;
        public TextureBurstEmitter(Texture2D texture, Vector2 position, Room room) : base(position, room)
        {            
            SpawnTimeout = 0;
            colors = texture.GetPixels();
            SpawnRate = colors.Length;            
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
            new TextureBurstParticle(this, colors[Particles.Count]);
        }
    }
}
