#region

using System;
using System.Diagnostics;
using Forge.Core.Physics;
using Forge.Framework;
using MonoGameUtility;

#endregion

namespace Forge.Core.Airship{
    /// <summary>
    /// Used to represent an airship's hardpoints, aka the cannons. Essentially this
    /// class takes all the fancy mumbo jumbo and side effects behind firing a cannon 
    /// and wraps it behind a pretty class.
    /// </summary>
    public class Hardpoint : IDisposable{
        readonly Vector3 _aimDir;
        readonly ProjectileEmitter _emitter;
        readonly Vector3 _localPosition;
        public Matrix ShipTranslationMtx;
        bool _disposed;

        /// <summary>
        /// </summary>
        /// <param name="position"> Model space position of cannon </param>
        /// <param name="aimDir"> Direction in which the cannon fires. </param>
        /// <param name="emitter"> </param>
        public Hardpoint(Vector3 position, Vector3 aimDir, ProjectileEmitter emitter){
            _localPosition = position;
            _aimDir = aimDir;
            _emitter = emitter;
        }

        #region IDisposable Members

        public void Dispose(){
            Debug.Assert(!_disposed);
            _disposed = true;
        }

        #endregion

        public void Fire(){
            var globalPosition = Common.MultMatrix(ShipTranslationMtx, _localPosition);

            Vector3 _, __;
            Quaternion q;
            ShipTranslationMtx.Decompose(out _, out q, out __);
            var rotate = Matrix.CreateFromQuaternion(q);
            var globalAim = Common.MultMatrix(rotate, _aimDir);

            _emitter.CreateProjectile(globalPosition, globalAim);
        }

        //target will need to be converted to local coords
        public void AimAt(Vector3 target){
            throw new NotImplementedException();
        }

        ~Hardpoint(){
            if (!_disposed)
                throw new ResourceNotDisposedException();
        }
    }
}