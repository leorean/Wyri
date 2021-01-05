using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Main;
using Wyri.Util;

namespace Wyri.Objects.Levels.Effects
{
    public class ElectricSparkParticle : Particle
    {
        readonly float yGrav = .06f;

        bool bounced = false;

        public ElectricSparkParticle(ParticleEmitter emitter) : base(emitter, 120)
        {
            Texture = Primitives2D.Pixel;            
            XVel = -.5f + RND.Next * 1;
            YVel = -0.5f + RND.Next * .5f;
            //Color = new Color(134, 234, 255);
        }

        public override void Update()
        {
            base.Update();

            //Alpha = LifeTime / (float)MaxLifeTime;

            YVel += yGrav;

            X += XVel;
            Y += YVel;

            var t = Collisions.TileAt(X, Y + YVel, "FG");
            if (t != null && t.IsSolid)
            {
                if (!bounced)
                {
                    XVel *= .7f;
                    Y -= YVel;
                    YVel *= -.5f;
                    bounced = true;
                }
                else
                {
                    LifeTime = 0;
                }
            }
        }
    }

    public class ElectricSparkEmitter : ParticleEmitter
    {
        public ElectricSparkEmitter(Vector2 position, Room room) : base(position, room)
        {
            SpawnTimeout = 0;
            SpawnRate = 0;

            ResetTimer();
        }

        void ResetTimer()
        {
            SpawnRate = 1 + RND.Int(2);
            SpawnTimeout = 60 + RND.Int(30);
            Active = true;
        }

        public override void Update()
        {
            base.Update();

            if (!Active && Particles.Count == 0)
            {
                ResetTimer();
            }
        }

        public override void CreateParticle()
        {
            new ElectricSparkParticle(this);
            Active = false;
        }
    }
}
