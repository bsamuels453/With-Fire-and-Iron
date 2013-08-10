#region

using Forge.Core.Airship.Data;
using Forge.Core.Airship.Export;
using Forge.Core.Airship.Generation;
using Forge.Core.Camera;
using Forge.Core.ObjectEditor;
using Forge.Core.ObjectEditor.Subsystems;
using Forge.Core.ObjectEditor.UI;
using Forge.Framework.Control;
using Forge.Framework.Draw;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#endregion

namespace Forge.Core.GameState{
    public class ObjectEditorState : IGameState{
        readonly BodyCenteredCamera _cameraController;
        readonly ObjectEditorUI _doodadUI;
        readonly GameObjectEnvironment _gameObjectEnvironment;
        readonly HullEnvironment _hullEnvironment;
        readonly ObjectFootprintVisualizer _objFootprintVisualizer;
        readonly Battlefield _placeboBattlefield;
        readonly RenderTarget _renderTarget;
        readonly InternalWallEnvironment _wallEnvironment;

        public ObjectEditorState(){
            _renderTarget = new RenderTarget(0, 0, Resource.ScreenSize.X, Resource.ScreenSize.Y);
            _renderTarget.Bind();

            _cameraController = new BodyCenteredCamera(false);
            GameStateManager.CameraController = _cameraController;

            _placeboBattlefield = new Battlefield();
            //AirshipPackager.ConvertDefToProtocol(new DefinitionPath("ExportedAirship"), new SerializedPath("ExportedAirship"));
            var serial = AirshipPackager.LoadAirshipSerialization(new SerializedPath("ExportedAirship"));

            _hullEnvironment = new HullEnvironment(serial);

            _gameObjectEnvironment = new GameObjectEnvironment(_hullEnvironment);
            _wallEnvironment = new InternalWallEnvironment(_hullEnvironment, _gameObjectEnvironment);
            _gameObjectEnvironment.InternalWallEnvironment = _wallEnvironment;
            _objFootprintVisualizer = new ObjectFootprintVisualizer(_gameObjectEnvironment, _hullEnvironment);

            _cameraController.SetCameraTarget(_hullEnvironment.CenterPoint);
            _doodadUI = new ObjectEditorUI(_hullEnvironment, _gameObjectEnvironment, _wallEnvironment, _renderTarget);

            var controller = new KeyboardController();
            controller.CreateNewBind(Keys.S, 0, SaveShip, BindCondition.OnKeyDown, Keys.LeftControl);
            KeyboardManager.SetActiveController(controller);
        }

        #region IGameState Members

        public void Dispose(){
            _hullEnvironment.Dispose();
            _gameObjectEnvironment.Dispose();
            _objFootprintVisualizer.Dispose();
            _wallEnvironment.Dispose();
            _placeboBattlefield.Dispose();
            _doodadUI.Dispose();
            _renderTarget.Unbind();
            _renderTarget.Dispose();
        }

        public void Update(double timeDelta){
            _doodadUI.UpdateLogic(timeDelta);
        }

        public void Draw(){
            _renderTarget.Draw(_cameraController.ViewMatrix, Color.CornflowerBlue);
        }

        #endregion

        void SaveShip(object o, int i, ForgeKeyState state){
            var attribs = HullAttributeGenerator.Generate(4);

            AirshipPackager.ExportToProtocolFile
                (
                    new SerializedPath("ExportedAirship"),
                    _hullEnvironment.HullSectionContainer,
                    _hullEnvironment.DeckSectionContainer,
                    attribs,
                    _gameObjectEnvironment.DumpGameObjects()
                );
        }
    }
}