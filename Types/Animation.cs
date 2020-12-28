using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wyri.Types
{
    public class Animation
    {
        public TextureSet Texture { get; set; }
        public int MinFrame { get; set; }
        public int FrameCount { get; set; }
        public int Frame
        {
            get { return (int)Math.Floor(frame); }
            set { frame = value; }
        }
        public float AnimationSpeed { get; set; }
        public bool IsLooping { get; set; }
        public bool IsDone { get; private set; }

        private float frame;

        public Animation(TextureSet texture, int minFrame, int frameCount, float animationSpeed, bool isLooping = true)
        {
            Texture = texture;
            MinFrame = minFrame;
            FrameCount = frameCount;
            AnimationSpeed = animationSpeed;
            IsLooping = isLooping;
        }

        public void Reset()
        {
            frame = 0;
            IsDone = false;
        }

        public void Update()
        {
            frame = (frame + AnimationSpeed);

            if (IsLooping)
            {
                if (frame >= FrameCount)
                    frame = 0;
            }
            else
            {                
                frame = Math.Min(frame, FrameCount - 1);
                if (frame == FrameCount - 1)
                    IsDone = true;
            }
        }

        public void Draw(SpriteBatch sb, Vector2 position, Vector2 offset, Vector2 scale, Color color, float angle, float depth)
        {
            sb.Draw(Texture[MinFrame + Frame], position, null, color, angle, offset, scale, SpriteEffects.None, depth);
        }
    }
}
