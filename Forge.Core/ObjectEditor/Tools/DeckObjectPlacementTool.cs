#region

using System.Collections.Generic;
using Forge.Core.Airship.Data;
using Forge.Core.GameObjects;
using Forge.Core.Util;
using Forge.Framework.Draw;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;
using Matrix = MonoGameUtility.Matrix;
using Vector3 = MonoGameUtility.Vector3;

#endregion

namespace Forge.Core.ObjectEditor.Tools{
    /// <summary>
    /// Tool for placing generic objects on the airship's deck.
    /// </summary>
    internal class DeckObjectPlacementTool : DeckPlacementBase{
        readonly GeometryBuffer<VertexPositionNormalTexture> _dimensionAccessFootprint;
        readonly GeometryBuffer<VertexPositionNormalTexture> _dimensionFootprint;
        readonly GameObjectEnvironment _gameObjectEnvironment;
        readonly GameObjectType _gameObjectType;
        readonly ObjectModelBuffer<int> _ghostedObjectModel;
        readonly HullEnvironment _hullData;
        readonly XZPoint _objectGridDims;
        readonly string _objectModelName;
        readonly string _objectParams;
        protected Vector3 CursorOffset;
        protected GameObjectEnvironment.SideEffect PlacementSideEffect;
        float _rotation;
        Matrix _transform;

        public DeckObjectPlacementTool(
            HullEnvironment hullData,
            GameObjectEnvironment gameObjectEnvironment,
            GameObjectType objectType,
            string objectParams) :
                base(hullData){
            _gameObjectType = objectType;
            _objectGridDims = objectType.Attribute<XZPoint>(GameObjectAttr.Dimensions);
            _objectModelName = objectType.Attribute<string>(GameObjectAttr.ModelName);
            _hullData = hullData;
            _gameObjectEnvironment = gameObjectEnvironment;
            PlacementSideEffect = objectType.Attribute<GameObjectEnvironment.SideEffect>(GameObjectAttr.SideEffect);
            _objectParams = objectParams;
            CursorOffset = CalculateCursorOffset(_objectGridDims);

            _ghostedObjectModel = new ObjectModelBuffer<int>(1, "Config/Shaders/TintedModel.config");
            _ghostedObjectModel.AddObject(0, Resource.LoadContent<Model>(_objectModelName), Matrix.Identity);
            _ghostedObjectModel.Enabled = false;

            _dimensionFootprint = new GeometryBuffer<VertexPositionNormalTexture>(6, 4, 2, "Config/Shaders/ObjectPlacementFootprint.config");
            _dimensionAccessFootprint = new GeometryBuffer<VertexPositionNormalTexture>(6, 4, 2, "Config/Shaders/ObjectPlacementAccess.config");


            VertexPositionNormalTexture[] dimensionVerts;
            int[] dimensionInds;
            MeshHelper.GenerateFlatQuad(out dimensionVerts, out dimensionInds, new Vector3(0, 0.025f, 0), _objectGridDims.X/2f, _objectGridDims.Z/2f);
            _dimensionFootprint.SetIndexBufferData(dimensionInds);
            _dimensionFootprint.SetVertexBufferData(dimensionVerts);
            _dimensionFootprint.Enabled = false;
            _dimensionFootprint.ShaderParams["f4_TintColor"].SetValue(Color.DarkRed.ToVector4());

            var interactionArea = _gameObjectType.Attribute<XZRectangle>(GameObjectAttr.InteractionArea);
            var interactionAreaOffst = new Vector3(interactionArea.X/2f, 0, interactionArea.Z/2f);
            var orientation = _gameObjectType.Attribute<Quadrant.Direction>(GameObjectAttr.InteractionOrientation);

            MeshHelper.GenerateFlatQuad
                (out dimensionVerts, out dimensionInds, interactionAreaOffst + new Vector3(0, 0.025f, 0), interactionArea.Width/2f, interactionArea.Length/2f);
            MeshHelper.GenerateRotatedQuadTexcoords(orientation, dimensionVerts);
            _dimensionAccessFootprint.SetIndexBufferData(dimensionInds);
            _dimensionAccessFootprint.SetVertexBufferData(dimensionVerts);
            _dimensionAccessFootprint.Enabled = false;
            _dimensionAccessFootprint.ShaderParams["f4_TintColor"].SetValue(Color.DarkRed.ToVector4());

            _transform = Matrix.Identity;
        }

