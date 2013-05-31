using System.Collections.Generic;
using System.Diagnostics;
using Forge.Framework;
using Forge.Framework.Draw;
using Forge.Core.Logic;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BoundingBox = MonoGameUtility.BoundingBox;
using Matrix = MonoGameUtility.Matrix;
using Vector3 = MonoGameUtility.Vector3;

namespace Forge.Core.ObjectEditor.Tools {
    internal class LadderBuildTool : DeckPlacementBase {
        const float _ladderWidth = 1f;
        const float _gridWidth = 0.5f;

        readonly HullDataManager _hullData;
        readonly ObjectModelBuffer<int> _ghostedLadderModel;

        public LadderBuildTool(HullDataManager hullData)
            : base(hullData, hullData.WallResolution, 2) {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            Debug.Assert(_ladderWidth % _gridWidth == 0);
            // ReSharper restore CompareOfFloatsByEqualityOperator
            _hullData = hullData;

            _ghostedLadderModel = new ObjectModelBuffer<int>(1, "Shader_TintedModel");
            //Matrix trans = Matrix.CreateRotationX((float)-Math.PI / 2) * Matrix.CreateRotationY((float)-Math.PI / 2);
            var trans = Matrix.Identity;

            _ghostedLadderModel.AddObject(0, Resource.LoadContent<Model>("Models/Ladder"), trans);
            _ghostedLadderModel.DisableObject(0);
        }

        protected override void EnableCursorGhost() {
            
            _ghostedLadderModel.ShaderParams["TintColor"].SetValue(Color.Green.ToVector4());
        }

        protected override void DisableCursorGhost() {
            _ghostedLadderModel.ShaderParams["TintColor"].SetValue(Color.DarkRed.ToVector4());
        }

        protected override void UpdateCursorGhost() {
            _ghostedLadderModel.TransformAll(base.CursorPosition);
        }

        protected override void HandleCursorChange(bool isDrawing) {
        }

        protected override void HandleCursorRelease() {
            
            //_hullData.TopExposedHullLayer.ForEach(item => item.DisablePanel(CursorPosition.X, CursorPosition.Z, 0));
            //_hullData.TopExposedHullLayer.ForEach(item => item.DisablePanel(CursorPosition.X, CursorPosition.Z, 1));
            //_hullData.TopExposedHullLayer.ForEach(item => item.DisablePanel(CursorPosition.X, CursorPosition.Z, 2));
            //_hullData.TopExposedHullLayer.ForEach(item => item.DisablePanel(CursorPosition.X, CursorPosition.Z, 3));
            //_hullData.TopExposedHullLayer.ForEach(item => item.DisablePanel(CursorPosition.X, CursorPosition.Z, 4)); 

            //_hullData.TopExposedHullLayer.ForEach(item => item.DisablePanel(1, 2, 1));
            //_hullData.TopExposedHullLayer[0].Cut(CursorPosition, CursorPosition + new Vector3(0.5f, 0, 0));

            var identifier = new AirshipObjectIdentifier(ObjectType.Ladder, CursorPosition);

            //Matrix trans = Matrix.CreateRotationX((float)-Math.PI / 2) * Matrix.CreateRotationY((float)-Math.PI / 2) * Matrix.CreateTranslation(CursorPosition);
            Matrix trans = Matrix.Identity * Matrix.CreateTranslation(CursorPosition);
            _hullData.CurObjBuffer.AddObject(identifier, Resource.LoadContent<Model>("Models/Ladder"), trans);

            var quadsToHide = new List<AirshipObjectIdentifier>();
            var upperBoxesToHide = new List<BoundingBox>();
            var lowerBoxesToHide = new List<BoundingBox>();
            for (float x = 0; x < _ladderWidth; x += _gridWidth) {
                for (float z = 0; z < _ladderWidth; z += _gridWidth) {
                    var min = CursorPosition + new Vector3(x, _hullData.DeckHeight, z);
                    quadsToHide.Add(new AirshipObjectIdentifier(ObjectType.Deckboard, min));
                    upperBoxesToHide.Add(new BoundingBox(min, min + new Vector3(_gridWidth, 0, _gridWidth)));
                    lowerBoxesToHide.Add(new BoundingBox(CursorPosition + new Vector3(x, 0, z), CursorPosition + new Vector3(x + _gridWidth, 0, z + _gridWidth)));
                }
            }
            if (HullData.CurDeck != 0) {
                foreach (var quad in quadsToHide) {
                    bool b = _hullData.DeckSectionContainer.DeckBufferByDeck[_hullData.CurDeck - 1].DisableObject(quad);
                    Debug.Assert(b);
                }
                foreach (var bbox in upperBoxesToHide) {
                    _hullData.DeckSectionContainer.BoundingBoxesByDeck[_hullData.CurDeck - 1].Remove(bbox);
                }
            }
            foreach (var bbox in lowerBoxesToHide) {
                _hullData.DeckSectionContainer.BoundingBoxesByDeck[_hullData.CurDeck].Remove(bbox);
            }
            GenerateGuideGrid();
        }

        protected override void HandleCursorDown() {

        }

        protected override void OnCurDeckChange() {
        }

        protected override void OnEnable() {
            _ghostedLadderModel.EnableObject(0);
        }

        protected override void OnDisable() {
            _ghostedLadderModel.DisableObject(0);
        }

        protected override void DisposeChild(){
            _ghostedLadderModel.Dispose();
        }
    }
}
