#region

using Forge.Core.Airship.Controllers;
using Forge.Core.Airship.Controllers.AutoPilot;
using Forge.Core.Airship.Data;
using Forge.Core.Airship.Export;
using Forge.Core.Camera;
using Forge.Core.Terrain;
using Forge.Framework.Control;
using Forge.Framework.Draw;
using Forge.Framework.UI;
using Microsoft.Xna.Framework;

#endregion

namespace Forge.Core.GameState{
    public class PrimaryGameMode : IGameState{
        readonly Battlefield _battlefield;
        readonly BodyCenteredCamera _cameraController;
        readonly RenderTarget _renderTarget;
        readonly TerrainUpdater _terrainUpdater;
        readonly UIElementCollection _uiElementCollection;


        public PrimaryGameMode(){
            _renderTarget = new RenderTarget();
            _renderTarget.Bind();
            _uiElementCollection = new UIElementCollection(GamestateManager.MouseManager);
            _uiElementCollection.Bind();

            _terrainUpdater = new TerrainUpdater();

            _battlefield = new Battlefield();
            //AirshipPackager.ConvertDefToProtocolFile("ExportedAirship");

            _battlefield.ShipsOnField.Add(AirshipPackager.LoadAirship("PlayerShip", _battlefield));
            _battlefield.ShipsOnField.Add(AirshipPackager.LoadAirship("AIShip", _battlefield));

            var controller = _battlefield.ShipsOnField[0].Controller;
            var binds = ((PlayerAirshipController) controller).GenerateKeyboardBindings();
            KeyboardManager.SetActiveController(binds);

            /*
            _battlefield.ShipsOnField[1].SetAutoPilot
                (new Orbit
                    (
                    _battlefield.ShipsOnField[1],
                    _battlefield.ShipsOnField,
                    0,
                    500
                    )
                );
             */


            _cameraController = new BodyCenteredCamera(false);
            GamestateManager.CameraController = _cameraController;
            _cameraController.SetCameraTarget(_battlefield.ShipsOnField[0].StateData.Position);
            /*
            var buttonGen = new ButtonGenerator();
            const int yPos = 100;
            buttonGen.X = 0;
            buttonGen.Y = yPos;
            buttonGen.SpriteTexRepeatX = 0.6625f;
            buttonGen.SpriteTexRepeatY = 1.509433962f;
            buttonGen.Height = 84;
            buttonGen.Width = 53;
            buttonGen.Depth = FrameStrata.Medium;
            buttonGen.TextureName = "Icons/SpeedIndicator";
            _speedIndicator = buttonGen.GenerateButton();

            const int height = 14;
            buttonGen.TextureName = "Effects/SolidBlack";
            buttonGen.Height = height;
            buttonGen.Width = 53;
            buttonGen.Depth = FrameStrata.High;
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
             */
            DebugText.CreateText("FPS", 0, 0);
            DebugText.CreateText("RunningSlowly", 0, 11);
            DebugText.CreateText("PrivateMem", 0, 24);
        }

        #region IGameState Members

        public void Update(double timeDelta){
            _uiElementCollection.Update((float) timeDelta);

            _battlefield.Update(timeDelta);
            _cameraController.SetCameraTarget(_battlefield.ShipsOnField[0].StateData.Position);

            _terrainUpdater.Update(timeDelta);
        }

        public void Draw(){
            _renderTarget.Draw(_cameraController.ViewMatrix, Color.Transparent);
        }

        public void Dispose(){
            _terrainUpdater.Dispose();
            _renderTarget.Dispose();
            _battlefield.Dispose();
            _uiElementCollection.Dispose();
        }

        #endregion
    }
}