#region

using System.Diagnostics;
using MonoGameUtility;

#endregion

namespace Forge.Core.Physics{
    /// <summary>
    ///   Used to define an object bullets can collide with. IntersectPoints are used for 
    ///   fast-checking whether or not projectiles intersect this object.
    /// </summary>
    public struct CollisionObject{
        public int Id;
        public Vector3[] Vertexes;

        public CollisionObject(int id, Vector3[] vertexes){
            Id = id;
            Vertexes = vertexes;
            Debug.Assert(vertexes.Length == 3);
        }
    }
}