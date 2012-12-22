#region

using System;
using System.Diagnostics;

#endregion

namespace Gondola.Logic{
    internal class GamestateManager : IDisposable{
        readonly InputHandler _inputHandler;
        IGameState _currentState;

        public GamestateManager(){
            _currentState = null;
            _inputHandler = new InputHandler();
        }

        #region IDisposable Members

        public void Dispose(){
            if (_currentState != null){
                _currentState.Dispose();
            }
        }

        #endregion

        public void ClearGameState(){
            Debug.Assert(_currentState != null);

            _currentState = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void SetGameState(IGameState newState){
            Debug.Assert(_currentState == null, "Gamestate must be cleared before a new state is set");

            _currentState = newState;
        }

        public void Update(){
            _inputHandler.Update();
            if (_currentState != null){
                _currentState.Update(_inputHandler.CurrentInputState, 0);
            }
        }

        public void Draw(){
            Debug.Assert(_currentState != null);
            _currentState.Draw();
        }
    }
}