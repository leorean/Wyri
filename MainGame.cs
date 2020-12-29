using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Wyri.Main;
using Wyri.Objects;
using Wyri.Objects.Levels;
using Wyri.Types;

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
                await Map.LoadMapContentAsync("map.tmx");

                isLoading = false;
            });

            GC.Collect();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            if (!isLoading)
            {
                InputController.Update();

                Camera.Update();

                ObjectController.SetAllActive<RoomObject>(false);
                ObjectController.SetRegionActive<SpatialObject>(Camera.Room.X, Camera.Room.Y, Camera.Room.Width, Camera.Room.Height, true);

                ObjectController.Update();

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

                if (InputController.IsKeyPressed(Keys.D0, KeyState.Pressed)) { Camera.Room.SwitchState = !Camera.Room.SwitchState; }

                if (InputController.IsMousePressed(KeyState.Holding))
                {
                    Player.Position = Camera.ToVirtual(Mouse.GetState().Position.ToVector2());
                }        
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // ++++ prepare resolution ++++

            InvokeSizeChange(new Size(Window.ClientBounds.Width, Window.ClientBounds.Height));

            // ++++ begin draw ++++

            GraphicsDevice.Clear(Color.Black);

            Camera.ResolutionRenderer.SetupDraw();
            
            // actual object drawing etc.

            SpriteBatch.BeginCamera(Camera, BlendState.NonPremultiplied, DepthStencilState.None);

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
                //if (Player == null || Player.State != PlayerState.Dead)
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

            SpriteBatch.End();
        }
    }
}
