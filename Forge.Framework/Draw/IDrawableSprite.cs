#region

using MonoGameUtility;

#endregion

namespace Forge.Framework.Draw{
    public interface IDrawableSprite{
        void Draw();
        void SetTextureFromString(string textureName);
        void Dispose();
    }

    public interface IDrawableBuffer{
        void Draw(Matrix viewMatrix);
    }
}