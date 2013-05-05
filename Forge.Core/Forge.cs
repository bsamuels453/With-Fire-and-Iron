#region

using System;
using System.Threading;
using Forge.Core.Airship;
using Forge.Core.Logic;
using Forge.Framework;
using Forge.Framework.Draw;
using Forge.Core.HullEditor;
using Forge.Core.TerrainManager;
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
            Gbl.Device = _graphics.GraphicsDevice;
            Gbl.ContentManager = Content;
            Gbl.ScreenSize = new ScreenSize(1200, 800);

            var aspectRatio = Gbl.Device.Viewport.Bounds.Width/(float) Gbl.Device.Viewport.Bounds.Height;
            Gbl.ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                fieldOfView: 3.14f/4,
                aspectRatio: aspectRatio,
                nearPlaneDistance: 0.01f,
                farPlaneDistance: 50000
                );
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
            GamestateManager.UseGlobalRenderTarget = true;
            GamestateManager.AddGameState(new PlayerState(new Point(Gbl.Device.Viewport.Bounds.Width, Gbl.Device.Viewport.Bounds.Height)));
            GamestateManager.AddGameState(new TerrainManagerState());
            GamestateManager.AddGameState(new AirshipManagerState());
            
            //GamestateManager.AddGameState(new HullEditorState());

            IsMouseVisible = true;
            base.Initialize();
        }

        protected override void LoadContent(){
        }

        protected override void UnloadContent(){
            Gbl.CommitHashChanges();
            GamestateManager.ClearAllStates();
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