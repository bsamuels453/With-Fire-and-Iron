#region

using System;
using MonoGameUtility;

#endregion

namespace Forge.Core.Physics{
    /// <summary>
    ///   Returned on the creation of a collision object. This class allows external classes to manipulate the collision object it created.
    /// </summary>
    public class CollisionObjectHandle{
        public readonly Action<Matrix> SetObjectMatrix;
        public readonly Action Terminate;

        public CollisionObjectHandle(Action<Matrix> setObjectMatrix, Action terminate){
            SetObjectMatrix = setObjectMatrix;
            Terminate = terminate;
        }
    }
}