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
        public static List<Room> RoomsVisited;

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
            RoomsVisited = new List<Room>();
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

            if (InputController.IsKeyPressed(Keys.W, KeyState.Holding))
            {
                var xo = Camera.ViewX + 8;
                var yo = Camera.ViewY + 8;

                Color bgFill = new Color(22, 22, 29);
                Color bgGrid = new Color(24, 24, 31);
                Color unvisitedFill = new Color(105, 106, 106);
                Color unvisitedGrid = new Color(131, 140, 145);
                Color visitedFill = new Color(121, 215, 255);
                Color visitedGrid = new Color(255, 255, 255);

                List<Room> drawn = new List<Room>();

                var rmW = (int)((double)Map.Width / (double)Camera.ViewWidth * (double)G.T);
                var rmH = (int)((double)Map.Height / (double)Camera.ViewHeight * (double)G.T);

                var sizeX = 5;
                var sizeY = 3;
                var depth = .9f;

                for (var i = 0; i < rmW; i++)
                {
                    for (var j = 0; j < rmH; j++)
                    {                        
                        var bgCol = bgFill;
                        var fgCol = bgGrid;
                        //SpriteBatch.DrawRectangle(new RectF(xo + i * sizeX, yo + j * sizeY, sizeX, sizeY), bgCol, true, depth - .0004f);
                        //SpriteBatch.DrawRectangle(new RectF(xo + i * sizeX, yo + j * sizeY, sizeX, sizeY), fgCol, false, depth - .0003f);
                        SpriteBatch.Draw(GameResources.Map, new Vector2(xo + i * (sizeX), yo + j * (sizeY)), null, new Color(Color.White, .85f), 0, Vector2.Zero, Vector2.One, SpriteEffects.None, depth - .0003f);

                        var r = Collisions.CollisionPoint<Room>(i * Camera.ViewWidth + G.T, j * Camera.ViewHeight + G.T).FirstOrDefault();
                        if (r == null || drawn.Contains(r))
                            continue;

                        drawn.Add(r);

                        var w = sizeX * r.Width / (float)Camera.ViewWidth;
                        var h = sizeY * r.Height / (float)Camera.ViewHeight;

                        var d = depth - .0001f;
                        if (RoomsVisited.Contains(r))
                        {
                            bgCol = visitedFill;
                            fgCol = visitedGrid;
                            d = depth;
                        }
                        else
                        {
                            bgCol = unvisitedFill;
                            fgCol = unvisitedGrid;
                        }

                        SpriteBatch.DrawRectangle(new RectF(xo + i * sizeX, yo + j * sizeY, w, h), bgCol, true, d - .0001f);
                        SpriteBatch.DrawRectangle(new RectF(xo + i * sizeX, yo + j * sizeY, w, h), fgCol, false, d);
                    }
                }
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
