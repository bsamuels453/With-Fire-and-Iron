#region

using System;
using System.Diagnostics;
using Forge.Framework;
using MonoGameUtility;

#endregion

namespace Forge.Core.Airship.Controllers.AutoPilot{
    internal class KeepAtRange : AirshipAutoPilot{
        readonly float _preferredRange;
        readonly Airship _targetAirship;

        public KeepAtRange(int targetUid, float range, AirshipController controller, AirshipIndexer airships) : base(controller, airships){
            _preferredRange = range;
            _targetAirship = airships[targetUid];
        }

        protected override void UpdateChild(double timeDelta){
            //get target position

            //figure out if we should go forwards or backwards

            //apply the change

            throw new NotImplementedException();
        }

    }
}