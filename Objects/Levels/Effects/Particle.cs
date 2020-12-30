using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wyri.Objects.Levels.Effects
{
    public class Particle
    {
        public ParticleEmitter Emitter { get; private set; }

        public Texture2D Texture { get; set; }
        public float Depth;

        public float Angle;
        public Vector2 Scale;

        public float XVel;
        public float YVel;

        public Vector2 Position;
        public Color Color;
        public float Alpha;
        public Vector2 DrawOffset;

        public float X
        {
            get { return Position.X; }
            set { Position = new Vector2(value, Y); }
        }
        public float Y
        {
            get { return Position.Y; }
            set { Position = new Vector2(X, value); }
        }

        public int LifeTime;
        public int MaxLifeTime;

        public Particle(ParticleEmitter emitter, int maxLifeTime)
        {
            Color = Color.White;

            Scale = Vector2.One;
            Alpha = 1;

            Emitter = emitter;
            Emitter.Add(this);

            Position = Emitter.Position;

            Texture = Primitives2D.Pixel;
            Depth = emitter.Depth;

            MaxLifeTime = maxLifeTime;
            LifeTime = MaxLifeTime;
        }

        ~Particle()
        {
            Emitter.Remove(this);
            Emitter = null;
        }

        public virtual void Update()
        {
            LifeTime = Math.Max(LifeTime - 1, 0);

            if (LifeTime == 0)
                return;

            Position = new Vector2(Position.X + XVel, Position.Y + YVel);
        }

        public virtual void Draw(SpriteBatch sb)
        {
            sb.Draw(Texture, Position, null, new Color(Color, Alpha), Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
        }
    }

    // ++++ Emitter ++++

    public abstract class ParticleEmitter : SpatialObject
    {
        public List<Particle> Particles { get; protected set; } = new List<Particle>();
        public bool Active { get; set; } = true;
        public float Depth { get; set; } = G.D_EFFECT;
        public int SpawnRate { get; set; } = 1;
        public int SpawnTimeout { get; set; } = 0;

        protected int currentSpawnTimeout = 0;

        public ParticleEmitter(Vector2 position) : base(position, new Types.RectF(0, 0, 0, 0))
        {            
        }

        public void Add(Particle particle)
        {
            Particles.Add(particle);
        }

        public void Remove(Particle particle)
        {
            if (Particles.Contains(particle))
                Particles.Remove(particle);
        }

        ~ParticleEmitter()
        {
            Particles.Clear();
        }

        public abstract void CreateParticle();

        public override void Update()
        {
            if (Active)
            {
                currentSpawnTimeout -= 1;
                if (currentSpawnTimeout <= 0)
                {
                    for (var i = 0; i < SpawnRate; i++)
                        CreateParticle();

                    currentSpawnTimeout = SpawnTimeout;
                }
            }

            foreach (var p in Particles.ToList())
            {
                p.Update();

                if (p.LifeTime == 0)
                    Remove(p);
            }
        }

        public override void Draw(SpriteBatch sb)
        {
            // draw only particles which are within the camera bounds

            var t = G.T;

            foreach (var part in Particles)
            {
                if (part.Position.X.In(MainGame.Camera.ViewX - t, MainGame.Camera.ViewX + MainGame.Camera.ViewWidth + t)
                    && part.Position.Y.In(MainGame.Camera.ViewY - t, MainGame.Camera.ViewY + MainGame.Camera.ViewHeight + t))
                {
                    part.Draw(sb);
                }
            }
        }
    }
}
