#region

using System;
using System.Collections.Generic;
using Forge.Core.Logic;
using Forge.Framework;
using Forge.Framework.Draw;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Forge.Core.Airship{
    internal class Hardpoint{
        readonly List<ProjectilePhysics.Projectile> _activeProjectiles;
        readonly Vector3 _aimDir;
        readonly ProjectilePhysics.ObjectVariant _enemyVariant;
        readonly Vector3 _localPosition;
        readonly ProjectilePhysics _projectileEngine;
        readonly ObjectModelBuffer<ProjectilePhysics.Projectile> _projectileBuff;
        Matrix _shipTranslationMtx;

        /// <summary>
        /// </summary>
        /// <param name="position"> Model space position of cannon </param>
        /// <param name="aimDir"> Direction in which the cannon fires. </param>
        /// <param name="projectileEngine"> </param>
        /// <param name="enemyVariant"> The kind of enemy that this hardpoint can damage </param>
        public Hardpoint(Vector3 position, Vector3 aimDir, ProjectilePhysics projectileEngine, ProjectilePhysics.ObjectVariant enemyVariant){
            _localPosition = position;
            _aimDir = aimDir;
            _projectileEngine = projectileEngine;
            _enemyVariant = enemyVariant;

            _activeProjectiles = new List<ProjectilePhysics.Projectile>();
            _projectileBuff = new ObjectModelBuffer<ProjectilePhysics.Projectile>(5000, "Shader_TintedModel");
        }

        public void Fire(){
            var globalPosition = Common.MultMatrix(_shipTranslationMtx, _localPosition);

            

            Vector3 _, __;
            Quaternion q;
            _shipTranslationMtx.Decompose(out _, out q, out __);
            var rotate = Matrix.CreateFromQuaternion(q);
            var globalAim = Common.MultMatrix(rotate, _aimDir);

            var translation = Matrix.CreateTranslation(globalPosition);
            var handle = _projectileEngine.AddProjectile(globalPosition, globalAim, _enemyVariant);
            _projectileBuff.AddObject(handle, Gbl.ContentManager.Load<Model>("Models/Sphere"), translation);
            _activeProjectiles.Add(handle);
        }

        public void Terminate(){
            foreach (var projectile in _activeProjectiles){
                projectile.Terminate.Invoke();
            }
        }

        public void Update(double timeDelta, Matrix shipTranslationMtx){
            _shipTranslationMtx = shipTranslationMtx;
            foreach (var projectile in _activeProjectiles){
                var translation = Matrix.CreateTranslation(projectile.GetPosition.Invoke());
                _projectileBuff.SetObjectTransform(projectile, translation);
            }
        }

        //target will need to be converted to local coords
        public void AimAt(Vector3 target){
            throw new NotImplementedException();
        }
    }
}