        protected float Rotation{
            get { return _rotation; }
            set{
                _rotation = value;
                _transform = Matrix.CreateFromYawPitchRoll(_rotation, 0, 0);
            }
        }

        Vector3 CalculateCursorOffset(XZPoint dimensions){
            int x = (int) (-dimensions.X/2f);
            int z = (int) (-dimensions.Z/2f);

            var ret = new Vector3(x/2f, 0, z/2f);
            return ret;
        }

        protected override void EnableCursorGhost(){
            _ghostedObjectModel.Enabled = true;
            _dimensionFootprint.Enabled = true;
            _dimensionAccessFootprint.Enabled = true;
            _ghostedObjectModel.ShaderParams["f4_TintColor"].SetValue(Color.Green.ToVector4());
            _dimensionFootprint.ShaderParams["f4_TintColor"].SetValue(Color.Green.ToVector4());
            _dimensionAccessFootprint.ShaderParams["f4_TintColor"].SetValue(Color.LightGreen.ToVector4());
        }

        protected override void DisableCursorGhost(DisableReason reason){
            switch (reason){
                case DisableReason.CursorNotValid:
                    _ghostedObjectModel.ShaderParams["f4_TintColor"].SetValue(Color.DarkRed.ToVector4());
                    _dimensionFootprint.ShaderParams["f4_TintColor"].SetValue(Color.DarkRed.ToVector4());
                    _dimensionAccessFootprint.ShaderParams["f4_TintColor"].SetValue(Color.DarkRed.ToVector4());
                    break;
                case DisableReason.NoBoundingBoxInterception:
                    _ghostedObjectModel.Enabled = false;
                    _dimensionFootprint.Enabled = false;
                    _dimensionAccessFootprint.Enabled = false;
                    break;
            }
        }

        protected override void UpdateCursorGhost(){
            var translation = Matrix.CreateTranslation(base.CursorPosition + CursorOffset);
            _ghostedObjectModel.SetObjectTransform(0, _transform*translation);
            var t = (_transform*translation).Translation;
            _dimensionFootprint.Position = (_transform*translation).Translation;
            _dimensionAccessFootprint.Position = (_transform*translation).Translation;
            /*
            _dimensionFootprint.ApplyTransform(v => {
                var vec = Common.MultMatrix(_transform * translation, v.Position);
                v.Position = vec;
                return v;
            }
             */
        }

        protected override void HandleCursorChange(bool isDrawing){
        }

        protected override void HandleCursorRelease(){
            bool isPlacementValid = _gameObjectEnvironment.IsObjectPlacementValid
                (
                    CursorPosition + CursorOffset,
                    _objectGridDims,
                    _hullData.CurDeck,
                    _rotation,
                    _gameObjectType,
                    PlacementSideEffect
                );

            if (!isPlacementValid){
                return;
            }

            var gameObj = new GameObject
                (
                CursorPosition + CursorOffset,
                _hullData.CurDeck,
                _gameObjectType,
                _rotation,
                _objectParams
                );
            _gameObjectEnvironment.AddObject(gameObj, _objectModelName, PlacementSideEffect);

            //This used to be used to regenerate grid to remove grid lines obfuscated/covered by objects.
            //For now, the deckobjectenvironment doesn't mess with the deck's bounding boxes because it
            //would create ridic coupling issues. Leaving this here though as a footnote to keep in mind
            //a clean way to do this in the future.
            //GenerateGuideGrid(); 
        }

        protected override void HandleCursorDown(){
        }

        protected override void OnCurDeckChange(int newDeck){
        }

        protected override void OnEnable(){
            _ghostedObjectModel.Enabled = true;
            _dimensionFootprint.Enabled = true;
            _dimensionAccessFootprint.Enabled = true;
        }

        protected override void OnDisable(){
            _ghostedObjectModel.Enabled = false;
            _dimensionFootprint.Enabled = false;
            _dimensionAccessFootprint.Enabled = false;
        }

        public override void Dispose(){
            _ghostedObjectModel.Dispose();
            _dimensionFootprint.Dispose();
            _dimensionAccessFootprint.Dispose();
            base.Dispose();
        }

        protected override bool IsCursorValid(Vector3 newCursorPos, Vector3 prevCursorPosition, List<Vector3> deckFloorVertexes, float distToPt){
            return _gameObjectEnvironment.IsObjectPlacementValid
                (
                    newCursorPos + CursorOffset,
                    _objectGridDims,
                    _hullData.CurDeck,
                    _rotation,
                    _gameObjectType,
                    PlacementSideEffect
                );
        }
    }
}