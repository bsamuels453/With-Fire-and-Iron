using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forge.Core.Airship.Data;
using Forge.Framework;
using MonoGameUtility;

namespace Forge.Core.Airship {
    class AIAirshipController : AirshipController{
        public AIAirshipController(Action<Matrix> setWorldMatrix, ModelAttributes modelData, AirshipMovementData movementData, List<Hardpoint> hardPoints) : 
            base(setWorldMatrix, modelData, movementData, hardPoints){



        }

        protected override void UpdateController(ref InputState state, double timeDelta){


        }
    }
}
