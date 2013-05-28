#define PLAYMODE

#region

using System;
using System.Threading;
using Forge.Core.Airship;
using Forge.Core.Logic;
using Forge.Framework;
using Forge.Framework.Draw;
using Forge.Core.HullEditor;
using Forge.Core.GameState;
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
            DebugConsole.InitalizeConsole();

            /*
            var p = new ProjectilePhysics();
            var proj = p.AddProjectile(new Vector3(0, 0, 200), new Vector3(0, 0, 0), ProjectilePhysics.EntityVariant.EnemyShip);
            for (int i = 0; i < 500; i++){
                p.Update();
                Thread.Sleep(1);
            }
            proj.Terminate.Invoke();
            p.Dispose();
            Exit();
             */
#if PLAYMODE
            //GamestateManager.AddGameState(new PlayerState(new Point(Resource.Device.Viewport.Bounds.Width, Resource.Device.Viewport.Bounds.Height)));
            GamestateManager.AddGameState(new PrimaryGameMode());
#else
            GamestateManager.AddGameState(new HullEditorState());
#endif

            IsMouseVisible = true;
            base.Initialize();
            DebugConsole.WriteLine("Game initalized");
        }

        protected override void LoadContent(){
        }

        protected override void UnloadContent(){
            Resource.CommitHashChanges();
            GamestateManager.ClearState();
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