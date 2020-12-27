using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Reflection;
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

        public static Map map;
        public static Camera Camera;
        public static Player Player;

        public Size ViewSize { get; private set; }
        private float scale;
        private Size screenSize;

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

        }

        protected override void Initialize()
        {
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

            map = new Map($"map.tmx");

            // TODO: use this.Content to load your game content here

            Camera.Position = new Vector2(ViewSize.Width * .5f, ViewSize.Height * .5f);
            Camera.Room = map.Rooms[0];
        }

        int debugRoomIndex;

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            InputController.Update();

            if (InputController.IsKeyPressed(Keys.Right)) { Player.Position = new Vector2(Player.Position.X + 2, Player.Position.Y); }
            if (InputController.IsKeyPressed(Keys.Left)) { Player.Position = new Vector2(Player.Position.X - 2, Player.Position.Y); }
            if (InputController.IsKeyPressed(Keys.Down)) { Player.Position = new Vector2(Player.Position.X, Player.Position.Y + 2); }
            if (InputController.IsKeyPressed(Keys.Up)) { Player.Position = new Vector2(Player.Position.X, Player.Position.Y - 2); }

            if (InputController.IsKeyPressed(Keys.A, KeyState.Pressed)) { Camera.Room.SwitchState = !Camera.Room.SwitchState; }
            
            map.Update();
            Camera.Update();

            ObjectController.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // ++++ prepare resolution ++++

            InvokeSizeChange(new Size(Window.ClientBounds.Width, Window.ClientBounds.Height));

            // ++++ begin draw ++++

            GraphicsDevice.Clear(Color.Gray);

            Camera.ResolutionRenderer.SetupDraw();

            // actual object drawing etc.

            SpriteBatch.BeginCamera(Camera, BlendState.NonPremultiplied, DepthStencilState.None);

            map.Draw(SpriteBatch);
            ObjectController.Draw(SpriteBatch);

            SpriteBatch.End();

            //base.Draw(gameTime);
        }
    }
}
