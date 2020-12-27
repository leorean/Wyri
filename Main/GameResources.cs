using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Types;

namespace Wyri.Main
{
    public static class GameResources
    {
        public static TextureSet Tiles { get; private set; }
        public static TextureSet Player { get; private set; }
        public static TextureSet Background { get; private set; }

        public static void Init(ContentManager content)
        {
            Player = content.LoadTextureSet("player", 16, 16);
            Tiles = content.LoadTextureSet("tiles", 8, 8);
            Background = content.LoadTextureSet("background", 256, 144);
        }
    }
}
