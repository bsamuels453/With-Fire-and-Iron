#region

using System;
using Forge.Framework;

#endregion

namespace Forge.Core.GameState{
    public interface IGameState : IDisposable{
        void Update(double timeDelta);
        void Draw();
    }
}