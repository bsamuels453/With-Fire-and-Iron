#region

using System;
using Forge.Framework.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Forge.Framework.Draw{
    internal class Line2D : IDrawableSprite{
        readonly Line _parent;
        //Color _color;
        bool _isDisposed;
        Texture2D _texture;


        public Line2D(Line parent, Color color){
            _isDisposed = false;
            _texture = new Texture2D(Gbl.Device, 1, 1, false, SurfaceFormat.Color);
            _texture.SetData(new[]{color});
            _parent = parent;
            RenderTarget.Sprites.Add(this);
        }

        #region IDrawableSprite Members

        public void Draw(){
            RenderTarget.CurSpriteBatch.Draw(
                _texture,
                _parent.OriginPoint,
                null,
                Color.White*_parent.Alpha,
                _parent.Angle,
                Vector2.Zero,
                new Vector2(_parent.Length, _parent.LineWidth),
                SpriteEffects.None,
                _parent.Depth
                );
        }

        public Texture2D Texture{
            get { return _texture; }
            set { _texture = value; }
        }

        public void SetTextureFromString(string textureName){
            throw new NotImplementedException();
        }

        public void Dispose(){
            if (!_isDisposed){
                RenderTarget.Sprites.Remove(this);
                _isDisposed = true;
            }
        }

        #endregion

        ~Line2D(){
            if (!_isDisposed){
                _isDisposed = true;
            }
        }
    }
}