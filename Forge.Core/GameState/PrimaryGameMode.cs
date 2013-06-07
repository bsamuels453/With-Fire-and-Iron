#region

using Forge.Core.Airship.Data;
using Forge.Core.Airship.Export;
using Forge.Core.Camera;
using Forge.Core.Terrain;
using Forge.Framework;
using Forge.Framework.Draw;
using Forge.Framework.UI;
using Microsoft.Xna.Framework;

#endregion

namespace Forge.Core.GameState{
    internal class PrimaryGameMode : IGameState{
        readonly Battlefield _battlefield;
        readonly BodyCenteredCamera _cameraController;

        readonly Button _deckDownButton;
        readonly Button _deckUpButton;
        readonly Button[] _highlightMasks;

        readonly RenderTarget _renderTarget;

        readonly TerrainUpdater _terrainUpdater;

        readonly UIElementCollection _uiElementCollection;

        Button _speedIndicator;

        public PrimaryGameMode(){
            _uiElementCollection = new UIElementCollection();
            _uiElementCollection.Bind();
            _renderTarget = new RenderTarget();
            _renderTarget.Bind();

            _terrainUpdater = new TerrainUpdater();

            _battlefield = new Battlefield();


            _battlefield.ShipsOnField.Add(AirshipPackager.LoadAirship("PlayerShip", _battlefield));
            _battlefield.ShipsOnField.Add(AirshipPackager.LoadAirship("AIShip", _battlefield));

            _cameraController = new BodyCenteredCamera(false);
            GamestateManager.CameraController = _cameraController;
            _cameraController.SetCameraTarget(_battlefield.ShipsOnField[0].Position);

            var buttonGen = new ButtonGenerator();
            const int yPos = 100;
            buttonGen.X = 0;
            buttonGen.Y = yPos;
            buttonGen.SpriteTexRepeatX = 0.6625f;
            buttonGen.SpriteTexRepeatY = 1.509433962f;
            buttonGen.Height = 84;
            buttonGen.Width = 53;
            buttonGen.Depth = DepthLevel.Medium;
            buttonGen.TextureName = "Icons/SpeedIndicator";
            _speedIndicator = buttonGen.GenerateButton();

            const int height = 14;
            buttonGen.TextureName = "Effects/SolidBlack";
            buttonGen.Height = height;
            buttonGen.Width = 53;
            buttonGen.Depth = DepthLevel.High;
            buttonGen.SpriteTexRepeatX = 1;
            buttonGen.SpriteTexRepeatY = 1;

            _highlightMasks = new Button[6];
            for (int i = 0; i < 6; i++){
                buttonGen.Y = height*i + yPos;
                _highlightMasks[i] = buttonGen.GenerateButton();
                _highlightMasks[i].Alpha = 0.65f;
            }
            _highlightMasks[3].Alpha = 0;

            buttonGen = new ButtonGenerator("ToolbarButton32.json");
            buttonGen.X = 0;
            buttonGen.Y = 200;
            buttonGen.TextureName = "Icons/UpArrow";
            _deckUpButton = buttonGen.GenerateButton();
            buttonGen.Y = 200 + 32;
            buttonGen.TextureName = "Icons/DownArrow";
            _deckDownButton = buttonGen.GenerateButton();
            _deckUpButton.OnLeftClickDispatcher += _battlefield.ShipsOnField[0].AddVisibleLayer;
            _deckDownButton.OnLeftClickDispatcher += _battlefield.ShipsOnField[0].RemoveVisibleLayer;

            _uiElementCollection.Unbind();
        }

        #region IGameState Members

        public void Update(InputState state, double timeDelta){
            _uiElementCollection.UpdateInput(ref state);
            _uiElementCollection.UpdateLogic(timeDelta);

            _battlefield.Update(ref state, timeDelta);
            _cameraController.SetCameraTarget(_battlefield.ShipsOnField[0].Position);
            _cameraController.Update(ref state, timeDelta);

            int incremental = (int) ((_battlefield.ShipsOnField[0].Velocity/_battlefield.ShipsOnField[0].ModelAttributes.MaxForwardVelocity)*3);

            int absSpeed = 6 - (incremental + 3);
            foreach (var button in _highlightMasks){
                button.Alpha = 0.65f;
            }
            _highlightMasks[absSpeed].Alpha = 0;
            _terrainUpdater.Update(state, timeDelta);
        }

        public void Draw(){
            _renderTarget.Draw(_cameraController.ViewMatrix, Color.Transparent);
        }

        public void Dispose(){
            _terrainUpdater.Dispose();
            _renderTarget.Dispose();
            _battlefield.Dispose();
        }

        #endregion
    }
}