#region

using System;

#endregion

namespace Gondola.Logic{
    internal interface IGameState : IDisposable{
        void Update(InputState state, double timeDelta);
        void Draw();
    }
}