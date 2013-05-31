#region

using Forge.Framework;
using MonoGameUtility;

#endregion

namespace Forge.Core.Camera{
    internal interface ICamera{
        Matrix ViewMatrix { get; }
        void Update(ref InputState state, double timeDelta);
    }
}