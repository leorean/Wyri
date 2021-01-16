using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Util;

namespace Wyri.Objects.Levels.Effects
{
    public class WaterSplashParticle : Particle
    {
        private bool visible;

        public WaterSplashParticle(ParticleEmitter emitter) : base(emitter, 120)
        {
            Scale = new Vector2(1.5f);
            Position = new Vector2(emitter.X - 6 + (float)(RND.Next * 12), emitter.Y);

            Alpha = 1;
            Angle = (float)((RND.Next * 360) / (2 * Math.PI));

            XVel = -1 + (RND.Next * 2f);
            YVel = -1.5f - (RND.Next * 1f);
        }
        
        public override void Update()
        {
            base.Update();

            DrawOffset = new Vector2(.5f * Texture.Width, .5f * Texture.Height);

            // destroy:

            var onTile = Collisions.TileAt(X, Y, "FG") != null;
            var inWater = Collisions.TileAt(X, Y, "WATER") != null;
            if (onTile)
            {
                LifeTime = 0;
            }

            if (inWater)
            {
                YVel *= .7f;
                XVel *= .9f;

                //YVel -= .3f;
                Alpha = Math.Max(Alpha - .04f, 0);
            }
            else
            {
                YVel = Math.Min(YVel + .15f, 1.5f);
            }

            if (Alpha == 0)
                LifeTime = 0;

            visible = true;
        }

        public override void Draw(SpriteBatch sb)
        {
            if (visible)
                base.Draw(sb);
        }
    }

    public class WaterSplashEmitter : ParticleEmitter
    {
        public List<Color> ParticleColors { get; set; } = new List<Color>
        {
            new Color(255, 255, 255),
            new Color(206, 255, 255),
            new Color(168, 248, 248),
            new Color(104, 216, 248)
        };

        public WaterSplashEmitter(Vector2 position, Room room) : base(position, room)
        {
            SpawnRate = 15;
        }

        public override void Update()
        {
            base.Update();

            SpawnRate = 0;

            if (Particles.Count == 0)
                Destroy();
        }

        public override void CreateParticle()
        {
            var colorIndex = RND.Int(ParticleColors.Count - 1);

            var particle = new WaterSplashParticle(this);
            particle.Color = ParticleColors[colorIndex];
        }
    }
}
