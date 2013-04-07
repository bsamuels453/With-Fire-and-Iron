#region

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace Forge.Core.Logic{
    internal class ProjectilePhysics{
        #region ObjectVariant enum

        public enum ObjectVariant{
            EnemyShip,
            AllyShip
        }

        #endregion

        public BoundingObject AddBoundingObject(List<BoundingSphere> spheres, ObjectVariant variant){
            throw new Exception();
        }

        public Projectile AddProjectile(Vector3 position, Vector3 velocity, ObjectVariant collisionFilter){
            throw new NotImplementedException();
        }

        #region Nested type: BoundingObject

        public struct BoundingObject{
            public Action<Matrix> SetObjectMatrix;
            public Action Terminate;
            public event Action<Vector3, Vector3> OnCollision;
        }

        #endregion

        #region Nested type: BoundingObjectData

        struct BoundingObjectData{
            public readonly List<BoundingSphere> BoundingSpheres;
            public readonly object Owner;
            public readonly ObjectVariant Type;
        }

        #endregion

        #region Nested type: Projectile

        public struct Projectile{
            public Func<Vector3> GetPosition;
            public Action Terminate;
            //public event Action<float, Vector3, Vector3> OnCollision; //theres no real reason for the projectile to care about OnCollision (yet)
        }

        #endregion
    }
}