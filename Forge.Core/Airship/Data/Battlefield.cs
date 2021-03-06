﻿#region

using System;
using System.Diagnostics;
using Forge.Core.Physics;

#endregion

namespace Forge.Core.Airship.Data{
    /// <summary>
    ///   This class contains all of the information relevant to the current battlefield. 
    ///   It acts as a container for that information, and handles the updating/disposal of said information.
    /// </summary>
    public class Battlefield : IDisposable{
        public readonly ProjectilePhysics ProjectileEngine;
        public readonly AirshipIndexer ShipsOnField;

        bool _disposed;

        public Battlefield(){
            ShipsOnField = new AirshipIndexer();
            ProjectileEngine = new ProjectilePhysics();
        }

        #region IDisposable Members

        public void Dispose(){
            Debug.Assert(!_disposed);
            ProjectileEngine.Dispose();
            foreach (Airship airship in ShipsOnField){
                airship.Dispose();
            }
            _disposed = true;
        }

        #endregion

        public void Update(double timeDelta){
            foreach (Airship airship in ShipsOnField){
                airship.Update(timeDelta);
            }
            ProjectileEngine.Update(timeDelta);
        }

        ~Battlefield(){
            Debug.Assert(_disposed);
        }
    }
}