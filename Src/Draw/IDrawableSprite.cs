using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gondola.Draw{
    internal interface IDrawableSprite{
        Texture2D Texture { get; set; }
        void Draw();
        void SetTextureFromString(string textureName);
        void Dispose();
    }
    internal interface IDrawableBuffer {
        void Draw(Matrix viewMatrix);
    }
}