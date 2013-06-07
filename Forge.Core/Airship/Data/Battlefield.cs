using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Forge.Core.Physics;
using Forge.Framework;

namespace Forge.Core.Airship.Data {
    /// <summary>
    /// This class contains all of the information relevant to the current battlefield, for use by the airship class' AI, physics engine, etc.
    /// </summary>
    class Battlefield : IDisposable {
        public readonly AirshipIndexer ShipsOnField;
        public readonly ProjectilePhysics ProjectileEngine;

        public Battlefield(){
            ShipsOnField = new AirshipIndexer();
            ProjectileEngine = new ProjectilePhysics();
        }

        bool _disposed;

        public void Dispose(){
            Debug.Assert(!_disposed);
            ProjectileEngine.Dispose();
            foreach (Airship airship in ShipsOnField){
                airship.Dispose();
            }
            _disposed = true;
        }

        public void Update(ref InputState state, double timeDelta){
            foreach (Airship airship in ShipsOnField){
                airship.Update(ref state, timeDelta);
            }
            ProjectileEngine.Update(timeDelta);

        }

        ~Battlefield(){
            Debug.Assert(_disposed);
        }
    }
}
