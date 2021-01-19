using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wyri.Objects.Levels.Effects;
using Wyri.Types;

namespace Wyri.Objects.Levels.Enemies
{
    public abstract class Enemy : Obstacle
    {
        public Enemy(Vector2 position, RectF boundingBox, Room room) : base(position, boundingBox, room) { }

        public bool Dead { get; set; } = false;

        public override void Update()
        {
            base.Update();

            var bullet = this.CollisionBounds<Bullet>().FirstOrDefault();

            if (bullet != null && bullet.Parent != this)
            {
                bullet.Kill();
                Dead = true;
            }

            if (Dead) Kill();
            if (Dead) Destroy();
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
        }

        public abstract void Kill();
    }
}
