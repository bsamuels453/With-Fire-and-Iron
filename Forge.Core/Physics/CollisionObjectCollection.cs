#region

using System.Collections.Generic;
using MonoGameUtility;

#endregion

namespace Forge.Core.Physics{
    public delegate void CollisionCallback(int id, Vector3 localCollisionPoint, Ray localCollisionRay, Ray globalCollisionRay);

    /// <summary>
    /// used to group together collision objects, such as the plates on the side of an airship, into one datum.
    /// </summary>
    public class CollisionObjectCollection{
        public readonly List<Projectile> BlacklistedProjectiles;

        /// <summary>
        ///   Position of target sphere, velocity of projectile relative to sphere Implement projectile relative speed multiplier here
        /// </summary>
        public readonly CollisionCallback CollisionEventDispatcher;

        public readonly CollisionObject[] CollisionObjects;
        public readonly int FactionId;
        public readonly BoundingSphere ShipSOI;

        public Matrix WorldTransform;

        public CollisionObjectCollection(CollisionObject[] collisionObjects, int factionId, BoundingSphere soi, CollisionCallback collisionEventDispatcher){
            CollisionObjects = collisionObjects;
            FactionId = factionId;
            WorldTransform = Matrix.Identity;
            ShipSOI = soi;
            CollisionEventDispatcher = collisionEventDispatcher;
            BlacklistedProjectiles = new List<Projectile>();
        }
    }
}