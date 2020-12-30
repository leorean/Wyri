using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Main;
using Wyri.Types;
using Wyri.Util;

namespace Wyri.Objects.Levels.Effects
{
    //public class SmokeParticle : Particle
    //{
    //    float alpha = 0;
    //    bool fadeIn = false;

    //    public SmokeParticle(ParticleEmitter emitter) : base(emitter, 60)
    //    {            
    //        Alpha = alpha;

    //        Scale = new Vector2(1.5f + RND.Next * 1f);

    //        Position = emitter.Position + new Vector2(-5 + RND.Int(10), 0);
    //        Depth = G.D_BG1 - .01f;

    //        XVel = -.2f + RND.Next * .4f;
    //        YVel = -.4f;

    //        var val = 200 + RND.Int(55);
    //        Color = new Color(val, val, val);
    //    }

    //    public override void Update()
    //    {
    //        base.Update();

    //        var s = Scale.X + .03f;

    //        if (!fadeIn)
    //        {
    //            alpha = Math.Min(alpha + .03f, .5f);
    //            if (alpha == .5f)
    //                fadeIn = true;
    //        }

    //        Scale = new Vector2(s);
    //        Alpha = (LifeTime / (float)MaxLifeTime) * alpha;
    //    }
    //}

    public class SmokeParticle : Particle
    {
        int frame = 0;
        float alpha = 0;
        bool fadeIn = false;

        public SmokeParticle(ParticleEmitter emitter) : base(emitter, 45)
        {
            //Scale = new Vector2(1.5f + RND.Next * 1f);

            Position = emitter.Position + new Vector2(-2 + RND.Int(4), 0);
            Depth = G.D_BG1 - .01f;

            XVel = -.2f + RND.Next * .4f;
            YVel = -.4f;

            DrawOffset = new Vector2(4);

            Color = RND.Choose(Color.White, Color.Black, Color.Gray);
        }

        public override void Update()
        {
            base.Update();

            frame = (int)Math.Max(0, Math.Floor((1 - ((LifeTime - 5) / (float)MaxLifeTime)) * 7));
            Texture = GameResources.Smoke[frame];

            if (!fadeIn)
            {
                alpha = Math.Min(alpha + .03f, .5f);
                if (alpha == .5f)
                    fadeIn = true;
            }

            //var s = Scale.X + .03f;
            //Scale = new Vector2(s);
            //Alpha = (LifeTime / (float)MaxLifeTime) * alpha;
            Alpha = 1f;
        }
    }

    public class SmokeEmitter : ParticleEmitter
    {
        public SmokeEmitter(Vector2 position) : base(position)
        {
            SpawnRate = 1;
            SpawnTimeout = 10;
        }

        public override void CreateParticle()
        {
            new SmokeParticle(this);
        }
    }

    public class Smoke : RoomObject
    {
        private SmokeEmitter emitter;
        public Smoke(Vector2 position, Room room) : base(position, new RectF(), room)
        {
            emitter = new SmokeEmitter(position);
        }

        public override void Destroy()
        {
            emitter.Destroy();
            base.Destroy();
        }

        public override void Draw(SpriteBatch sb)
        {            
        }

        public override void Update()
        {            
        }
    }
}
