#region

using System.Diagnostics;
using Forge.Core.Camera;
using Forge.Framework.Control;
using Forge.Framework.Draw;

#endregion

namespace Forge.Core.GameState{
    public delegate void OnCameraControllerChange(ICamera prevCamera, ICamera newCamera);

    public static class GamestateManager{
        public static readonly MouseManager MouseManager;

        static IGameState _activeState;
        static readonly Stopwatch _stopwatch;

        static GamestateManager(){
            _activeState = null;
            MouseManager = new MouseManager();
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
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
            _stopwatch.Restart();

            const double timeDelta = 16.666666667f;

            MouseManager.UpdateMouse(timeDelta);
            KeyboardManager.UpdateKeyboard();

            _activeState.Update(timeDelta);
        }

        public static void Draw(){
            _activeState.Draw();
        }
    }
}