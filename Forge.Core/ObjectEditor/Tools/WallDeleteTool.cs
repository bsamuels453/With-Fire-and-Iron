#region

using System;
using System.Collections.Generic;
using Forge.Core.Util;
using Forge.Framework.Draw;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;

#endregion

namespace Forge.Core.ObjectEditor.Tools{
    public class WallDeleteTool : DeckPlacementBase{
        readonly ObjectBuffer<WallSegmentIdentifier> _tempWallBuffer;
        readonly InternalWallEnvironment _wallEnv;
        List<WallSegmentIdentifier> _prevIdentifiers;

        public WallDeleteTool(HullEnvironment hullData, InternalWallEnvironment wallEnv) :
            base(hullData){
            _tempWallBuffer = new ObjectBuffer<WallSegmentIdentifier>
                (5000, 10, 20, 30, "Config/Shaders/GroundDecal.config"){UpdateBufferManually = true};
            _prevIdentifiers = new List<WallSegmentIdentifier>();
            _wallEnv = wallEnv;
        }

        protected override void HandleCursorChange(bool isDrawing){
            if (!isDrawing)
                return;
            _tempWallBuffer.ClearObjects();
            int strokeW = (int) ((StrokeEnd.Z - StrokeOrigin.Z)/GridResolution);
            int strokeH = (int) ((StrokeEnd.X - StrokeOrigin.X)/GridResolution);

            int wDir;
            int hDir;
            if (strokeW > 0)
                wDir = 1;
            else
                wDir = -1;
            if (strokeH > 0)
                hDir = 1;
            else
                hDir = -1;

            var identifiers = new List<WallSegmentIdentifier>();
            //generate width walls
            const float wallWidth = 0.1f;
            const float height = 0.01f;
            for (int i = 0; i < Math.Abs(strokeW); i++){
                int[] indicies;
                VertexPositionNormalTexture[] verticies;
                var origin = new Vector3(StrokeOrigin.X, StrokeOrigin.Y, StrokeOrigin.Z + GridResolution*i*wDir);
                MeshHelper.GenerateCube(out verticies, out indicies, origin, wallWidth, height, GridResolution*wDir);
                var identifier = new WallSegmentIdentifier(origin, new Vector3(origin.X, origin.Y, origin.Z + GridResolution*wDir));
                _tempWallBuffer.AddObject(identifier, indicies, verticies);
                identifiers.Add(identifier);
            }
            for (int i = 0; i < Math.Abs(strokeW); i++){
                int[] indicies;
                VertexPositionNormalTexture[] verticies;
                var origin = new Vector3(StrokeEnd.X, StrokeOrigin.Y, StrokeOrigin.Z + GridResolution*i*wDir);
                MeshHelper.GenerateCube(out verticies, out indicies, origin, wallWidth, height, GridResolution*wDir);
                var identifier = new WallSegmentIdentifier(origin, new Vector3(origin.X, origin.Y, origin.Z + GridResolution*wDir));
                _tempWallBuffer.AddObject(identifier, indicies, verticies);
                identifiers.Add(identifier);
            }
            //generate height walls
            for (int i = 0; i < Math.Abs(strokeH); i++){
                int[] indicies;
                VertexPositionNormalTexture[] verticies;
                var origin = new Vector3(StrokeOrigin.X + GridResolution*i*hDir, StrokeOrigin.Y, StrokeOrigin.Z);
                MeshHelper.GenerateCube(out verticies, out indicies, origin, GridResolution*hDir, height, wallWidth);
                var identifier = new WallSegmentIdentifier(origin, new Vector3(origin.X + GridResolution*hDir, origin.Y, origin.Z));
                _tempWallBuffer.AddObject(identifier, indicies, verticies);
                identifiers.Add(identifier);
            }
            for (int i = 0; i < Math.Abs(strokeH); i++){
                int[] indicies;
                VertexPositionNormalTexture[] verticies;
                var origin = new Vector3(StrokeOrigin.X + GridResolution*i*hDir, StrokeOrigin.Y, StrokeEnd.Z);
                MeshHelper.GenerateCube(out verticies, out indicies, origin, GridResolution*hDir, height, wallWidth);
                var identifier = new WallSegmentIdentifier(origin, new Vector3(origin.X + GridResolution*hDir, origin.Y, origin.Z));
                _tempWallBuffer.AddObject(identifier, indicies, verticies);
                identifiers.Add(identifier);
            }

            _tempWallBuffer.UpdateBuffers();

            foreach (var identifier in _prevIdentifiers){
                _wallEnv.EnableSegment(identifier);
            }
            foreach (var identifier in identifiers){
                _wallEnv.DisableSegment(identifier);
            }

            _prevIdentifiers = identifiers;
        }

        protected override void HandleCursorRelease(){
            foreach (var identifier in _prevIdentifiers){
                _wallEnv.RemoveSegment(identifier);
            }

            _tempWallBuffer.ClearObjects();
            _prevIdentifiers.Clear();
        }

        protected override void HandleCursorDown(){
        }

        protected override void OnCurDeckChange(){
            _prevIdentifiers.Clear();
        }

        protected override void OnEnable(){
            _tempWallBuffer.Enabled = true;
        }

        protected override void OnDisable(){
            _tempWallBuffer.Enabled = false;
            _prevIdentifiers.Clear();
        }

        protected override void DisposeChild(){
            _tempWallBuffer.Dispose();
        }
    }
}