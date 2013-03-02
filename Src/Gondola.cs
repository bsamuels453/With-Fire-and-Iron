#region

using Gondola.Draw;
using Gondola.GameState;
using Gondola.GameState.Airship;
using Gondola.GameState.HullEditor;
using Gondola.GameState.TerrainManager;
using Gondola.Logic;
using Microsoft.Xna.Framework;

#endregion

namespace Gondola{
    public class Gondola : Game{
        readonly GraphicsDeviceManager _graphics;
        public Gondola(){
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
                nearPlaneDistance: 1,
                farPlaneDistance: 50000
                );
            GamestateManager.UseGlobalRenderTarget = true;
            GamestateManager.AddGameState(new PlayerState(new Point(Gbl.Device.Viewport.Bounds.Width, Gbl.Device.Viewport.Bounds.Height)));
            GamestateManager.AddGameState(new TerrainManager());
            GamestateManager.AddGameState(new AirshipManagerState());
            //GamestateManager.AddGameState(new HullEditor());

            IsMouseVisible = true;
            base.Initialize();
        }

        protected override void LoadContent(){
        }

        protected override void UnloadContent(){
            Gbl.CommitHashChanges();
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