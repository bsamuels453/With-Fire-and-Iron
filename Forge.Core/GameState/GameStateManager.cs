#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Forge.Framework.Draw;
using Forge.Core.Logic;
using Forge.Framework.UI;
using Forge.Core.Util;
using Microsoft.Xna.Framework;

#endregion

namespace Forge.Core.GameState{
    delegate void OnCameraControllerChange(ICamera prevCamera, ICamera newCamera);

    internal static class GamestateManager{
        static readonly InputHandler _inputHandler;

        static IGameState _activeState;
        static ICamera _cameraController;
        static Stopwatch _stopwatch;

        static GamestateManager(){
            _activeState = null;
            _inputHandler = new InputHandler();
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        public static ICamera CameraController {
            get { return _cameraController; }
            set {_cameraController = value;}
        }


        public static void ClearState() {
            _activeState.Dispose();
            _activeState = null;
        }

        public static void AddGameState(IGameState newState) {
            _activeState = newState;
        }

        public static void Update() {
            _inputHandler.Update();

            _stopwatch.Stop();
            double d = _stopwatch.ElapsedMilliseconds;
            _activeState.Update(_inputHandler.CurrentInputState, d);
            _stopwatch.Restart();

        }

        public static void Draw() {
            _activeState.Draw();

            /*if (_useGlobalRenderTarget){
                _globalRenderTarget.Draw(CameraController.ViewMatrix, Color.Transparent);
            }*/
        }
    }
}