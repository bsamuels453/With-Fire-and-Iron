#region

using System;
using System.Collections.Generic;
using System.Linq;
using Forge.Core.Logic;
using Forge.Core.ObjectEditor;
using Forge.Framework;
using Forge.Framework.Draw;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Forge.Core.Airship{
    internal class HullIntegrityMesh{
        //-creates overlay mesh for damaged portions
        //-removes hull portions if damage exceeds limits
        //-generates physics block for deflected blows
        //acts as the maanger of the boundingobject for physics
        //generates boundingobject spheres

        ProjectilePhysics.CollisionObjectHandle _collisionObjectHandle;
        Func<HullSection, bool> _disableHullSection;
        Func<HullSection, bool> _enableHullSection;
        readonly ObjectBuffer<HullSection> _redBuff;
        readonly ObjectBuffer<HullSection> _orangeBuff;
        readonly ObjectBuffer<HullSection> _greenBuff;

        const float _meshOffsetRed = 1.005f;
        const float _meshOffsetOrange = 1.010f;
        const float _meshOffsetGreen = 1.015f;
        public HullIntegrityMesh(
            ObjectBuffer<HullSection>[] hullBuffers,
            float length){

            int f = 4;
            int numObjs = hullBuffers[0].MaxObjects*hullBuffers.Length;

            _redBuff = new ObjectBuffer<HullSection>(
                numObjs,
                hullBuffers[0].IndiciesPerObject/3,
                hullBuffers[0].VerticiesPerObject,
                hullBuffers[0].IndiciesPerObject,
                "Shader_DamageMeshRed"
                );
            _orangeBuff = new ObjectBuffer<HullSection>(
                numObjs,
                hullBuffers[0].IndiciesPerObject/3,
                hullBuffers[0].VerticiesPerObject,
                hullBuffers[0].IndiciesPerObject,
                "Shader_DamageMeshOrange"
                );
            _greenBuff = new ObjectBuffer<HullSection>(
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

        }

        public Matrix WorldMatrix{
            set {
                _redBuff.WorldMatrix = value;
                _orangeBuff.WorldMatrix = value;
                _greenBuff.WorldMatrix = value;
            }

        }

        void OnCollision(Vector3 position, Vector3 velocity){
            throw new NotImplementedException();
        }

        void UpdateDamageTexture(Vector3 position){
            //make sure position is unwrapped and is not in world coordinates
            throw new NotImplementedException();
        }
    }
}