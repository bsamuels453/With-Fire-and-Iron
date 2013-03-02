#region

using Gondola.GameState;
using Microsoft.Xna.Framework;

#endregion

namespace Gondola.Logic{
    internal interface ICamera{
        Matrix ViewMatrix { get; }
        void Update(ref InputState state, double timeDelta);
    }
}