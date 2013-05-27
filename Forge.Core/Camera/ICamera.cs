#region

using Forge.Framework;
using Microsoft.Xna.Framework;

#endregion

namespace Forge.Core.Camera{
    internal interface ICamera{
        Matrix ViewMatrix { get; }
        void Update(ref InputState state, double timeDelta);
    }
}