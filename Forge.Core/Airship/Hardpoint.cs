using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forge.Core.Logic;
using Microsoft.Xna.Framework;

namespace Forge.Core.Airship {
    class Hardpoint {
        //cooldowns and stuff should be handled by the hardpoint rather than the airship

        public Matrix WorldMatrix;

        //should side be inferred from aimdir?
        public Hardpoint(Vector3 position, Vector3 aimDir, ProjectilePhysics.ObjectVariant enemyVariant){
            throw new NotImplementedException();
        }

        public void Fire(){
            throw new NotImplementedException();
        }

        public void Terminate(){
            throw new NotImplementedException();
        }
    }
}
