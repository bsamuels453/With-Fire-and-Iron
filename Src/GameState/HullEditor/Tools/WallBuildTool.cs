using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gondola.Draw;
using Gondola.GameState.ObjectEditor;
using Gondola.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gondola.GameState.HullEditor.Tools {
    internal class WallBuildTool : DeckPlacementBase {
        readonly ObjectBuffer<WallSegmentIdentifier> _tempWallBuffer;
        readonly List<WallSegmentIdentifier> _tempWallIdentifiers;
        readonly float _wallHeight;

        public WallBuildTool(HullDataManager hullData) :
            base(hullData, hullData.WallResolution) {
            _tempWallBuffer = new ObjectBuffer<WallSegmentIdentifier>(
            hullData.DeckVertexes[0].Count() * 2,
            10,
            20,
            30,
            "Shader_AirshipWalls") { UpdateBufferManually = true };

            _tempWallIdentifiers = new List<WallSegmentIdentifier>();
            _wallHeight = hullData.DeckHeight - 0.01f;
        }

        protected override void HandleCursorChange(bool isDrawing) {
            if (isDrawing)
                GenerateWallsFromStroke();
        }

        protected override void HandleCursorRelease() {
            HullData.CurWallIdentifiers.AddRange(
                from id in _tempWallIdentifiers
                where !HullData.CurWallIdentifiers.Contains(id)
                select id
                );
            _tempWallIdentifiers.Clear();
            HullData.CurWallBuffer.AbsorbBuffer(_tempWallBuffer);
        }

        protected override void HandleCursorDown() {
        }

        protected override void OnCurDeckChange() {
        }

        protected override void OnEnable() {
            _tempWallBuffer.Enabled = true;
        }

        protected override void OnDisable() {
            _tempWallBuffer.Enabled = false;
        }

        void GenerateWallsFromStroke() {
            _tempWallIdentifiers.Clear();
            int strokeW = (int)((StrokeEnd.Z - StrokeOrigin.Z) / GridResolution);
            int strokeH = (int)((StrokeEnd.X - StrokeOrigin.X) / GridResolution);

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
            for (int i = 0; i < Math.Abs(strokeW); i++) {
                int[] indicies;
                VertexPositionNormalTexture[] verticies;
                var origin = new Vector3(StrokeOrigin.X, StrokeOrigin.Y, StrokeOrigin.Z + GridResolution * i * wDir);
                MeshHelper.GenerateCube(out verticies, out indicies, origin, wallWidth, _wallHeight, GridResolution * wDir);
                var identifier = new WallSegmentIdentifier(origin, new Vector3(origin.X, origin.Y, origin.Z + GridResolution * wDir));
                _tempWallBuffer.AddObject(identifier, indicies, verticies);
                _tempWallIdentifiers.Add(identifier);
            }
            for (int i = 0; i < Math.Abs(strokeW); i++) {
                int[] indicies;
                VertexPositionNormalTexture[] verticies;
                var origin = new Vector3(StrokeEnd.X, StrokeOrigin.Y, StrokeOrigin.Z + GridResolution * i * wDir);
                MeshHelper.GenerateCube(out verticies, out indicies, origin, wallWidth, _wallHeight, GridResolution * wDir);
                var identifier = new WallSegmentIdentifier(origin, new Vector3(origin.X, origin.Y, origin.Z + GridResolution * wDir));
                _tempWallBuffer.AddObject(identifier, indicies, verticies);
                _tempWallIdentifiers.Add(identifier);
            }
            //generate height walls
            for (int i = 0; i < Math.Abs(strokeH); i++) {
                int[] indicies;
                VertexPositionNormalTexture[] verticies;
                var origin = new Vector3(StrokeOrigin.X + GridResolution * i * hDir, StrokeOrigin.Y, StrokeOrigin.Z);
                MeshHelper.GenerateCube(out verticies, out indicies, origin, GridResolution * hDir, _wallHeight, wallWidth);
                var identifier = new WallSegmentIdentifier(origin, new Vector3(origin.X + GridResolution * hDir, origin.Y, origin.Z));
                _tempWallBuffer.AddObject(identifier, indicies, verticies);
                _tempWallIdentifiers.Add(identifier);
            }
            for (int i = 0; i < Math.Abs(strokeH); i++) {
                int[] indicies;
                VertexPositionNormalTexture[] verticies;
                var origin = new Vector3(StrokeOrigin.X + GridResolution * i * hDir, StrokeOrigin.Y, StrokeEnd.Z);
                MeshHelper.GenerateCube(out verticies, out indicies, origin, GridResolution * hDir, _wallHeight, wallWidth);
                var identifier = new WallSegmentIdentifier(origin, new Vector3(origin.X + GridResolution * hDir, origin.Y, origin.Z));
                _tempWallBuffer.AddObject(identifier, indicies, verticies);
                _tempWallIdentifiers.Add(identifier);
            }

            _tempWallBuffer.UpdateBuffers();
        }
    }

    #region wallidentifier

    internal struct WallSegmentIdentifier : IEquatable<WallSegmentIdentifier> {
        public readonly Vector3 RefPoint1;
        public readonly Vector3 RefPoint2;

        public WallSegmentIdentifier(Vector3 refPoint2, Vector3 refPoint1) {
            RefPoint2 = refPoint2;
            RefPoint1 = refPoint1;
        }

        #region equality operators

        public bool Equals(WallSegmentIdentifier other) {
            if (RefPoint2 == other.RefPoint2 && RefPoint1 == other.RefPoint1)
                return true;
            if (RefPoint2 == other.RefPoint1 && other.RefPoint2 == RefPoint1)
                return true;
            return false;
        }

        public static bool operator ==(WallSegmentIdentifier wallid1, WallSegmentIdentifier wallid2) {
            if (wallid1.RefPoint2 == wallid2.RefPoint2 && wallid1.RefPoint1 == wallid2.RefPoint1)
                return true;
            if (wallid1.RefPoint2 == wallid2.RefPoint1 && wallid2.RefPoint1 == wallid1.RefPoint2)
                return true;
            return false;
        }

        public static bool operator !=(WallSegmentIdentifier wallid1, WallSegmentIdentifier wallid2) {
            if (wallid1.RefPoint2 == wallid2.RefPoint2 && wallid1.RefPoint1 == wallid2.RefPoint1)
                return false;
            if (wallid1.RefPoint2 == wallid2.RefPoint1 && wallid2.RefPoint1 == wallid1.RefPoint2)
                return false;
            return true;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(WallSegmentIdentifier)) return false;
            return Equals((WallSegmentIdentifier)obj);
        }

        #endregion

        public override int GetHashCode() {
            unchecked {
                // ReSharper disable NonReadonlyFieldInGetHashCode
                return (RefPoint2.GetHashCode() * 397) ^ RefPoint1.GetHashCode();
                // ReSharper restore NonReadonlyFieldInGetHashCode
            }
        }
    }

    #endregion
}
