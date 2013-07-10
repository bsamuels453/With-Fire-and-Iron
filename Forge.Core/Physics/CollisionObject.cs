#region

using MonoGameUtility;

#endregion

namespace Forge.Core.Physics{
    /// <summary>
    ///   Used to define an object bullets can collide with. IntersectPoints are used for 
    ///   fast-checking whether or not projectiles intersect this object.
    /// </summary>
    public struct CollisionObject{
        public readonly Vector3 Centroid;
        public readonly int Id;
        public readonly Vector3 Normal;
        public readonly Vector3[] Vertexes;

        public CollisionObject(int id, Vector3[] vertexes){
            Id = id;
            Vertexes = vertexes;
            //Debug.Assert(vertexes.Length == 3);
            Centroid = new Vector3();
            foreach (var vertex in vertexes){
                Centroid += vertex;
            }
            Centroid /= vertexes.Length;
            Vector3 crossSum = new Vector3();
            crossSum += Vector3.Cross(vertexes[1] - vertexes[0], vertexes[2] - vertexes[0]);
            crossSum.Normalize();
            Normal = crossSum;
        }

        public bool IsInRange(Vector3 target, float range){
            foreach (Vector3 t in Vertexes){
                var dist = Vector3.Distance(t, target);
                if (dist <= range){
                    return true;
                }
            }
            return false;
        }
    }
}