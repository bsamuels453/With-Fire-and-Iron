using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gondola.Draw;
using Gondola.Logic;

namespace Gondola.GameState {
    class TestState : IGameState{
        Sprite2D sprite;
        RenderTarget target;

        Sprite2D sprite2;
        Text2D text1;
        RenderTarget target2;
        Sprite2D sprite3;

        public TestState(){
            target = new RenderTarget();
            sprite = new Sprite2D(target, "TestTexture", 0, 0, 50, 50);

            target2 = new RenderTarget(200,200,200,200, 0.5f);
            sprite2 = new Sprite2D(target2, "TestTexture", 0, 0, 25, 25);

            sprite3 = new Sprite2D(target2, "TestTexture", 0, 0, 50, 50);
            text1 = new Text2D(target2, 0, 0, "test");

        }

        public void Dispose(){
        }

        public void Update(InputState state, double timeDelta){
        }

        public void Draw(){
            target.Bind();
            sprite.Draw();
            target.Unbind();
            
            target2.Bind();
            sprite3.Draw();
            text1.Draw();
            sprite2.Draw();
            

            
            target2.Unbind();

        }
    }
}
