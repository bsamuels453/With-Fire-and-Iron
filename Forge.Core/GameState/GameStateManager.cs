#region

using System.Diagnostics;
using Forge.Core.Camera;
using Forge.Core.Input;
using Forge.Framework.Draw;

#endregion

namespace Forge.Core.GameState{
    public delegate void OnCameraControllerChange(ICamera prevCamera, ICamera newCamera);

    public static class GamestateManager{
        static readonly InputHandler _inputHandler;

        static IGameState _activeState;
        static readonly Stopwatch _stopwatch;

        static GamestateManager(){
            _activeState = null;
            _inputHandler = new InputHandler();
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            DebugText.CreateText("FPS", 0, 0);
            DebugText.CreateText("RunningSlowly", 0, 11);
            DebugText.CreateText("PrivateMem", 0, 24);
        }

        public static ICamera CameraController { get; set; }


        public static void ClearState(){
            _activeState.Dispose();
            _activeState = null;
        }

        public static void AddGameState(IGameState newState){
            _activeState = newState;
        }

        public static void Update(){
            _stopwatch.Stop();
            double d = _stopwatch.ElapsedMilliseconds;
            _stopwatch.Restart();
            _inputHandler.Update();

            _activeState.Update(_inputHandler.CurrentInputState, 16.6666667f);
        }

        public static void Draw(){
            _activeState.Draw();
        }
    }
}