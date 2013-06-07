using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forge.Core.Airship.Controllers.AutoPilot {
    class KeepAtRange : AirshipAutoPilot{
        public KeepAtRange(AirshipController controller, AirshipIndexer airships) : base(controller, airships){
        }

        protected override void UpdateChild(double timeDelta){
            throw new NotImplementedException();
        }
    }
}
