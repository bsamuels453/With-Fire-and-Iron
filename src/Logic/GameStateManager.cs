#region

using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace Gondola.Logic{
    internal class GamestateManager{
        readonly InputHandler _inputHandler;

        readonly List<IGameState> _activeStates;
        readonly Dictionary<SharedStateData, object> _sharedData;

        public GamestateManager(){
            _activeStates = new List<IGameState>();
            _inputHandler = new InputHandler();
            _sharedData = new Dictionary<SharedStateData, object>();
        }

        public void ClearAllStates(){
            foreach (var state in _activeStates){
                state.Dispose();
            }
            _activeStates.Clear();
        }

        public void ClearState(IGameState state){
            _activeStates.Remove(state);
            state.Dispose();
        }

        public object QuerySharedData(SharedStateData identifier) {
            return _sharedData[identifier];
        }

        public void AddSharedData(SharedStateData identifier, object data) {
            _sharedData.Add(identifier, data);
        }

        public void AddGameState(IGameState newState){
            _activeStates.Add(newState);
        }

        public void Update(){
            _inputHandler.Update();
            foreach (var state in _activeStates){
                state.Update(_inputHandler.CurrentInputState, 0);
            }
        }

        public void Draw(){
            foreach (var state in _activeStates){
                state.Draw();
            }
        }
    }
}