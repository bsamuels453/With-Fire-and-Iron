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
        static bool _useGlobalRenderTarget;
        static RenderTarget _globalRenderTarget;
        static UIElementCollection _globalElementCollection;
        static ICamera _cameraController;
        static Stopwatch _stopwatch;
        

        public static ICamera CameraController{
            get { return _cameraController; }
            set {
                if (OnCameraControllerChange != null){
                    OnCameraControllerChange.Invoke(_cameraController, value);
                }
                _cameraController = value; 
            }
        }

        public static event OnCameraControllerChange OnCameraControllerChange;

        public static bool UseGlobalRenderTarget{
            set {
                if (value) {
                    if (!_useGlobalRenderTarget) {
                        _useGlobalRenderTarget = true;
                        _globalRenderTarget = new RenderTarget();
                        _globalElementCollection = new UIElementCollection();
                        _globalElementCollection.Bind();
                        _globalRenderTarget.Bind();
                    }
                }
                else{
                    throw new Exception("not supported");
                }
            }
        }

        static GamestateManager(){
            _activeState = null;
            _inputHandler = new InputHandler();
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
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
            if (_useGlobalRenderTarget){
            }
            _stopwatch.Stop();
            double d = _stopwatch.ElapsedMilliseconds;
            _activeState.Update(_inputHandler.CurrentInputState, d);
            _stopwatch.Restart();

            if (_useGlobalRenderTarget){
                _globalElementCollection.UpdateLogic(0);
                _globalElementCollection.UpdateInput(ref _inputHandler.CurrentInputState);
            }
        }

        public static void Draw() {
            _activeState.Draw();

            if (_useGlobalRenderTarget){
                _globalRenderTarget.Draw(CameraController.ViewMatrix, Color.Transparent);
            }
        }
    }
}