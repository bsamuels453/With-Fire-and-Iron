using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gondola.Logic;
using Microsoft.Xna.Framework;

namespace Gondola.GameState.Airship {
    class AirshipManagerState : IGameState{
        readonly BodyCenteredCamera _cameraController;
        Airship _airship;

        public AirshipManagerState(){
            GamestateManager.UseGlobalRenderTarget = true;
            _airship = AirshipPackager.Import("Export.airship");
            _cameraController = new BodyCenteredCamera();
            GamestateManager.CameraController = _cameraController;
            _cameraController.SetCameraTarget(_airship.Centroid + new Vector3(0,1000,0));
        }

        public void Update(InputState state, double timeDelta){
            _cameraController.Update(ref state, timeDelta);
        }

        public void Draw(){
            //throw new NotImplementedException();
        }

        public void Dispose() {
            //throw new NotImplementedException();
        }
    }
}
