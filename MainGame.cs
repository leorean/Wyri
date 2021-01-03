using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Wyri.Main;
using Wyri.Objects;
using Wyri.Objects.Levels;
using Wyri.Types;
using Object = Wyri.Objects.Object;

namespace Wyri
{
    public class MainGame : Game
    {
        public static GraphicsDeviceManager GraphicsDeviceManager { get; private set; }
        public static SpriteBatch SpriteBatch { get; private set; }

        public static Map Map;
        public static Camera Camera;
        public static Player Player;
        public static SaveGame SaveGame = new SaveGame();        

        public Size ViewSize { get; private set; }
        private float scale;
        private Size screenSize;

        public Animation LoadingAnimation { get; private set; }
        private bool isLoading;
        private static bool issueReloading;
        private float fadeInAlpha;

        public static int Ticks { get; private set; }

        public MainGame()
        {
            GraphicsDeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            ViewSize = new Size(256, 144);
            scale = 4.0f;
            screenSize = new Size((int)(ViewSize.Width * scale), (int)(ViewSize.Height * scale));

            IsMouseVisible = true;
            Window.AllowUserResizing = true;

            Map = new Map();            
        }

        protected override void Initialize()
        {
            Primitives2D.Setup(GraphicsDevice);

            var resolutionRenderer = new ResolutionRenderer(GraphicsDevice, ViewSize.Width, ViewSize.Height, screenSize.Width, screenSize.Height);
            Camera = new Camera(resolutionRenderer) { MaxZoom = 2f, MinZoom = .5f, Zoom = 1f };
            Camera.Position = new Vector2(ViewSize.Width * .5f, ViewSize.Height * .5f);
            
            InvokeSizeChange(screenSize);

            base.Initialize();
        }

        private void InvokeSizeChange(Size windowSize)
        {            
            var w = windowSize.Width;
            var h = windowSize.Height;

            GraphicsDeviceManager.PreferredBackBufferWidth = w;
            GraphicsDeviceManager.PreferredBackBufferHeight = h;

            Camera.ResolutionRenderer.ScreenWidth = w;
            Camera.ResolutionRenderer.ScreenHeight = h;

            GraphicsDeviceManager.ApplyChanges();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            GameResources.Init(Content);

            LoadingAnimation = new Animation(GameResources.Spinner, 0, 7, .3f);

            Reload();
        }

        public static void ReloadLevel()
        {
            issueReloading = true;
        }

        public void Reload()
        {
            isLoading = true;
            fadeInAlpha = 1;

            Player?.Destroy();
            Player = null;

            Task.Run(async () => {
                await Map?.UnloadAsync();

                foreach(var o in ObjectController.FindAll<Object>())
                {
                    if (o is Player)
                        continue;
                    o.Destroy();
                }

                await Map.LoadMapContentAsync("map.tmx");

                isLoading = false;
            });

            GC.Collect();
        }

        public static void Save(Vector2 position)
        {
            SaveGame.Abilities = Player.Abilities;
            SaveGame.Position = position;
            SaveGame.Direction = Player.Direction;
            SaveGame.Background = Camera.Background;
            SaveGame.Weather = Camera.Weather;
            SaveGame.Darkness = Camera.Darkness;

            SaveManager.Save(SaveGame);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            if (!isLoading)
            {
                InputController.Update();

                ObjectController.SetAllActive<Object>(false);
                ObjectController.SetAllActive<Player>(true);
                ObjectController.SetAllActive<Room>(true);
                ObjectController.SetRegionActive<SpatialObject>(Camera.Room.X, Camera.Room.Y, Camera.Room.Width, Camera.Room.Height, true);

                ObjectController.Update();

                Camera.Update();

                if (issueReloading)
                {
                    if (fadeInAlpha == 1)
                    {
                        issueReloading = false;
                        Reload();
                    }
                }

                if (InputController.IsKeyPressed(Keys.R, KeyState.Pressed))
                {
                    Reload();
                }

                if (InputController.IsKeyPressed(Keys.C, KeyState.Pressed))
                {
                    SaveManager.DeleteSaveGame();
                }

                if (InputController.IsKeyPressed(Keys.D0, KeyState.Pressed))
                {
                    Save(Player.Position);
                }
                

                //if (InputController.IsKeyPressed(Keys.D0, KeyState.Pressed)) { Camera.Room.SwitchState = !Camera.Room.SwitchState; }

                if (InputController.IsMousePressed(KeyState.Holding))
                {
                    if (Player != null)
                        Player.Position = Camera.ToVirtual(Mouse.GetState().Position.ToVector2());
                }        
            }

            Ticks = (Ticks + 1) % 9000;

            base.Update(gameTime);
        }

