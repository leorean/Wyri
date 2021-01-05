using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Objects;
using Wyri.Objects.Levels;
using Wyri.Types;

namespace Wyri.Main
{
    public static class CameraExtensions
    {
        public static void BeginCamera(this SpriteBatch mBatch, Camera camera)
        {
            mBatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, camera.GetViewTransformationMatrix());
        }

        public static void BeginCamera(this SpriteBatch mBatch, Camera camera, BlendState bstate, DepthStencilState dstate, SpriteSortMode sortMode = SpriteSortMode.FrontToBack)
        {
            mBatch.Begin(sortMode, bstate, SamplerState.PointClamp, dstate, RasterizerState.CullNone, null, camera.GetViewTransformationMatrix());
        }

        public static Rectangle NewInflate(this Rectangle rect, int width, int height)
        {
            var r = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
            r.Inflate(width, height);
            return r;
        }

        public static void DrawString(this SpriteBatch mBatch, SpriteFont font, string text, Vector2 pos, Color color, float scale = 1f)
        {
            mBatch.DrawString(font, text, pos, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        public static void Draw(this SpriteBatch mBatch, Texture2D tex, Rectangle rec)
        {
            mBatch.Draw(tex, rec, Color.White);
        }

        public static void Draw(this SpriteBatch mBatch, Texture2D tex, Vector2 pos)
        {
            mBatch.Draw(tex, pos, Color.White);
        }

        public static void BeginResolution(this SpriteBatch mBatch, ResolutionRenderer renderer)
        {
            mBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, renderer.GetTransformationMatrix());
        }

        public static void BeginResolution(this SpriteBatch mBatch, ResolutionRenderer renderer, BlendState bstate)
        {
            mBatch.Begin(SpriteSortMode.Deferred, bstate, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, renderer.GetTransformationMatrix());
        }
    }

    public class Camera : IDisposable
    {
        #region Variables

        public ResolutionRenderer ResolutionRenderer { get; protected set; }

        public int ViewWidth => ResolutionRenderer.ViewWidth;
        public int ViewHeight => ResolutionRenderer.ViewHeight;

        public float ViewX => Position.X - .5f * ViewWidth;        
        public float ViewY => Position.Y - .5f * ViewHeight;

        public SpatialObject Target { get; set; }

        protected Rectangle bounds;

        private float _zoom;
        private float _rotation;
        private Vector2 _position;
        private Matrix _transform = Matrix.Identity;
        private bool _isViewTransformationDirty = true;
        private Matrix _camTranslationMatrix = Matrix.Identity;
        private Matrix _camRotationMatrix = Matrix.Identity;
        private Matrix _camScaleMatrix = Matrix.Identity;
        private Matrix _resTranslationMatrix = Matrix.Identity;

        private Vector3 _camTranslationVector = Vector3.Zero;
        private Vector3 _camScaleVector = Vector3.Zero;
        private Vector3 _resTranslationVector = Vector3.Zero;

        private bool _boundsEnabled = false;

        /// <summary>
        /// Enables bounds in which the camera is able to move.
        /// </summary>
        /// <param name="rect"></param>
        public void EnableBounds(Rectangle rect)
        {
            bounds = rect;
            _boundsEnabled = true;
        }

        /// <summary>
        /// Disables bounds.
        /// </summary>
        public void DisableBounds()
        {
            _boundsEnabled = false;
        }

