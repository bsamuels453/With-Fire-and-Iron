#region

using System.Collections.Generic;
using System.Diagnostics;
using Forge.Framework.Draw;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BoundingBox = MonoGameUtility.BoundingBox;
using Matrix = MonoGameUtility.Matrix;
using Vector3 = MonoGameUtility.Vector3;

#endregion

namespace Forge.Core.ObjectEditor.Tools{
    /// <summary>
    /// Tool for placing generic objects on the airship's deck.
    /// </summary>
    internal class DeckObjectPlacementTool : DeckPlacementBase{
        readonly ObjectModelBuffer<int> _ghostedObjectModel;
        readonly HullDataManager _hullData;
        readonly float _objectGridLength;
        readonly float _objectGridWidth;
        readonly string _objectModelName;

        public DeckObjectPlacementTool(HullDataManager hullData, string objectModel, float objectGridWidth, float objectGridLength) :
            base(hullData){
            _objectGridWidth = objectGridWidth;
            _objectGridLength = objectGridLength;
            _objectModelName = objectModel;
            _hullData = hullData;

            _ghostedObjectModel = new ObjectModelBuffer<int>(1, "Config/Shaders/TintedModel.config");
            _ghostedObjectModel.AddObject(0, Resource.LoadContent<Model>(_objectModelName), Matrix.Identity);
            _ghostedObjectModel.Enabled = false;
        }

        protected override void EnableCursorGhost(){
            _ghostedObjectModel.Enabled = true;
            _ghostedObjectModel.ShaderParams["f4_TintColor"].SetValue(Color.Green.ToVector4());
        }

        protected override void DisableCursorGhost(DisableReason reason){
            switch (reason){
                case DisableReason.CursorNotValid:
                    _ghostedObjectModel.ShaderParams["f4_TintColor"].SetValue(Color.DarkRed.ToVector4());
                    break;
                case DisableReason.NoBoundingBoxInterception:
                    _ghostedObjectModel.Enabled = false;
                    break;
            }
        }

        protected override void UpdateCursorGhost(){
            _ghostedObjectModel.TransformAll(base.CursorPosition);
        }

        protected override void HandleCursorChange(bool isDrawing){
        }

        protected override void HandleCursorRelease(){
            var identifier = new ObjectIdentifier();

            //Matrix trans = Matrix.CreateRotationX((float)-Math.PI / 2) * Matrix.CreateRotationY((float)-Math.PI / 2) * Matrix.CreateTranslation(CursorPosition);
            Matrix trans = Matrix.Identity*Matrix.CreateTranslation(CursorPosition);
            _hullData.CurObjBuffer.AddObject(identifier, Resource.LoadContent<Model>(_objectModelName), trans);

            var quadsToHide = new List<ObjectIdentifier>();
            var upperBoxesToHide = new List<BoundingBox>();
            var lowerBoxesToHide = new List<BoundingBox>();
            for (float x = 0; x < _objectGridWidth; x += GridResolution){
                for (float z = 0; z < _objectGridLength; z += GridResolution){
                    var min = CursorPosition + new Vector3(x, _hullData.DeckHeight, z);
                    quadsToHide.Add(new ObjectIdentifier());
                    upperBoxesToHide.Add(new BoundingBox(min, min + new Vector3(GridResolution, 0, GridResolution)));
                    lowerBoxesToHide.Add
                        (new BoundingBox(CursorPosition + new Vector3(x, 0, z), CursorPosition + new Vector3(x + GridResolution, 0, z + GridResolution)));
                }
            }
            if (HullData.CurDeck != 0){
                foreach (var quad in quadsToHide){
                    /*
                    bool b = _hullData.DeckSectionContainer.DeckBufferByDeck[_hullData.CurDeck - 1].DisableObject(quad);
                    Debug.Assert(b);
                     */
                }
                foreach (var bbox in upperBoxesToHide){
                    _hullData.DeckSectionContainer.BoundingBoxesByDeck[_hullData.CurDeck - 1].Remove(bbox);
                }
            }
            foreach (var bbox in lowerBoxesToHide){
                _hullData.DeckSectionContainer.BoundingBoxesByDeck[_hullData.CurDeck].Remove(bbox);
            }
            GenerateGuideGrid();
        }

        protected override void HandleCursorDown(){
        }

        protected override void OnCurDeckChange(){
        }

        protected override void OnEnable(){
            _ghostedObjectModel.Enabled = true;
        }

        protected override void OnDisable(){
            _ghostedObjectModel.Enabled = false;
        }

        protected override void DisposeChild(){
            _ghostedObjectModel.Dispose();
        }

        protected override bool IsCursorValid(Vector3 newCursorPos, Vector3 prevCursorPosition, List<Vector3> deckFloorVertexes, float distToPt){
            bool validCursor = true;
            for (float x = 0; x <= _objectGridWidth; x += GridResolution){
                for (float z = 0; z <= _objectGridLength; z += GridResolution){
                    var vert = newCursorPos + new Vector3(x, 0, z);
                    if (!deckFloorVertexes.Contains(vert)){
                        validCursor = false;
                        break;
                    }
                }
            }
            return validCursor;
        }
    }
}