#region

using System;
using BulletXNA.LinearMath;
using Forge.Framework;
using Forge.Framework.Draw;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;
using Vector2 = Microsoft.Xna.Framework.Vector2;

#endregion

namespace Forge.Core{
    internal class DebugDraw : IDebugDraw, IDisposable{
        const int _maxLines = 50000;
        readonly GeometryBuffer<VertexPositionTexture> _lineBuffer;
        readonly VertexPositionTexture[] _lines;
        int _numLines;


        public DebugDraw(){
            _lineBuffer = new GeometryBuffer<VertexPositionTexture>
                (_maxLines*2, _maxLines*2, _maxLines, "Config/Shaders/Wireframe.config", PrimitiveType.LineList);
            int[] indicies = new int[_maxLines*2];
            for (int i = 0; i < _maxLines*2; i++){
                indicies[i] = i;
            }
            _lines = new VertexPositionTexture[_maxLines*2];
            _lineBuffer.SetIndexBufferData(indicies);
        }

        #region IDebugDraw Members

        public void DrawLine(IndexedVector3 @from, IndexedVector3 to, IndexedVector3 color){
#if !NO_REFRESH
            AddLine(@from, to);
#endif
        }

        public void DrawLine(ref IndexedVector3 @from, ref IndexedVector3 to, ref IndexedVector3 fromColor){
#if !NO_REFRESH
            AddLine(@from, to);
#endif
        }

        public void DrawLine(ref IndexedVector3 @from, ref IndexedVector3 to, ref IndexedVector3 fromColor, ref IndexedVector3 toColor){
            throw new NotImplementedException();
        }

        public void DrawBox(ref IndexedVector3 bbMin, ref IndexedVector3 bbMax, ref IndexedVector3 color){
            throw new NotImplementedException();
        }

        public void DrawBox(ref IndexedVector3 bbMin, ref IndexedVector3 bbMax, ref IndexedMatrix trans, ref IndexedVector3 color){
            Matrix mtx = trans;

            var v1 = new Vector3(bbMin.X, bbMin.Y, bbMin.Z);
            var v2 = new Vector3(bbMin.X, bbMin.Y, bbMax.Z);
            var v3 = new Vector3(bbMax.X, bbMin.Y, bbMax.Z);
            var v4 = new Vector3(bbMax.X, bbMin.Y, bbMin.Z);

            var v5 = new Vector3(bbMin.X, bbMax.Y, bbMin.Z);
            var v6 = new Vector3(bbMin.X, bbMax.Y, bbMax.Z);
            var v7 = new Vector3(bbMax.X, bbMax.Y, bbMax.Z);
            var v8 = new Vector3(bbMax.X, bbMax.Y, bbMin.Z);

            v1 = Common.MultMatrix(mtx, v1);
            v2 = Common.MultMatrix(mtx, v2);
            v3 = Common.MultMatrix(mtx, v3);
            v4 = Common.MultMatrix(mtx, v4);
            v5 = Common.MultMatrix(mtx, v5);
            v6 = Common.MultMatrix(mtx, v6);
            v7 = Common.MultMatrix(mtx, v7);
            v8 = Common.MultMatrix(mtx, v8);

            AddLine(v1, v2);
            AddLine(v2, v3);
            AddLine(v3, v4);
            AddLine(v4, v1);

            AddLine(v5, v6);
            AddLine(v6, v7);
            AddLine(v7, v8);
            AddLine(v8, v5);

            AddLine(v1, v5);
            AddLine(v2, v6);
            AddLine(v3, v7);
            AddLine(v4, v8);
        }

        public void DrawSphere(IndexedVector3 p, float radius, IndexedVector3 color){
            throw new NotImplementedException();
        }

        public void DrawSphere(ref IndexedVector3 p, float radius, ref IndexedVector3 color){
            throw new NotImplementedException();
        }

        public void DrawSphere(float radius, ref IndexedMatrix transform, ref IndexedVector3 color){
            //throw new NotImplementedException();
        }

        public void DrawTriangle(ref IndexedVector3 v0, ref IndexedVector3 v1, ref IndexedVector3 v2, ref IndexedVector3 n0, ref IndexedVector3 n1,
            ref IndexedVector3 n2, ref IndexedVector3 color, float alpha){
            throw new NotImplementedException();
        }

        public void DrawTriangle(ref IndexedVector3 v0, ref IndexedVector3 v1, ref IndexedVector3 v2, ref IndexedVector3 color, float alpha){
            throw new NotImplementedException();
        }

        public void DrawContactPoint(IndexedVector3 pointOnB, IndexedVector3 normalOnB, float distance, int lifeTime, IndexedVector3 color){
            var pt2 = normalOnB*10 + pointOnB;

            DrawLine(ref pointOnB, ref pt2, ref color);
        }

        public void DrawContactPoint(ref IndexedVector3 pointOnB, ref IndexedVector3 normalOnB, float distance, int lifeTime, ref IndexedVector3 color){
            throw new NotImplementedException();
        }

        public void ReportErrorWarning(string warningString){
            throw new NotImplementedException();
        }

