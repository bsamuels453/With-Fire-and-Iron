#region

using System.Collections.Generic;
using Forge.Framework.Draw;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;
using Matrix = MonoGameUtility.Matrix;
using Point = MonoGameUtility.Point;
using Vector3 = MonoGameUtility.Vector3;

#endregion

namespace Forge.Core.ObjectEditor.Tools{
    /// <summary>
    /// Tool for placing generic objects on the airship's deck.
    /// </summary>
    internal class DeckObjectPlacementTool : DeckPlacementBase{
        readonly DeckObjectEnvironment _deckObjectEnvironment;
        readonly ObjectModelBuffer<int> _ghostedObjectModel;
        readonly HullEnvironment _hullData;
        readonly XZPoint _objectGridDims;
        readonly string _objectModelName;

        public DeckObjectPlacementTool(HullEnvironment hullData, DeckObjectEnvironment deckObjectEnvironment, string objectModel, XZPoint objectGridDims) :
            base(hullData){
            _objectGridDims = objectGridDims;
            _objectModelName = objectModel;
            _hullData = hullData;
            _deckObjectEnvironment = deckObjectEnvironment;

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
            _deckObjectEnvironment.AddObject
                (
                    _objectModelName,
                    CursorPosition,
                    _objectGridDims,
                    _hullData.CurDeck,
                    DeckObjectEnvironment.SideEffect.CutsIntoCeiling
                );
            //This used to be used to regenerate grid to remove grid lines obfuscated/covered by objects.
            //For now, the deckobjectenvironment doesn't mess with the deck's bounding boxes because it
            //would create ridic coupling issues. Leaving this here though as a footnote to keep in mind
            //a clean way to do this in the future.
            //GenerateGuideGrid(); 
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
            return _deckObjectEnvironment.IsObjectPlacementValid(newCursorPos, _objectGridDims, _hullData.CurDeck, true);
        }
    }
}