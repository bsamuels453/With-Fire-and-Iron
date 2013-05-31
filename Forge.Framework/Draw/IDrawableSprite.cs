#region

using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;

#endregion

namespace Forge.Framework.Draw{
    public interface IDrawableSprite{
        Texture2D Texture { get; set; }
        void Draw();
        void SetTextureFromString(string textureName);
        void Dispose();
    }

    public interface IDrawableBuffer{
        void Draw(Matrix viewMatrix);
    }
}