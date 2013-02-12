#region

using System.Diagnostics;
using Gondola.Draw;
using Gondola.Logic.Terrain;
using Gondola.UI;

#endregion

namespace Gondola.Logic.GameState{
    internal class TestState : IGameState{
        //readonly Sprite2D sprite;
        readonly RenderTarget target;


        readonly Button button;
        readonly Button button1;

        public TestState(){
            var t = new TerrainGen();
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 500; i++){

                t.GenerateChunk(i, 0);



            }
            sw.Stop();
            double d = sw.ElapsedMilliseconds / 500d;
            int g = 5;


            target = new RenderTarget();
            //sprite = new Sprite2D(target, "TestTexture", 0, 0, 50, 50);
            var buttongen = new ButtonGenerator("ToolbarButton64.json");
            buttongen.RenderTarget = target;
            buttongen.TextureName = "UI_TestTexture";
            buttongen.X = 0;
            buttongen.Y = 0;
            button = buttongen.GenerateButton();

            buttongen.X = 64;
            buttongen.Y = 0;
            button1 = buttongen.GenerateButton();
        }

        #region IGameState Members

        public void Dispose(){
            target.Dispose();
        }

        public void Update(InputState state, double timeDelta){
            button.Update(ref state, timeDelta);
            button1.Update(ref state, timeDelta);
        }

        public void Draw(){
            target.Bind();
            //sprite.Draw();
            button.Draw();
            button1.Draw();
            target.Unbind();
        }

        #endregion
    }
}