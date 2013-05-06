#region

using System;
using Forge.Core.Logic;
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
        ObjectBuffer<HullSection> _hullDamageOverlay;

        readonly GeometryBuffer<VertexPositionNormalTexture> _redBuff;
        readonly GeometryBuffer<VertexPositionNormalTexture> _orangeBuff;
        readonly GeometryBuffer<VertexPositionNormalTexture> _greenBuff;

        const float meshOffsetRed = 1.005f;
        const float meshOffsetOrange = 1.010f;
        const float meshOffsetGreen = 1.015f;
        public HullIntegrityMesh(
            VertexPositionNormalTexture[] verts,
            int[] indicieDump,
            float length){

            var greenVerts = (VertexPositionNormalTexture[])verts.Clone();
            var redVerts = (VertexPositionNormalTexture[])verts.Clone();
            var orangeVerts = (VertexPositionNormalTexture[])verts.Clone();

            float lenOffset = (length * meshOffsetGreen - length) / 2;
            for (int i = 0; i < verts.Length; i++){
                Vector3 newPos = verts[i].Position * meshOffsetGreen;
                newPos.X += lenOffset;
                greenVerts[i].Position = newPos;
            }

            lenOffset = (length * meshOffsetOrange - length) / 2;
            for (int i = 0; i < verts.Length; i++) {
                Vector3 newPos = verts[i].Position * meshOffsetOrange;
                newPos.X += lenOffset;
                orangeVerts[i].Position = newPos;
            }

            lenOffset = (length * meshOffsetRed - length) / 2;
            for (int i = 0; i < verts.Length; i++) {
                Vector3 newPos = verts[i].Position * meshOffsetRed;
                newPos.X += lenOffset;
                redVerts[i].Position = newPos;
            }

            _redBuff = new GeometryBuffer<VertexPositionNormalTexture>(indicieDump.Length, verts.Length, indicieDump.Length/3, "Shader_DamageMeshRed");
            _orangeBuff = new GeometryBuffer<VertexPositionNormalTexture>(indicieDump.Length, verts.Length, indicieDump.Length / 3, "Shader_DamageMeshOrange");
            _greenBuff = new GeometryBuffer<VertexPositionNormalTexture>(indicieDump.Length, verts.Length, indicieDump.Length / 3, "Shader_DamageMeshGreen");
            _redBuff.IndexBuffer.SetData(indicieDump);
            _orangeBuff.IndexBuffer.SetData(indicieDump);
            _greenBuff.IndexBuffer.SetData(indicieDump);

            _redBuff.VertexBuffer.SetData(redVerts);
            _orangeBuff.VertexBuffer.SetData(orangeVerts);
            _greenBuff.VertexBuffer.SetData(greenVerts);
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