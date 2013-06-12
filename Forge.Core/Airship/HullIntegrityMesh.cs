#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Forge.Core.Airship.Data;
using Forge.Core.Physics;
using Forge.Framework.Draw;
using MonoGameUtility;

#endregion

namespace Forge.Core.Airship{
    public class HullIntegrityMesh : IDisposable{
        //-creates overlay mesh for damaged portions
        //-removes hull portions if damage exceeds limits
        //-generates physics block for deflected blows
        //acts as the maanger of the boundingobject for physics
        //generates boundingobject spheres

        const float _meshOffsetRed = 1.005f;
        const float _meshOffsetOrange = 1.010f;
        const float _meshOffsetGreen = 1.015f;
        readonly ProjectilePhysics.CollisionObjectHandle _collisionObjectHandle;
        readonly ObjectBuffer<int> _greenBuff;

        readonly ObjectBuffer<int> _orangeBuff;
        readonly ObjectBuffer<int> _redBuff;
        bool _disposed;

        public HullIntegrityMesh(
            HullSectionContainer hullSections,
            ProjectilePhysics projectilePhysics,
            Vector3 shipCentroid,
            float length){
            #region setup collision objects

            var hullBuffers = hullSections.HullBuffersByDeck;
            var cumulativeBufferData = new List<ObjectBuffer<int>.ObjectData>(hullBuffers.Length*hullBuffers[0].MaxObjects);
            foreach (var buffer in hullBuffers){
                cumulativeBufferData.AddRange(buffer.DumpObjectData());
            }

            var collisionObjects = new List<ProjectilePhysics.CollisionObject>();

            foreach (var section in hullSections){
                var cSection = (HullSection) section;
                /*
                collisionObjects.Add(new ProjectilePhysics.CollisionObject(cSection.Uid, new[] { 
                    new Vector3(-100,-100,-100),
                    new Vector3(-100,-100,100),
                    new Vector3(100,-100,100)
                }));
                collisionObjects.Add(new ProjectilePhysics.CollisionObject(cSection.Uid, new[] { 
                    new Vector3(-100,-100,-100),
                    new Vector3(100,-100,-100),
                    new Vector3(100,-100,100)
                                    }));
                 */

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
                            cSection.AliasedVertexes[3],
                            cSection.AliasedVertexes[4],
                            cSection.AliasedVertexes[5]
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

            //initalize mesh buffers

            #region

            int numObjs = hullBuffers.Sum(b => b.MaxObjects);

            _redBuff = new ObjectBuffer<int>
                (
                numObjs,
                hullBuffers[0].IndiciesPerObject/3,
                hullBuffers[0].VerticiesPerObject,
                hullBuffers[0].IndiciesPerObject,
                "Shader_DamageMeshRed"
                );
            _orangeBuff = new ObjectBuffer<int>
                (
                numObjs,
                hullBuffers[0].IndiciesPerObject/3,
                hullBuffers[0].VerticiesPerObject,
                hullBuffers[0].IndiciesPerObject,
                "Shader_DamageMeshOrange"
                );
            _greenBuff = new ObjectBuffer<int>
                (
                numObjs,
                hullBuffers[0].IndiciesPerObject/3,
                hullBuffers[0].VerticiesPerObject,
                hullBuffers[0].IndiciesPerObject,
                "Shader_DamageMeshGreen"
                );

            foreach (var buffer in hullBuffers){
                _redBuff.AbsorbBuffer(buffer, true, false);
            }
            _orangeBuff.AbsorbBuffer(_redBuff, true, false);
            _greenBuff.AbsorbBuffer(_redBuff, true, false);

            float lenOffset = (length*_meshOffsetRed - length)/2;

            _redBuff.ApplyTransform
                ((vertex) =>{
                     vertex.Position *= _meshOffsetRed;
                     vertex.Position.X += lenOffset;
                     return vertex;
                 }
                );

            lenOffset = (length*_meshOffsetOrange - length)/2;
            _orangeBuff.ApplyTransform
                ((vertex) =>{
                     vertex.Position *= _meshOffsetOrange;
                     vertex.Position.X += lenOffset;
                     return vertex;
                 }
                );

            lenOffset = (length*_meshOffsetGreen - length)/2;
            _greenBuff.ApplyTransform
                ((vertex) =>{
                     vertex.Position *= _meshOffsetGreen;
                     vertex.Position.X += lenOffset;
                     return vertex;
                 }
                );

            #endregion
        }

        public Matrix WorldTransform{
            set{
                _redBuff.WorldTransform = value;
                _orangeBuff.WorldTransform = value;
                _greenBuff.WorldTransform = value;
                _collisionObjectHandle.SetObjectMatrix(value);
            }
        }

        #region IDisposable Members

        public void Dispose(){
            Debug.Assert(!_disposed);
            _greenBuff.Dispose();
            _orangeBuff.Dispose();
            _redBuff.Dispose();
            _disposed = true;
        }

        #endregion

        void OnCollision(int id, Vector3 position, Vector3 velocity){
            if (_greenBuff.IsObjectEnabled(id)){
                _greenBuff.DisableObject(id);
            }
        }

        void UpdateDamageTexture(Vector3 position){
            //make sure position is unwrapped and is not in world coordinates
            throw new NotImplementedException();
        }

        ~HullIntegrityMesh(){
            Debug.Assert(_disposed);
        }
    }
}