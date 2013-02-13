#region

using Gondola.Draw;
using Gondola.Logic;
using Gondola.Logic.GameState;
using Microsoft.Xna.Framework;

#endregion

namespace Gondola{
    public class Gondola : Game{
        readonly GraphicsDeviceManager _graphics;
        GamestateManager _gamestateManager;

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
            Gbl.ScreenSize = new Point(1200, 800);

            var aspectRatio = Gbl.Device.Viewport.Bounds.Width/(float) Gbl.Device.Viewport.Bounds.Height;
            Gbl.ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                fieldOfView: 3.14f/4,
                aspectRatio: aspectRatio,
                nearPlaneDistance: 1,
                farPlaneDistance: 500
                );

            _gamestateManager = new GamestateManager();
            _gamestateManager.AddGameState(new PlayerState(_gamestateManager, 
                new Point(Gbl.Device.Viewport.Bounds.Width, Gbl.Device.Viewport.Bounds.Height)));

            _gamestateManager.AddGameState(new TerrainManager(_gamestateManager));

            IsMouseVisible = true;
            base.Initialize();
        }

        protected override void LoadContent(){
        }

        protected override void UnloadContent(){
            Gbl.CommitHashChanges();
        }

        protected override void Update(GameTime gameTime){
            _gamestateManager.Update();
            base.Update(gameTime);
            //Exit();
        }

        protected override void Draw(GameTime gameTime){
            RenderTarget.BeginDraw();
            _gamestateManager.Draw();
            RenderTarget.EndDraw();
            base.Draw(gameTime);
        }
    }
}