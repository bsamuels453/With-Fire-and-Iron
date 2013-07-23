#region

using Forge.Core.Airship.Data;
using Forge.Core.Airship.Export;
using Forge.Core.Camera;
using Forge.Core.ObjectEditor;
using Forge.Core.ObjectEditor.UI;
using Forge.Framework.Draw;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework;

#endregion

namespace Forge.Core.GameState{
    public class ObjectEditorState : IGameState{
        readonly BodyCenteredCamera _cameraController;
        readonly GameObjectEnvironment _gameObjectEnvironment;
        readonly ObjectEditorUI _doodadUI;
        readonly HullEnvironment _hullEnvironment;
        readonly Battlefield _placeboBattlefield;
        readonly RenderTarget _renderTarget;
        readonly InternalWallEnvironment _wallEnvironment;

        public ObjectEditorState(){
            _renderTarget = new RenderTarget(0, 0, Resource.ScreenSize.X, Resource.ScreenSize.Y);
            _renderTarget.Bind();

            _cameraController = new BodyCenteredCamera(false);
            GameStateManager.CameraController = _cameraController;

            _placeboBattlefield = new Battlefield();
            AirshipPackager.ConvertDefToProtocol(new DefinitionPath("ExportedAirship"), new SerializedPath("ExportedAirship"));
            var serial = AirshipPackager.LoadAirshipSerialization(new SerializedPath("ExportedAirship"));

            _hullEnvironment = new HullEnvironment(serial);

            _gameObjectEnvironment = new GameObjectEnvironment(_hullEnvironment);
            _wallEnvironment = new InternalWallEnvironment(_hullEnvironment, _gameObjectEnvironment);
            _gameObjectEnvironment.InternalWallEnvironment = _wallEnvironment;

            _cameraController.SetCameraTarget(_hullEnvironment.CenterPoint);
            _doodadUI = new ObjectEditorUI(_hullEnvironment, _gameObjectEnvironment, _wallEnvironment, _renderTarget);

        }

        #region IGameState Members

        public void Dispose(){
            _hullEnvironment.Dispose();
            _gameObjectEnvironment.Dispose();
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
    }
}