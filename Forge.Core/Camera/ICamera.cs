#region

using MonoGameUtility;

#endregion

namespace Forge.Core.Camera{
    public interface ICamera{
        Matrix ViewMatrix { get; }
    }
}