#region

using System.Collections.Generic;
using Forge.Core.Util;
using Forge.Framework.Draw;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;
using Vector3 = MonoGameUtility.Vector3;

#endregion

namespace Forge.Core.ObjectEditor.Tools{
    internal class ZoningTool : DeckPlacementBase{
        readonly GeometryBuffer<VertexPositionNormalTexture> _footprint;
        readonly GameObjectEnvironment _gameObjectEnv;
        readonly IZoneFiller _zoneFiller;
        int _curDeck;

        public ZoningTool(
            HullEnvironment hullData,
            GameObjectEnvironment gameObjectEnvironment,
            IZoneFiller zoneFiller
            ) : base(hullData){
            _footprint = new GeometryBuffer<VertexPositionNormalTexture>(30, 20, 10, "Config/Shaders/ObjectPlacementFootprint.config");
            _footprint.ShaderParams["f4_TintColor"].SetValue(Color.Green.ToVector4());
            _gameObjectEnv = gameObjectEnvironment;
            _zoneFiller = zoneFiller;
        }

        protected override void HandleCursorChange(bool isDrawing){
            _footprint.Enabled = true;
            //place zone
            Vector3 strokeOrigin;
            float strokeX, strokeZ;
            CalculateStrokeArea(out strokeOrigin, out strokeX, out strokeZ);

            var footprintOrigin = strokeOrigin;
            VertexPositionNormalTexture[] verts;
            int[] inds;
            MeshHelper.GenerateCube(out verts, out inds, footprintOrigin, strokeX, 0.01f, strokeZ);
            _footprint.SetIndexBufferData(inds);
            _footprint.SetVertexBufferData(verts);
        }


        void CalculateStrokeArea(out Vector3 strokeOrigin, out float strokeX, out float strokeZ){
            strokeZ = ((StrokeEnd.Z - StrokeOrigin.Z));
            strokeX = ((StrokeEnd.X - StrokeOrigin.X));

            strokeOrigin = StrokeOrigin;
            if (StrokeEnd.X < strokeOrigin.X){
                strokeOrigin.X = StrokeEnd.X;
                strokeX *= -1;
            }
            if (StrokeEnd.Z < strokeOrigin.Z){
                strokeOrigin.Z = StrokeEnd.Z;
                strokeZ *= -1;
            }
        }

        protected override void HandleCursorRelease(){
            _footprint.Enabled = false;
            bool isValid = IsCursorValid(CursorPosition, Vector3.Zero, null, 0);
            if (isValid){
                var gameObjs = _zoneFiller.ExtractGeneratedObjects();
                _zoneFiller.Reset();

                foreach (var gameObj in gameObjs){
                    _gameObjectEnv.AddObject(gameObj.GameObject, gameObj.Model, gameObj.SideEffect);
                }
            }
        }

        protected override void HandleCursorDown(){
        }


        protected override void OnCurDeckChange(int newDeck){
            _curDeck = newDeck;
        }

        protected override void DisableCursorGhost(DisableReason reason){
            base.DisableCursorGhost(reason);

            switch (reason){
                case DisableReason.CursorNotValid:
                    _footprint.ShaderParams["f4_TintColor"].SetValue(Color.DarkRed.ToVector4());
                    _zoneFiller.TintColor = Color.DarkRed;
                    break;
                case DisableReason.NoBoundingBoxInterception:
                    _footprint.Enabled = false;
                    _zoneFiller.Enabled = false;
                    break;
            }
        }

        protected override void EnableCursorGhost(){
            base.EnableCursorGhost();
            _footprint.Enabled = true;
            _zoneFiller.Enabled = true;
            _footprint.ShaderParams["f4_TintColor"].SetValue(Color.Green.ToVector4());
            _zoneFiller.TintColor = Color.Green;
        }

        protected override void OnEnable(){
            _footprint.Enabled = true;
            _zoneFiller.Enabled = true;
        }

        protected override void OnDisable(){
            _footprint.Enabled = false;
            _zoneFiller.Enabled = false;
        }

        public override void Dispose(){
            _footprint.Dispose();
            _zoneFiller.Dispose();
            base.Dispose();
        }

        protected override bool IsCursorValid(Vector3 newCursorPos, Vector3 prevCursorPosition, List<Vector3> deckFloorVertexes, float distToPt){
            Vector3 strokeOrigin;
            float strokeX, strokeZ;
            CalculateStrokeArea(out strokeOrigin, out strokeX, out strokeZ);
            var gridDims = new XZPoint((int) (strokeX*2), (int) (strokeZ*2));

            return _gameObjectEnv.IsObjectPlacementValid(strokeOrigin, gridDims, _curDeck, 0, GameObjectType.Engine, 0, GameObjectEnvironment.SideEffect.None);
        }
    }
}