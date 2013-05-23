#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Forge.Core.Logic;
using Forge.Framework.Draw;
using Microsoft.Xna.Framework;

#endregion

namespace Forge.Core.Airship{
    internal class HullIntegrityMesh{
        //-creates overlay mesh for damaged portions
        //-removes hull portions if damage exceeds limits
        //-generates physics block for deflected blows
        //acts as the maanger of the boundingobject for physics
        //generates boundingobject spheres

        const float _meshOffsetRed = 1.005f;
        const float _meshOffsetOrange = 1.010f;
        const float _meshOffsetGreen = 1.015f;
        readonly ObjectBuffer<int> _greenBuff;

        readonly List<HullPlate> _hullPlates;
        readonly ObjectBuffer<int> _orangeBuff;
        readonly ObjectBuffer<int> _redBuff;
        ProjectilePhysics.CollisionObjectHandle _collisionObjectHandle;
        Func<HullSectionIdentifier, bool> _disableHullSection;
        Func<HullSectionIdentifier, bool> _enableHullSection;

        public HullIntegrityMesh(
            ObjectBuffer<int>[] hullBuffers,
            ProjectilePhysics projectilePhysics,
            Vector3 shipCentroid,
            float length) {

            #region setup collision objects

            _hullPlates = new List<HullPlate>(4000);
            var cumulativeBufferData = new List<ObjectBuffer<int>.ObjectData>(hullBuffers.Length*hullBuffers[0].MaxObjects);
            foreach (var buffer in hullBuffers){
                cumulativeBufferData.AddRange(buffer.DumpObjectData());
            }

            var groupedHullSections = (from obj in cumulativeBufferData
                group obj by obj.Identifier).ToArray();

            var collisionObjects = new List<ProjectilePhysics.CollisionObject>();
            int id = 0; //later we make the id associated with plates

            foreach (var section in groupedHullSections){
                //need to get 4 representative vertexes of this section of hull
                var cumulativeVerts = new List<Vector3>(30);
                foreach (var obj in section){
                    cumulativeVerts.AddRange(from v in obj.Verticies select v.Position);
                }
                float maxY = cumulativeVerts.Max(v => v.Y);
                float minY = cumulativeVerts.Max(v => v.Y);

                //wish there was a way to linq  this next part, but there isnt
                Vector3 invalidVert = new Vector3(-1, -1, -1);
                Vector3 maxXmaxY = invalidVert;
                Vector3 minXmaxY = invalidVert;
                Vector3 maxXminY = invalidVert;
                Vector3 minXminY = invalidVert;
                float maxXtop = float.MinValue;
                float minXtop = float.MaxValue;
                float maxXbot = float.MinValue;
                float minXbot = float.MaxValue;
                // ReSharper disable CompareOfFloatsByEqualityOperator
                foreach (var vert in cumulativeVerts){
                    if (vert.Y == maxY){
                        if (vert.X > maxXtop){
                            maxXmaxY = vert;
                            maxXtop = vert.X;
                        }
                        if (vert.X < minXtop){
                            minXmaxY = vert;
                            minXtop = vert.X;
                        }
                    }
                    if (vert.Y == minY){
                        if (vert.X > maxXbot){
                            maxXminY = vert;
                            maxXbot = vert.X;
                        }
                        if (vert.X < minXbot){
                            minXminY = vert;
                            minXbot = vert.X;
                        }
                    }
                }
                // ReSharper restore CompareOfFloatsByEqualityOperator
                Debug.Assert(maxXmaxY != invalidVert);
                Debug.Assert(minXmaxY != invalidVert);
                Debug.Assert(maxXminY != invalidVert);
                Debug.Assert(minXminY != invalidVert);
                collisionObjects.Add(new ProjectilePhysics.CollisionObject(id, new[]{minXmaxY, maxXmaxY, maxXminY}));
                collisionObjects.Add(new ProjectilePhysics.CollisionObject(id, new[]{minXmaxY, minXminY, maxXminY}));

                _hullPlates.Add(new HullPlate(id, section.Key));

                id++;
            }
            _hullPlates.TrimExcess();

            #endregion

            var boundingSphere = new BoundingSphere(shipCentroid, length);

            projectilePhysics.AddShipCollisionObjects(collisionObjects.ToArray(), boundingSphere, ProjectilePhysics.EntityVariant.EnemyShip, OnCollision);

            //initalize mesh buffers
            #region

            int numObjs = hullBuffers.Sum(b => b.MaxObjects);

            _redBuff = new ObjectBuffer<int>(
                numObjs,
                hullBuffers[0].IndiciesPerObject/3,
                hullBuffers[0].VerticiesPerObject,
                hullBuffers[0].IndiciesPerObject,
                "Shader_DamageMeshRed"
                );
            _orangeBuff = new ObjectBuffer<int>(
                numObjs,
                hullBuffers[0].IndiciesPerObject/3,
                hullBuffers[0].VerticiesPerObject,
                hullBuffers[0].IndiciesPerObject,
                "Shader_DamageMeshOrange"
                );
            _greenBuff = new ObjectBuffer<int>(
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

            _redBuff.ApplyTransform((vertex) =>{
                                        vertex.Position *= _meshOffsetRed;
                                        vertex.Position.X += lenOffset;
                                        return vertex;
                                    }
                );

            lenOffset = (length*_meshOffsetOrange - length)/2;
            _orangeBuff.ApplyTransform((vertex) =>{
                                           vertex.Position *= _meshOffsetOrange;
                                           vertex.Position.X += lenOffset;
                                           return vertex;
                                       }
                );

            lenOffset = (length*_meshOffsetGreen - length)/2;
            _greenBuff.ApplyTransform((vertex) =>{
                                          vertex.Position *= _meshOffsetGreen;
                                          vertex.Position.X += lenOffset;
                                          return vertex;
                                      }
                );

            #endregion
        }

        public Matrix WorldMatrix{
            set{
                _redBuff.WorldMatrix = value;
                _orangeBuff.WorldMatrix = value;
                _greenBuff.WorldMatrix = value;
            }
        }

        void OnCollision(int id, Vector3 position, Vector3 velocity){
            int f = 4;
        }

        void UpdateDamageTexture(Vector3 position){
            //make sure position is unwrapped and is not in world coordinates
            throw new NotImplementedException();
        }

        //todo: generalize this to HullSection
        #region Nested type: HullPlate

        struct HullPlate{
            public int Id;
            public IEquatable<int> SectionIdentifier;

            public HullPlate(int id, IEquatable<int> sectionIdentifier){
                Id = id;
                SectionIdentifier = sectionIdentifier;
            }
        }

        #endregion
    }
}