        public static RenderTarget2D LastBuffer { get; private set; }

        protected override void Draw(GameTime gameTime)
        {
            // ++++ prepare resolution ++++

            InvokeSizeChange(new Size(Window.ClientBounds.Width, Window.ClientBounds.Height));

            // ++++ begin draw ++++

            //LastBuffer = new RenderTarget2D(GraphicsDevice, 256, 144);
            //GraphicsDevice.SetRenderTarget(LastBuffer);

            GraphicsDevice.Clear(Color.Black);
            Camera.ResolutionRenderer.SetupDraw();

            SpriteBatch.BeginCamera(Camera, BlendState.NonPremultiplied, DepthStencilState.None);
            
            // actual object drawing etc.

            if (!isLoading)
            {
                
                Map.Draw(SpriteBatch);
                ObjectController.Draw(SpriteBatch);

                Camera.Draw(SpriteBatch);
            }

            if (isLoading)
            {
                LoadingAnimation.Update();
                LoadingAnimation.Draw(SpriteBatch, new Vector2(Camera.ViewX + 12, Camera.ViewY + Camera.ViewHeight - 12), new Vector2(8), Vector2.One, Color.White, 0, G.D_FADE);
                fadeInAlpha = 1.5f;
            }
            else
            {
                if (!issueReloading)
                {
                    fadeInAlpha = Math.Max(fadeInAlpha - .025f, 0);
                }
                else
                {
                    fadeInAlpha = Math.Min(fadeInAlpha + .025f, 1);
                }
                if (fadeInAlpha > 0)
                {
                    Primitives2D.DrawRectangle(SpriteBatch, new RectF(Camera.ViewX, Camera.ViewY, Camera.ViewWidth, Camera.ViewHeight), new Color(Color.Black, fadeInAlpha), true, G.D_FADE);
                }
            }

            // show map
            if (InputController.IsKeyPressed(Keys.W, KeyState.Holding))
            {
                if (Player != null && Player.Abilities.HasFlag(PlayerAbility.MAP))
                    MapDisplay.Draw(SpriteBatch);
            }

            SpriteBatch.End();

            //int w = GraphicsDevice.PresentationParameters.BackBufferWidth;
            //int h = GraphicsDevice.PresentationParameters.BackBufferHeight;
            //int[] backBuffer = new int[w * h];
            //GraphicsDevice.GetBackBufferData(backBuffer);
            //LastBuffer = new RenderTarget2D(GraphicsDevice, w, h);
            //LastBuffer.SetData(backBuffer);

            //GraphicsDevice.SetRenderTarget(null);

            //int w = GraphicsDevice.PresentationParameters.BackBufferWidth;
            //int h = GraphicsDevice.PresentationParameters.BackBufferHeight;
            //var sb = new SpriteBatch(GraphicsDevice);
            //sb.Begin();
            //sb.Draw(LastBuffer, new Rectangle(0, 0, w, h));
            //sb.End();

            /*
              How to water-shader:
              1) Draw background on background batch
              2) Draw every tile that is beneath water layer on waterBatch
              3) Draw everything that is above water layer on foreground batch
             */

            //var shaderBatch = new SpriteBatch(GraphicsDevice);
            ////mBatch.Begin(sortMode, bstate, SamplerState.PointClamp, dstate, RasterizerState.CullNone, null, camera.GetViewTransformationMatrix());
            ////shaderBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, GameResources.UnderWater, Camera.GetViewTransformationMatrix());
            //shaderBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, GameResources.UnderWater, Camera.GetViewTransformationMatrix());
            //GameResources.UnderWater.CurrentTechnique.Passes[0].Apply();
            //Map.DrawWater(shaderBatch);
            ////shaderBatch.Draw(GameResources.Tiles.OriginalTexture, new Vector2(i * G.T, j * G.T), Color.White);
            //shaderBatch.End();
        }
    }
}
