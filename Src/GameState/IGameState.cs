#region

using System;
using Gondola.Logic;

#endregion

namespace Gondola.GameState{
    internal interface IGameState : IDisposable{
        void Update(InputState state, double timeDelta);
        void Draw();
    }
}