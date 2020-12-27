using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wyri.Main;
using Wyri.Objects.Levels;
using Wyri.Types;

namespace Wyri.Objects
{
    public class Player : SpatialObject
    {

        Animation idleAnimation;

        Animation currentAnimation;

        public Player(Vector2 position) : base(position, new Rectangle())
        {
            idleAnimation = new Animation(GameResources.Player, 0, 4, .1f);
            currentAnimation = idleAnimation;
        }

        public override void Update()
        {
            currentAnimation.Update();

            var room = CollisionExtensions.CollisionPoint<Room>(X + 1, Y).FirstOrDefault();
            if (room != null)
            {
                if (room != MainGame.Camera.Room)
                {
                    MainGame.Camera.Room = room;
                }
            }
        }

        public override void Draw(SpriteBatch sb)
        {
            currentAnimation.Draw(sb, Position, Offset, Vector2.One, Color.White, 0, G.D_PLAYER);
        }
    }
}
