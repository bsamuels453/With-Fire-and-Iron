#region

using System.Collections.Generic;
using Forge.Framework.Control;
using Forge.Framework.Resources;
using Forge.Framework.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rectangle = MonoGameUtility.Rectangle;
using Vector2 = MonoGameUtility.Vector2;

#endregion

namespace Forge.Framework.Draw{
    internal class Sprite2D : IDrawableSprite, IUIElement{
        readonly FloatingRectangle _srcRect;

        public bool Enabled;

        Rectangle _destRect;
        bool _isDisposed;
        Texture2D _texture;

        /// <summary>
        ///   constructor for a normal sprite
        /// </summary>
        public Sprite2D(string textureName, int x, int y, int width, int height, FrameStrata targetStrata, float alpha = 1, float spriteRepeatX = 1,
            float spriteRepeatY = 1){
            _texture = Resource.LoadContent<Texture2D>(textureName);
            _srcRect = new FloatingRectangle(0f, 0f, _texture.Height*spriteRepeatX, _texture.Width*spriteRepeatY);
            _destRect = new Rectangle();
            _isDisposed = false;
            X = x;
            Y = y;
            _destRect = new Rectangle();
            Width = width;
            Height = height;
            FrameStrata = targetStrata;
            Alpha = alpha;
            Enabled = true;
            MouseController = new MouseController(this);
            RenderTarget.Sprites.Add(this);
        }

        #region IDrawableSprite Members

        public Texture2D Texture{
            set { _texture = value; }
            get { return _texture; }
        }

        public void Dispose(){
            if (!_isDisposed){
                _isDisposed = true;
                RenderTarget.Sprites.Remove(this);
            }
        }

        public void SetTextureFromString(string textureName){
            _texture = Resource.LoadContent<Texture2D>(textureName);
        }

        public void Draw(){
            if (Enabled){
                RenderTarget.CurSpriteBatch.Draw
                    (
                        _texture,
                        _destRect,
                        (Rectangle?) _srcRect,
                        Color.White*Alpha,
                        0,
                        Vector2.Zero,
                        SpriteEffects.None,
                        FrameStrata.FrameStrataValue
                    );
            }
        }

        #endregion

        #region IUIElement Members

        public FrameStrata FrameStrata { get; private set; }

        public int X{
            get { return _destRect.X; }
            set { _destRect.X = value; }
        }

        public int Y{
            get { return _destRect.Y; }
            set { _destRect.Y = value; }
        }

        public int Width{
            get { return _destRect.Width; }
            set { _destRect.Width = value; }
        }

        public int Height{
            get { return _destRect.Height; }
            set { _destRect.Height = value; }
        }

        public float Alpha { get; set; }

        public MouseController MouseController { get; private set; }

        public bool HitTest(int x, int y){
            return _destRect.Contains(x, y);
        }

        public List<IUIElement> GetElementStackAtPoint(int x, int y){
            if (HitTest(x, y)){
                var ret = new List<IUIElement>(1);
                ret.Add(this);
                return ret;
            }
            return new List<IUIElement>();
        }

        public void InitializeEvents(UIElementCollection parent){
        }

        #endregion

        ~Sprite2D(){
            if (!_isDisposed){
                _isDisposed = true;
            }
        }
    }
}