#region

using Microsoft.Xna.Framework;

#endregion

namespace Gondola.Draw{
    internal interface IDrawableBuffer{
        void Draw(Matrix viewMatrix);
    }
}