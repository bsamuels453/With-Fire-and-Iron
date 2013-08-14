//#define PLAYMODE

#region

using System;
using System.Diagnostics;
using Forge.Core.GameObjects;
using Forge.Core.GameState;
using Forge.Framework;
using Forge.Framework.Draw;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework;
using Matrix = MonoGameUtility.Matrix;

#endregion

namespace Forge.Core{
    public class Forge : Game{
        static Forge _game;
        readonly GraphicsDeviceManager _graphics;
        Process _currentProcess;
        Stopwatch _fpsStopwatch;
        int _numFramesLastSecond;

        public Forge(){
            _game = this;
            Content.RootDirectory = "Content";
            _graphics = new GraphicsDeviceManager(this){
                PreferredBackBufferWidth = 1200,
                PreferredBackBufferHeight = 800,
                SynchronizeWithVerticalRetrace = false,
            };
        }

        public static void ExitGame(){
            _game.Exit();
        }

        protected override void Initialize(){
            DebugConsole.InitalizeConsole();
            DebugStateShift.AddNewSet("RunningSlowly", false);
            DebugConsole.WriteLine("Initializing resources...");
            Resource.Initialize(Content, _graphics.GraphicsDevice);
            Resource.ScreenSize = new ScreenSize(1200, 800);
            _currentProcess = Process.GetCurrentProcess();

            IsFixedTimeStep = true;
            //for some reason beyond my comprehension, you have to use 64 denomator for 60fps
            TargetElapsedTime = new TimeSpan((long) (TimeSpan.TicksPerSecond*(1/64d)));

            var aspectRatio = Resource.Device.Viewport.Bounds.Width/(float) Resource.Device.Viewport.Bounds.Height;
            Resource.ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView
                (
                    fieldOfView: 3.14f/4,
                    aspectRatio: aspectRatio,
                    nearPlaneDistance: 0.3f,
                    farPlaneDistance: 13000f
                );

            GameObjectType.Initialize();
            DebugConsole.WriteLine("Resource initialization complete");

            DebugConsole.WriteLine("Initializing game-state...");
#if PLAYMODE
            GameStateManager.AddGameState(new PrimaryGameMode());
#else
            GameStateManager.AddGameState(new ObjectEditorState());
#endif

            IsMouseVisible = true;
            DebugConsole.WriteLine("Game-state initialization completed.");
            _fpsStopwatch = new Stopwatch();
            _fpsStopwatch.Start();
            base.Initialize();
            DebugConsole.WriteLine("Game initialization completed.");
        }

        protected override void LoadContent(){
        }

        protected override void UnloadContent(){
            var timer = new Stopwatch();
            DebugConsole.WriteLine("Unloading game-state resources...");
            timer.Start();
            GameStateManager.ClearState();
            timer.Stop();
            DebugConsole.WriteLine("Game-state resources released in " + timer.ElapsedMilliseconds + " ms");
            DebugConsole.DisposeStatic();
            Resource.Dispose();
        }

        protected override void Update(GameTime gameTime){
            GameStateManager.Update();
            base.Update(gameTime);
            DebugText.SetText("RunningSlowly", "RunningSlowly: " + gameTime.IsRunningSlowly);
            DebugText.SetText("PrivateMem", "Private: " + _currentProcess.PrivateMemorySize64/1000000f + " MB");
        }

        protected override void Draw(GameTime gameTime){
            RenderTarget.BeginDraw();
            GameStateManager.Draw();
            RenderTarget.EndDraw();
            base.Draw(gameTime);
            _numFramesLastSecond++;
            if (_fpsStopwatch.ElapsedMilliseconds > 1000){
                DebugText.SetText("FPS", "FPS: " + _numFramesLastSecond);
                _numFramesLastSecond = 0;
                _fpsStopwatch.Restart();
            }
        }
    }
}