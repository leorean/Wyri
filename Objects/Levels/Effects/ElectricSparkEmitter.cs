using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Util;

namespace Wyri.Objects.Levels.Effects
{
    public class ElectricSparkParticle : Particle
    {
        float yGrav = .1f;

        public ElectricSparkParticle(ParticleEmitter emitter) : base(emitter)
        {
            Texture = Primitives2D.Pixel;
            LifeTime = 60;
            XVel = -1 + RND.Next * 2;
            YVel = -2 + RND.Next * 2;
            Color = Color.Red;
        }

        public override void Update()
        {
            base.Update();

            YVel += yGrav;

            X += XVel;
            Y += YVel;
        }
    }

    public class ElectricSparkEmitter : ParticleEmitter
    {
        public ElectricSparkEmitter(Vector2 position) : base(position)
        {
            SpawnTimeout = 5;
            SpawnRate = 3;
        }

        public override void Update()
        {
            base.Update();

            if (Particles.Count == 0)
            {

            }
        }

        public override void CreateParticle()
        {
            new ElectricSparkParticle(this);
        }
    }
}
