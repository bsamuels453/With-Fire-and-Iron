#region

using System.Collections.Generic;
using Forge.Core.Airship.Data;
using Forge.Framework;

#endregion

namespace Forge.Core.Airship.Controllers{
    internal class AIAirshipController : AirshipController{
        public AIAirshipController(ModelAttributes modelData, AirshipStateData stateData, List<Hardpoint> hardPoints) :
            base(modelData, stateData, hardPoints){
        }

        protected override void UpdateController(ref InputState state, double timeDelta){
        }
    }
}