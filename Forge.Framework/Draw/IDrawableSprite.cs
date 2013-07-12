#region

using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;

#endregion

namespace Forge.Framework.Draw{
    public interface IDrawableSprite{
        void Draw();
        void Dispose();
    }

    public interface IDrawableBuffer{
        EffectParameterCollection ShaderParams { get; }
        void Draw(Matrix viewMatrix);
    }
}