#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Forge.Core.Airship.Data;
using Forge.Core.Physics;
using Forge.Framework.Draw;
using MonoGameUtility;

#endregion

namespace Forge.Core.Airship{
    /// <summary>
    /// This class is used to track and display damage done to an airship's hull through
    /// texture decaling.
    /// </summary>
    public class HullIntegrityMesh : IDisposable{
        readonly float _airshipLength;
        readonly HullSectionContainer _hullSectionContainer;
        readonly CollisionObjectHandle _portCollisionHandle;
        readonly CollisionObjectHandle _starboardCollisionHandle;
        bool _disposed;

        public HullIntegrityMesh(
            HullSectionContainer hullSections,
            ProjectilePhysics projectilePhysics,
            Vector3 shipCentroid,
            float length){
            #region setup collision objects

            _hullSectionContainer = hullSections;
            var hullBuffers = hullSections.HullBuffersByDeck;
            _airshipLength = length;
            var cumulativeBufferData = new List<ObjectBuffer<int>.ObjectData>(hullBuffers.Length*hullBuffers[0].MaxObjects);
            foreach (var buffer in hullBuffers){
                cumulativeBufferData.AddRange(buffer.DumpObjectData());
            }


            var portCollisionObjects = new List<CollisionObject>();
            var starboardCollisionObjects = new List<CollisionObject>();

            foreach (var section in hullSections){
                var cSection = (HullSection) section;

                var col = cSection.Side == Quadrant.Side.Port ? portCollisionObjects : starboardCollisionObjects;

                col.Add
                    (new CollisionObject
                        (cSection.GetHashCode(), new[]{
                            cSection.AliasedVertexes[0],
                            cSection.AliasedVertexes[1],
                            cSection.AliasedVertexes[2]
                        }));


                col.Add
                    (new CollisionObject
                        (cSection.GetHashCode(), new[]{
                            cSection.AliasedVertexes[0],
                            cSection.AliasedVertexes[2],
                            cSection.AliasedVertexes[3]
                        }));
            }

            #endregion

            var portCentroid = new BoundingSphere(shipCentroid + new Vector3(0, 0, -1), length);
            var starboardCentroid = new BoundingSphere(shipCentroid + new Vector3(0, 0, 1), length);

            _portCollisionHandle = projectilePhysics.AddCollisionObjectCollection
                (
                    portCollisionObjects.ToArray(),
                    portCentroid,
                    1,
                    OnCollision
                );

            _starboardCollisionHandle = projectilePhysics.AddCollisionObjectCollection
                (
                    starboardCollisionObjects.ToArray(),
                    starboardCentroid,
                    1,
                    OnCollision
                );
        }

        public Matrix WorldTransform{
            set{
                _portCollisionHandle.SetObjectMatrix(value);
                _starboardCollisionHandle.SetObjectMatrix(value);
            }
        }

        #region IDisposable Members

        public void Dispose(){
            Debug.Assert(!_disposed);
            _disposed = true;
        }

        #endregion

        void OnCollision(int id, Vector3 localCollisionPoint, Ray localCollisionRay, Ray globalCollisionRay) {
            var side = Quadrant.PointToSide(localCollisionPoint.Z);
            _hullSectionContainer.AddDamageDecal(new Vector2(localCollisionPoint.X + _airshipLength / 2, Math.Abs(localCollisionPoint.Y)), side);
        }

        ~HullIntegrityMesh(){
            Debug.Assert(_disposed);
        }
    }
}