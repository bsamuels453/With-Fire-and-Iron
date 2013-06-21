#region

using System.Collections.Generic;
using Forge.Core.Airship.Data;

#endregion

namespace Forge.Core.Airship.Controllers{
    /// <summary>
    /// The controller used for the AI to control the airship.
    /// </summary>
    public class AIAirshipController : AirshipController{
        public AIAirshipController(ModelAttributes modelData, AirshipStateData stateData, List<Hardpoint> hardPoints, AirshipIndexer airships) :
            base(modelData, stateData, airships, hardPoints){
        }

        protected override void UpdateController(double timeDelta){
        }
    }
}