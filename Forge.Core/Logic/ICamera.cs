#region

using Forge.Framework;
using Microsoft.Xna.Framework;

#endregion

namespace Forge.Core.Logic{
    internal interface ICamera{
        Matrix ViewMatrix { get; }
        void Update(ref InputState state, double timeDelta);
    }
}