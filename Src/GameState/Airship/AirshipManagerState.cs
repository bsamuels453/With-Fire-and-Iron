using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gondola.Logic;
using Gondola.UI;
using Microsoft.Xna.Framework;

namespace Gondola.GameState.Airship {
    class AirshipManagerState : IGameState{
        readonly BodyCenteredCamera _cameraController;
        Airship _airship;

        Button[] _highlightMasks;
        Button _speedIndicator;

        Button _deckUpButton;
        Button _deckDownButton;

        public AirshipManagerState(){
            GamestateManager.UseGlobalRenderTarget = true;

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
            int absSpeed = 6 - (_airship.EngineSpeed + 3);
            foreach (var button in _highlightMasks){
                button.Alpha = 0.65f;
            }
            _highlightMasks[absSpeed].Alpha = 0;
        }

        public void Draw(){
            //throw new NotImplementedException();
        }

        public void Dispose() {
            //throw new NotImplementedException();
        }
    }
}
