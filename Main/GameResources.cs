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
        public static Effect UnderWater { get; private set; }

        public static SpriteFont Font { get; private set; }
        public static Texture2D MessageBox { get; private set; }

        public static TextureSet Tiles { get; private set; }
        public static TextureSet Player { get; private set; }
        public static TextureSet Background { get; private set; }
        public static TextureSet Spinner { get; private set; }
        public static TextureSet Save { get; private set; }
        public static TextureSet Effects { get; private set; }
        public static TextureSet Smoke { get; private set; }
        public static TextureSet Oxygen { get; private set; }
        public static TextureSet Enemy1 { get; private set; }
        public static TextureSet Enemy2 { get; private set; }
        public static TextureSet Crosshair { get; private set; }
        public static TextureSet Projectiles { get; private set; }
        public static Texture2D Map { get; private set; }
        public static TextureSet Items { get; private set; }

        public static void Init(ContentManager content)
        {
            Font = content.Load<SpriteFont>("font");
            MessageBox = content.Load<Texture2D>("messagebox");
            Player = content.LoadTextureSet("player", 16, 16);
            Tiles = content.LoadTextureSet("tiles", 8, 8);
            Background = content.LoadTextureSet("background", 256, 216);
            Spinner = content.LoadTextureSet("spinner", 16, 16);
            Save = content.LoadTextureSet("save", 16, 16);
            Effects = content.LoadTextureSet("effects", 16, 16);
            Smoke = content.LoadTextureSet("smoke", 8, 8);
            Oxygen = content.LoadTextureSet("oxygen", 16, 16);
            Enemy1 = content.LoadTextureSet("enemy1", 16, 16);
            Enemy2 = content.LoadTextureSet("enemy2", 16, 16);
            Crosshair = content.LoadTextureSet("crosshair", 8, 8);
            Projectiles = content.LoadTextureSet("projectiles", 8, 8);
            Map = content.Load<Texture2D>("map");
            Items = content.LoadTextureSet("items", 16, 16);

            //UnderWater = content.Load<Effect>("testshader");
            //UnderWater.Parameters["fAmplitude"].SetValue(0.01f);
            //UnderWater.Parameters["fFrequency"].SetValue(1.0f);
            //UnderWater.Parameters["fPeriods"].SetValue(0.5f);
            //UnderWater.Parameters["fDistortStr"].SetValue(0.01f);
            //UnderWater.Parameters["fWaveFrequency"].SetValue(1f);
            //UnderWater.Parameters["fWaveAmplitude"].SetValue(0.01f);
            //UnderWater.Parameters["fWavePeriods"].SetValue(25f);
            //UnderWater.Parameters["fPixelWidth"].SetValue(1f);
            //UnderWater.Parameters["fPixelHeight"].SetValue(1f);

            //UnderWater.CurrentTechnique = UnderWater.Techniques["tech_main"];
            //float fAmplitude, fFrequency, fPeriods, fDistortStr, fWaveFrequency, fWaveAmplitude, fWavePeriods, fPixelWidth, fPixelHeight;

        }
    }
}
