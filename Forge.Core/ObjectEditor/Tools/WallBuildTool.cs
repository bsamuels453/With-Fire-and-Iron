#region

using System;
using System.Collections.Generic;
using System.Linq;
using Forge.Core.Util;
using Forge.Framework.Draw;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;

#endregion

namespace Forge.Core.ObjectEditor.Tools{
    public class WallBuildTool : DeckPlacementBase{
        readonly ObjectBuffer<WallSegmentIdentifier> _tempWallBuffer;
        readonly List<WallSegmentIdentifier> _tempWallIdentifiers;
        readonly float _wallHeight;

        public WallBuildTool(HullEnvironment hullData) :
            base(hullData){
            _tempWallBuffer = new ObjectBuffer<WallSegmentIdentifier>
                (
                hullData.DeckSectionContainer.DeckVertexesByDeck[0].Count()*2,
                10,
                20,
                30,
                "Config/Shaders/Airship_InternalWalls.config"){UpdateBufferManually = true};

            _tempWallIdentifiers = new List<WallSegmentIdentifier>();
            _wallHeight = hullData.DeckHeight - 0.01f;
        }

        protected override void HandleCursorChange(bool isDrawing){
            if (isDrawing)
                GenerateWallsFromStroke();
        }

        protected override void HandleCursorRelease(){
            HullData.CurWallIdentifiers.AddRange
                (
                    from id in _tempWallIdentifiers
                    where !HullData.CurWallIdentifiers.Contains(id)
                    select id
                );
            _tempWallIdentifiers.Clear();
            HullData.CurWallBuffer.AbsorbBuffer(_tempWallBuffer);
        }

        protected override void HandleCursorDown(){
        }

        protected override void OnCurDeckChange(){
        }

        protected override void OnEnable(){
            _tempWallBuffer.Enabled = true;
        }

        protected override void OnDisable(){
            _tempWallBuffer.Enabled = false;
        }

        protected override void DisposeChild(){
            _tempWallBuffer.Dispose();
        }

        void GenerateWallsFromStroke(){
            _tempWallIdentifiers.Clear();
            int strokeW = (int) ((StrokeEnd.Z - StrokeOrigin.Z)/GridResolution);
            int strokeH = (int) ((StrokeEnd.X - StrokeOrigin.X)/GridResolution);

            _tempWallBuffer.ClearObjects();
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

            //generate width walls
            const float wallWidth = 0.1f;
            for (int i = 0; i < Math.Abs(strokeW); i++){
                int[] indicies;
                VertexPositionNormalTexture[] verticies;
                var origin = new Vector3(StrokeOrigin.X, StrokeOrigin.Y, StrokeOrigin.Z + GridResolution*i*wDir);
                MeshHelper.GenerateCube(out verticies, out indicies, origin, wallWidth, _wallHeight, GridResolution*wDir);
                var identifier = new WallSegmentIdentifier(origin, new Vector3(origin.X, origin.Y, origin.Z + GridResolution*wDir));
                _tempWallBuffer.AddObject(identifier, indicies, verticies);
                _tempWallIdentifiers.Add(identifier);
            }
            for (int i = 0; i < Math.Abs(strokeW); i++){
                int[] indicies;
                VertexPositionNormalTexture[] verticies;
                var origin = new Vector3(StrokeEnd.X, StrokeOrigin.Y, StrokeOrigin.Z + GridResolution*i*wDir);
                MeshHelper.GenerateCube(out verticies, out indicies, origin, wallWidth, _wallHeight, GridResolution*wDir);
                var identifier = new WallSegmentIdentifier(origin, new Vector3(origin.X, origin.Y, origin.Z + GridResolution*wDir));
                _tempWallBuffer.AddObject(identifier, indicies, verticies);
                _tempWallIdentifiers.Add(identifier);
            }
            //generate height walls
            for (int i = 0; i < Math.Abs(strokeH); i++){
                int[] indicies;
                VertexPositionNormalTexture[] verticies;
                var origin = new Vector3(StrokeOrigin.X + GridResolution*i*hDir, StrokeOrigin.Y, StrokeOrigin.Z);
                MeshHelper.GenerateCube(out verticies, out indicies, origin, GridResolution*hDir, _wallHeight, wallWidth);
                var identifier = new WallSegmentIdentifier(origin, new Vector3(origin.X + GridResolution*hDir, origin.Y, origin.Z));
                _tempWallBuffer.AddObject(identifier, indicies, verticies);
                _tempWallIdentifiers.Add(identifier);
            }
            for (int i = 0; i < Math.Abs(strokeH); i++){
                int[] indicies;
                VertexPositionNormalTexture[] verticies;
                var origin = new Vector3(StrokeOrigin.X + GridResolution*i*hDir, StrokeOrigin.Y, StrokeEnd.Z);
                MeshHelper.GenerateCube(out verticies, out indicies, origin, GridResolution*hDir, _wallHeight, wallWidth);
                var identifier = new WallSegmentIdentifier(origin, new Vector3(origin.X + GridResolution*hDir, origin.Y, origin.Z));
                _tempWallBuffer.AddObject(identifier, indicies, verticies);
                _tempWallIdentifiers.Add(identifier);
            }

            _tempWallBuffer.UpdateBuffers();
        }
    }
}