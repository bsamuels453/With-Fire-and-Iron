#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Forge.Framework.Control;
using Forge.Framework.Resources;
using Forge.Framework.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = MonoGameUtility.Vector2;

#endregion

namespace Forge.Framework.Draw{
    public class Line2D : IDrawableSprite, IUIElement{
        readonly FloatingRectangle _boundingBox;
        readonly int _lineWidth;
        readonly Texture2D _texture;
        public bool Enabled;
        float _angle;

        bool _disposed;

        Vector2 _point1;
        Vector2 _point2;
        Vector2 _uVec;

        public Line2D(
            FrameStrata parentStrata,
            FrameStrata.Level targetStrata,
            Vector2 origin,
            Vector2 dest,
            Color color){
            _texture = new Texture2D(Resource.Device, 1, 1, false, SurfaceFormat.Color);
            _texture.SetData(new[]{color});

            RenderTarget.AddSprite(this);
            float minX = origin.X < dest.X ? origin.X : dest.X;
            float minY = origin.Y < dest.Y ? origin.Y : dest.Y;
            float maxX = origin.X > dest.X ? origin.X : dest.X;
            float maxY = origin.Y > dest.Y ? origin.Y : dest.Y;

            _lineWidth = 1;
            _boundingBox = new FloatingRectangle(minX, minY, maxX - minX, maxY - minY);
            MouseController = new MouseController(this);

            this.FrameStrata = new FrameStrata(targetStrata, parentStrata, "Line2D");
        }

        public float Length { get; private set; }

        public float Angle{
            get { return _angle; }
            set{
                _angle = value;
                _uVec = Common.GetComponentFromAngle(value, 1);
                CalculateDestFromUnitVector();
            }
        }

        public Vector2 OriginPoint{
            get { return _point1; }
            set{
                _point1 = value;
                CalculateInfoFromPoints();
            }
        }

        public Vector2 DestPoint{
            get { return _point2; }
            set{
                _point2 = value;
                CalculateInfoFromPoints();
            }
        }

        #region IDrawableSprite Members

        public void Draw(){
            RenderTarget.CurSpriteBatch.Draw
                (
                    _texture,
                    OriginPoint,
                    null,
                    Color.White*Alpha,
                    _angle,
                    Vector2.Zero,
                    new Vector2(Length, _lineWidth),
                    SpriteEffects.None,
                    FrameStrata.FrameStrataValue
                );
        }

        public void Dispose(){
            Debug.Assert(!_disposed);
            _texture.Dispose();
            RenderTarget.RemoveSprite(this);
            _disposed = true;
        }

        #endregion

        #region IUIElement Members

        public FrameStrata FrameStrata { get; private set; }

        public int X{
            get { return (int) _boundingBox.X; }
            set{
                float diff = value - _boundingBox.X;
                _point1.X += diff;
                _point2.X += diff;
                _boundingBox.X = value;
            }
        }

        public int Y{
            get { return (int) _boundingBox.Y; }
            set{
                float diff = value - _boundingBox.Y;
                _point1.Y += diff;
                _point2.Y += diff;
                _boundingBox.Y = value;
            }
        }

        public int Width{
            get { return (int) _boundingBox.Width; }
        }

        public int Height{
            get { return (int) _boundingBox.Height; }
        }

        public float Alpha { get; set; }

        public MouseController MouseController { get; private set; }

        public bool HitTest(int x, int y){
            return false;
        }

        public List<IUIElement> GetElementStackAtPoint(int x, int y){
            return new List<IUIElement>();
        }

        public void Update(float timeDelta){
        }

        #endregion

        ~Line2D(){
            if (!_disposed){
                Dispose();
                DebugConsole.WriteLine("RESOURCE DISPOSED BY FINALIZER: LINE2D");
            }
        }

        public void TranslateOrigin(float dx, float dy){
            _point1.X += dx;
            _point1.Y += dy;
            CalculateInfoFromPoints();
        }

        public void TranslateDestination(float dx, float dy){
            _point2.X += dx;
            _point2.Y += dy;
            CalculateInfoFromPoints();
        }

        /// <summary>
        ///   calculates the line's destination point from the line's unit vector and length
        /// </summary>
        void CalculateDestFromUnitVector(){
            _point2.X = _uVec.X*Length + _point1.X;
            _point2.Y = _uVec.Y*Length + _point1.Y;
        }

        /// <summary>
        ///   calculates the line's blit location, angle, length, and unit vector based on the origin point and destination point
        /// </summary>
        void CalculateInfoFromPoints(){
            _angle = (float) Math.Atan2(_point2.Y - _point1.Y, _point2.X - _point1.X);
            _uVec = Common.GetComponentFromAngle(_angle, 1);
            Length = Vector2.Distance(_point1, _point2);
        }
    }
}