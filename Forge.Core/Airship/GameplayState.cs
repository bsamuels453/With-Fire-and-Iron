using Forge.Core.GameState;
using Forge.Core.Logic;
using Forge.Core.TerrainManager;
using Forge.Framework;
using Forge.Framework.UI;

namespace Forge.Core.Airship {
    class GameplayState : IGameState{
        readonly BodyCenteredCamera _cameraController;
        Airship _airship;

        Button[] _highlightMasks;
        Button _speedIndicator;

        Button _deckUpButton;
        Button _deckDownButton;

        TerrainUpdater _terrainUpdater;

        public GameplayState(){
            GamestateManager.UseGlobalRenderTarget = true;

            _terrainUpdater = new TerrainUpdater();

            _airship = AirshipPackager.Import("Export.airship");
            _cameraController = new BodyCenteredCamera();
            GamestateManager.CameraController = _cameraController;
            _cameraController.SetCameraTarget(_airship.Position);


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
                buttonGen.Y = height * i + yPos;
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
            _deckUpButton.OnLeftClickDispatcher += _airship.AddVisibleLayer;
            _deckDownButton.OnLeftClickDispatcher += _airship.RemoveVisibleLayer;
        }

        public void Update(InputState state, double timeDelta) {
            _airship.Update(ref state, timeDelta);
            _cameraController.SetCameraTarget(_airship.Position);
            _cameraController.Update(ref state, timeDelta);

            int incremental = (int)((_airship.Velocity / _airship.ModelAttributes.MaxForwardSpeed) * 3);

            int absSpeed = 6 - (incremental + 3);
            foreach (var button in _highlightMasks){
                button.Alpha = 0.65f;
            }
            _highlightMasks[absSpeed].Alpha = 0;
            _terrainUpdater.Update(state, timeDelta);
        }

        public void Draw(){
            //throw new NotImplementedException();
        }

        public void Dispose() {
            _airship.Dispose();
            _terrainUpdater.Dispose();
        }
    }
}