        public void Draw3dText(ref IndexedVector3 location, string textString){
            throw new NotImplementedException();
        }

        public void SetDebugMode(DebugDrawModes debugMode){
            throw new NotImplementedException();
        }

        public DebugDrawModes GetDebugMode(){
            return DebugDrawModes.DBG_DrawAabb | DebugDrawModes.DBG_DrawWireframe | DebugDrawModes.DBG_DrawContactPoints;
        }

        public void DrawAabb(IndexedVector3 @from, IndexedVector3 to, IndexedVector3 color){
            DrawAabb(ref @from, ref to, ref color);
        }

        public void DrawAabb(ref IndexedVector3 @from, ref IndexedVector3 to, ref IndexedVector3 color){
            var v1 = @from;
            var v2 = to;
#if !NO_REFRESH
            AddLine(new Vector3(v1.X, v1.Y, v1.Z), new Vector3(v1.X, v1.Y, v2.Z));
            AddLine(new Vector3(v1.X, v1.Y, v2.Z), new Vector3(v2.X, v1.Y, v2.Z));
            AddLine(new Vector3(v2.X, v1.Y, v2.Z), new Vector3(v2.X, v1.Y, v1.Z));
            AddLine(new Vector3(v2.X, v1.Y, v1.Z), new Vector3(v1.X, v1.Y, v1.Z));

            AddLine(new Vector3(v1.X, v2.Y, v1.Z), new Vector3(v1.X, v2.Y, v2.Z));
            AddLine(new Vector3(v1.X, v2.Y, v2.Z), new Vector3(v2.X, v2.Y, v2.Z));
            AddLine(new Vector3(v2.X, v2.Y, v2.Z), new Vector3(v2.X, v2.Y, v1.Z));
            AddLine(new Vector3(v2.X, v2.Y, v1.Z), new Vector3(v1.X, v2.Y, v1.Z));

            AddLine(new Vector3(v1.X, v1.Y, v1.Z), new Vector3(v1.X, v2.Y, v1.Z));
            AddLine(new Vector3(v1.X, v1.Y, v2.Z), new Vector3(v1.X, v2.Y, v2.Z));
            AddLine(new Vector3(v2.X, v1.Y, v2.Z), new Vector3(v2.X, v2.Y, v2.Z));
            AddLine(new Vector3(v2.X, v1.Y, v1.Z), new Vector3(v2.X, v2.Y, v1.Z));
#endif
        }

        public void DrawTransform(ref IndexedMatrix transform, float orthoLen){
        }

        public void DrawArc(ref IndexedVector3 center, ref IndexedVector3 normal, ref IndexedVector3 axis, float radiusA, float radiusB, float minAngle,
            float maxAngle, ref IndexedVector3 color, bool drawSect){
            throw new NotImplementedException();
        }

        public void DrawArc(ref IndexedVector3 center, ref IndexedVector3 normal, ref IndexedVector3 axis, float radiusA, float radiusB, float minAngle,
            float maxAngle, ref IndexedVector3 color, bool drawSect, float stepDegrees){
            throw new NotImplementedException();
        }

        public void DrawSpherePatch(ref IndexedVector3 center, ref IndexedVector3 up, ref IndexedVector3 axis, float radius, float minTh, float maxTh,
            float minPs, float maxPs, ref IndexedVector3 color){
            throw new NotImplementedException();
        }

        public void DrawSpherePatch(ref IndexedVector3 center, ref IndexedVector3 up, ref IndexedVector3 axis, float radius, float minTh, float maxTh,
            float minPs, float maxPs, ref IndexedVector3 color, float stepDegrees){
            throw new NotImplementedException();
        }

        public void DrawCapsule(float radius, float halfHeight, int upAxis, ref IndexedMatrix transform, ref IndexedVector3 color){
            throw new NotImplementedException();
        }

        public void DrawCylinder(float radius, float halfHeight, int upAxis, ref IndexedMatrix transform, ref IndexedVector3 color){
            throw new NotImplementedException();
        }

        public void DrawCone(float radius, float height, int upAxis, ref IndexedMatrix transform, ref IndexedVector3 color){
            throw new NotImplementedException();
        }

        public void DrawPlane(ref IndexedVector3 planeNormal, float planeConst, ref IndexedMatrix transform, ref IndexedVector3 color){
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable Members

        public void Dispose(){
            _lineBuffer.Dispose();
        }

        #endregion

        public void Clear(){
            for (int i = 0; i < _numLines; i++){
                _lines[i].Position = Vector3.Zero;
            }
            UpdateBuffers();
            _numLines = 0;
        }

        public void UpdateBuffers(){
            _lineBuffer.SetVertexBufferData(_lines);
        }

        public void DrawLineImmediate(Vector3 @from, Vector3 to){
            AddLine(@from, to);
            _lineBuffer.SetVertexBufferData(_lines);
        }

        void AddLine(Vector3 p1, Vector3 p2){
            _lines[_numLines] = new VertexPositionTexture(p1, new Vector2());
            _lines[_numLines + 1] = new VertexPositionTexture(p2, new Vector2());
            _numLines += 2;
        }
    }
}