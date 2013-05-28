#define PLAYMODE

#region

using System;
using System.Diagnostics;
using System.Threading;
using Forge.Core.Airship;
using Forge.Core.Logic;
using Forge.Framework;
using Forge.Framework.Draw;
using Forge.Core.HullEditor;
using Forge.Core.GameState;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework;

#endregion

namespace Forge.Core{
    public class Forge : Game{
        readonly GraphicsDeviceManager _graphics;

        public Forge(){
            Content.RootDirectory = "Content";
            _graphics = new GraphicsDeviceManager(this){
                PreferredBackBufferWidth = 1200,
                PreferredBackBufferHeight = 800,
                SynchronizeWithVerticalRetrace = false,
            };
        }

        protected override void Initialize(){
            DebugConsole.InitalizeConsole();
            DebugConsole.WriteLine("Initializing resources...");
            Resource.Initialize();
            Resource.Device = _graphics.GraphicsDevice;
            Resource.ContentManager = Content;
            Resource.ScreenSize = new ScreenSize(1200, 800);

            var aspectRatio = Resource.Device.Viewport.Bounds.Width/(float) Resource.Device.Viewport.Bounds.Height;
            Resource.ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                fieldOfView: 3.14f/4,
                aspectRatio: aspectRatio,
                nearPlaneDistance: 0.3f,
                farPlaneDistance: 13000f
                );

            DebugConsole.WriteLine("Resource initialization complete");

            DebugConsole.WriteLine("Initializing game-state...");
#if PLAYMODE
            GamestateManager.AddGameState(new PrimaryGameMode());
#else
            GamestateManager.AddGameState(new HullEditorState());
#endif

            IsMouseVisible = true;
            DebugConsole.WriteLine("Game-state initialization completed.");
            base.Initialize();
            DebugConsole.WriteLine("Game initialization completed.");
        }

        protected override void LoadContent(){
        }

        protected override void UnloadContent(){
            var timer = new Stopwatch();
            DebugConsole.WriteLine("Unloading game-state resources...");
            timer.Start();
            GamestateManager.ClearState();
            timer.Stop();
            DebugConsole.WriteLine("Game-state resources released in " + timer.ElapsedMilliseconds + " ms");
            DebugConsole.Dispose();
        }

        protected override void Update(GameTime gameTime){
            GamestateManager.Update();
            base.Update(gameTime);
            //Exit();
        }

        protected override void Draw(GameTime gameTime){
            RenderTarget.BeginDraw();
            GamestateManager.Draw();
            RenderTarget.EndDraw();
            base.Draw(gameTime);
        }
    }
}