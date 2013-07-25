#region

using Forge.Core.Airship.Data;

#endregion

namespace Forge.Core.Airship.Controllers{
    /// <summary>
    /// The controller used for the AI to control the airship.
    /// </summary>
    public class AIAirshipController : AirshipController{
        public AIAirshipController(ModelAttributes modelData, AirshipStateData stateData, WeaponSystems weaponSystems, AirshipIndexer airships) :
            base(modelData, stateData, airships, weaponSystems){
        }

        protected override void UpdateController(double timeDelta){
        }
    }
}