#region

using System.Collections.Generic;
using MonoGameUtility;

#endregion

namespace Forge.Core.Physics{
    /// <summary>
    /// used to group together collision objects, such as the plates on the side of an airship, into one datum.
    /// </summary>
    public class CollisionObjectCollection{
        public readonly List<Projectile> BlacklistedProjectiles;
        public readonly CollisionObject[] CollisionObjects;
        public readonly int FactionId;
        public readonly BoundingSphere ShipSOI;

        /// <summary>
        ///   Position of target sphere, velocity of projectile relative to sphere Implement projectile relative speed multiplier here
        /// </summary>
        public ProjectilePhysics.CollisionCallback CollisionEventDispatcher;

        public Matrix WorldTransform;

        public CollisionObjectCollection(CollisionObject[] collisionObjects, int factionId, BoundingSphere soi){
            CollisionObjects = collisionObjects;
            FactionId = factionId;
            WorldTransform = Matrix.Identity;
            ShipSOI = soi;
            BlacklistedProjectiles = new List<Projectile>();
        }
    }
}