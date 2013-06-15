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
    public class HullIntegrityMesh : IDisposable{
        readonly float _airshipLength;
        readonly ProjectilePhysics.CollisionObjectHandle _collisionObjectHandle;
        readonly HullSectionContainer _hullSectionContainer;
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

            var collisionObjects = new List<ProjectilePhysics.CollisionObject>();

            foreach (var section in hullSections){
                var cSection = (HullSection) section;

                collisionObjects.Add
                    (new ProjectilePhysics.CollisionObject
                        (cSection.GetHashCode(), new[]{
                            cSection.AliasedVertexes[0],
                            cSection.AliasedVertexes[1],
                            cSection.AliasedVertexes[2]
                        }));
                collisionObjects.Add
                    (new ProjectilePhysics.CollisionObject
                        (cSection.GetHashCode(), new[]{
                            cSection.AliasedVertexes[0],
                            cSection.AliasedVertexes[3],
                            cSection.AliasedVertexes[2]
                        }));
            }

            #endregion

            var boundingSphere = new BoundingSphere(shipCentroid, length);

            _collisionObjectHandle = projectilePhysics.AddShipCollisionObjects
                (
                    collisionObjects.ToArray(),
                    boundingSphere,
                    ProjectilePhysics.EntityVariant.EnemyShip,
                    OnCollision
                );
        }

        public Matrix WorldTransform{
            set { _collisionObjectHandle.SetObjectMatrix(value); }
        }

        #region IDisposable Members

        public void Dispose(){
            Debug.Assert(!_disposed);
            _disposed = true;
        }

        #endregion

        void OnCollision(int id, Vector3 position, Vector3 velocity){
            var side = Quadrant.PointToSide(position.Z);
            _hullSectionContainer.AddDamageDecal(new Vector2(_airshipLength + position.X, Math.Abs(position.Y)), side);
        }

        ~HullIntegrityMesh(){
            Debug.Assert(_disposed);
        }
    }
}