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
        public int MaxFrame { get; set; }
        public int Frame
        {
            get { return (int)Math.Floor(frame); }
            set { frame = value; }
        }
        public float AnimationSpeed { get; set; }
        public bool IsLooping { get; set; }
        public bool IsDone { get; private set; }

        private float frame;

        public Animation(TextureSet texture, int minFrame, int maxFrame, float animationSpeed, bool isLooping = true)
        {
            Texture = texture;
            MinFrame = minFrame;
            MaxFrame = maxFrame;
            AnimationSpeed = animationSpeed;
            IsLooping = isLooping;
        }

        public void Reset()
        {
            frame = MinFrame;
            IsDone = false;
        }

        public void Update()
        {
            frame = (frame + AnimationSpeed);
            if (frame >= MaxFrame)
            {
                if (!IsLooping)
                {
                    frame = MaxFrame;
                    IsDone = true;
                }
                else
                {
                    frame = MinFrame;
                }
            }
        }

        public void Draw(SpriteBatch sb, Vector2 position, Vector2 offset, Vector2 scale, Color color, float angle, float depth)
        {
            sb.Draw(Texture[Frame], position, null, color, angle, offset, scale, SpriteEffects.None, depth);
        }
    }
}
