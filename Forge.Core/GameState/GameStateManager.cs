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

        static readonly List<IGameState> _activeStates;
        static readonly Dictionary<SharedStateData, object> _sharedData;

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
            _activeStates = new List<IGameState>();
            _inputHandler = new InputHandler();
            _sharedData = new Dictionary<SharedStateData, object>();//todo-optimize: might be able to make this into a list instead
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        public static void ClearAllStates() {
            foreach (var state in _activeStates){
                state.Dispose();
            }
            if (_useGlobalRenderTarget){
                _useGlobalRenderTarget = false;
                _globalRenderTarget.Unbind();
                _globalElementCollection.Unbind();
                _globalRenderTarget.Dispose();
            }

            _sharedData.Clear();
            _activeStates.Clear();
        }

        public static void ClearState(IGameState state) {
            _activeStates.Remove(state);
            state.Dispose();
        }

        public static object QuerySharedData(SharedStateData identifier) {
            return _sharedData[identifier];
        }

        public static void AddSharedData(SharedStateData identifier, object data) {
            _sharedData.Add(identifier, data);
        }

        public static void ModifySharedData(SharedStateData identifier, object data) {
            _sharedData[identifier] = data;
        }

        public static void DeleteSharedData(SharedStateData identifier) {
            _sharedData.Remove(identifier);
        }

        public static void AddGameState(IGameState newState) {
            _activeStates.Add(newState);
        }

        public static void Update() {
            _inputHandler.Update();
            if (_useGlobalRenderTarget){
            }
            for (int i = 0; i < _activeStates.Count; i++){
                    _stopwatch.Stop();
                    double d = _stopwatch.ElapsedMilliseconds;
                    _stopwatch.Start();
                    _activeStates[i].Update(_inputHandler.CurrentInputState, d);
                }
            _stopwatch.Restart();
            if (_useGlobalRenderTarget){
                _globalElementCollection.UpdateLogic(0);
                _globalElementCollection.UpdateInput(ref _inputHandler.CurrentInputState);
            }
        }

        public static void Draw() {
            foreach (var state in _activeStates){
                state.Draw();
            }

            if (_useGlobalRenderTarget){
                _globalRenderTarget.Draw(CameraController.ViewMatrix, Color.Transparent);
            }
        }
    }
}