        /// <summary>
        /// Current camera position
        /// </summary>
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                if (_boundsEnabled)
                {

                    var minX = Math.Max(value.X, bounds.X + ResolutionRenderer.ViewWidth * .5f / Zoom);
                    var x = Math.Min(minX, bounds.X + bounds.Width - ResolutionRenderer.ViewWidth * .5f / Zoom);

                    var minY = Math.Max(value.Y, bounds.Y + ResolutionRenderer.ViewHeight * .5f / Zoom);
                    var y = Math.Min(minY, bounds.Y + bounds.Height - ResolutionRenderer.ViewHeight * .5f / Zoom);

                    _position = new Vector2(x, y);
                }
                else
                {
                    _position = value;
                }
                _isViewTransformationDirty = true;
            }
        }

        /// <summary>
        /// Minimum zoom value (can be no less than 0.1f)
        /// </summary>
        public float MinZoom { get; set; }
        /// <summary>
        /// Maximum zoom value
        /// </summary>
        public float MaxZoom { get; set; }

        /// <summary>
        /// Gets or sets camera zoom value
        /// </summary>
        public float Zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = value;
                if (_zoom < 0.1f)
                    _zoom = 0.1f;
                if (_zoom < MinZoom) _zoom = MinZoom;
                if (_zoom > MaxZoom) _zoom = MaxZoom;
                _isViewTransformationDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets camera rotation value
        /// </summary>
        public float Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                _rotation = value;
                _isViewTransformationDirty = true;
            }
        }
        #endregion

        public Room Room { get; set; }
        
        public int Background { get; set; } = 0;
        public int Weather { get; set; } = 0;
        public float Darkness { get; set; } = 0;

        float flashAlpha = 0;

        public Camera(ResolutionRenderer resolutionRenderer)
        {
            ResolutionRenderer = resolutionRenderer;
            _zoom = 0.1f;
            _rotation = 0.0f;
            _position = Vector2.Zero;
            MinZoom = 0.1f;
            MaxZoom = 999f;
        }

        /// <summary>
        /// Center camera and fit the area of specified rectangle
        /// </summary>
        /// <param name="rec">Rectange</param>
        public void CenterOnTarget(Rectangle rec)
        {
            Position = new Vector2(rec.Center.X, rec.Center.Y);
            var fat1 = ResolutionRenderer.ViewWidth / (float)ResolutionRenderer.ViewHeight;
            var fat2 = rec.Width / (float)rec.Height;
            float ratio;
            if (fat2 >= fat1) ratio = ResolutionRenderer.ViewWidth / (float)rec.Width;
            else ratio = ResolutionRenderer.ViewHeight / (float)rec.Height;
            Zoom = ratio;
        }

        /// <summary>
        /// Get camera transformation matrix
        /// </summary>
        public Matrix GetViewTransformationMatrix()
        {
            if (_isViewTransformationDirty)
            {
                _camTranslationVector.X = -_position.X;
                _camTranslationVector.Y = -_position.Y;

                Matrix.CreateTranslation(ref _camTranslationVector, out _camTranslationMatrix);
                Matrix.CreateRotationZ(_rotation, out _camRotationMatrix);

                _camScaleVector.X = _zoom;
                _camScaleVector.Y = _zoom;
                _camScaleVector.Z = 1;

                Matrix.CreateScale(ref _camScaleVector, out _camScaleMatrix);

                _resTranslationVector.X = ResolutionRenderer.ViewWidth * 0.5f;
                _resTranslationVector.Y = ResolutionRenderer.ViewHeight * 0.5f;
                _resTranslationVector.Z = 0;

                Matrix.CreateTranslation(ref _resTranslationVector, out _resTranslationMatrix);

                _transform = _camTranslationMatrix *
                             _camRotationMatrix *
                             _camScaleMatrix *
                             _resTranslationMatrix *
                             ResolutionRenderer.GetTransformationMatrix();

                _isViewTransformationDirty = false;
            }

            return _transform;
        }

        public void RecalculateTransformationMatrices()
        {
            _isViewTransformationDirty = true;
        }

        /// <summary>
        /// Convert screen coordinates to virtual
        /// </summary>
        /// <param name="coord">Coordinates</param>
        /// <param name="useIrr"></param>
        public Vector2 ToVirtual(Vector2 coord, bool useIrr = true)
        {
            if (useIrr) coord -= new Vector2(ResolutionRenderer.Viewport.X, ResolutionRenderer.Viewport.Y);
            return Vector2.Transform(coord, Matrix.Invert(GetViewTransformationMatrix()));
        }

        /// <summary>
        /// Convert virtual coordinates to screen
        /// </summary>
        /// <param name="coord">Coordinates</param>
        public Vector2 ToDisplay(Vector2 coord)
        {
            return Vector2.Transform(coord, GetViewTransformationMatrix());
        }

        public void Dispose()
        {
            ResolutionRenderer = null;
        }

        public void Update()
        {
            if (Target != null)
            {
                Position = Target.Position;
            }

            if (Room != null)
            {
                var mx = M.Clamp(Position.X, Room.X + .5f * ViewWidth, Room.X + Room.Width -.5f * ViewWidth);
                var my = M.Clamp(Position.Y, Room.Y + .5f * ViewHeight, Room.Y + Room.Height - .5f * ViewHeight);

                Position = new Vector2(mx, my);

                if (Room.Background != -1)
                    Background = Room.Background;
                if (Room.Weather != -1)
                    Weather = Room.Weather;
                if (Room.Darkness != -1)
                    Darkness = Room.Darkness;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            if (Room != null)
            {
                var rh = (float)Room.Height;
                var vh = (float)ViewHeight;
                var ry = Room.Y;
                var vy = ViewY;
                var minYcam = ry;
                var maxYcam = ry + 1.5f * rh - .5f * vh;
                var posY = vy;
                if (minYcam != maxYcam)
                {
                    posY = vy - vh * ((vy - minYcam) / (maxYcam - minYcam));
                }

                var px = (Position.X * .5f) % 256;
                for (var i = -1; i < 2; i++)
                {
                    var posX = Position.X - ViewWidth * .5f + i * ViewWidth - px;
                    sb.Draw(GameResources.Background[Background], new Vector2(posX, posY), null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, G.D_BACKGROUND);
                }
            }

            if (flashAlpha > 0)
            {
                sb.DrawRectangle(new RectF(ViewX, ViewY, ViewWidth, ViewHeight), new Color(Color.White, flashAlpha), true, G.D_EFFECT);
                flashAlpha = Math.Max(flashAlpha - .03f, 0);
            }
        }

        public void Flash()
        {
            flashAlpha = 1.1f;
        }
    }
}
