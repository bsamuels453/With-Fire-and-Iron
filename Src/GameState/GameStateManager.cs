#region

using System;
using System.Collections.Generic;
using Gondola.Draw;
using Gondola.Logic;
using Gondola.Util;
using Microsoft.Xna.Framework;

#endregion

namespace Gondola.GameState{
    internal static class GamestateManager{
        static readonly InputHandler _inputHandler;

        static readonly List<IGameState> _activeStates;
        static readonly Dictionary<SharedStateData, object> _sharedData;

        static bool _useGlobalRenderTarget;
        static RenderTarget _globalRenderTarget;
        public static ICamera Camera;

        public static bool UseGlobalRenderTarget{
            set {
                if (value) {
                    if (!_useGlobalRenderTarget) {
                        _useGlobalRenderTarget = true;
                        _globalRenderTarget = new RenderTarget();
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
        }

        public static void ClearAllStates() {
            foreach (var state in _activeStates){
                state.Dispose();
            }
            _useGlobalRenderTarget = false;
            _globalRenderTarget.Dispose();

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
               // _globalRenderTarget.Bind();
            }
            for (int i = 0; i < _activeStates.Count; i++){
                    _activeStates[i].Update(_inputHandler.CurrentInputState, 0);
                }
            
            if (_useGlobalRenderTarget){
               // _globalRenderTarget.Unbind();
            }
        }

        public static void Draw() {
            foreach (var state in _activeStates){
                state.Draw();
            }

            if (_useGlobalRenderTarget){
                _globalRenderTarget.Draw(Camera.ViewMatrix, Color.Transparent);
            }
        }
    }
}