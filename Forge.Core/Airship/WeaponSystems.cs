#region

using System;
using System.Collections.Generic;
using Forge.Core.ObjectEditor;
using Forge.Core.Physics;
using Forge.Framework.Resources;
using MonoGameUtility;

#endregion

namespace Forge.Core.Airship{
    public class WeaponSystems : IDisposable{
        readonly ProjectilePhysics _engine;
        readonly int _factionId;
        readonly List<Hardpoint> _hardPoints;

        public WeaponSystems(ProjectilePhysics engine, int factionId){
            _hardPoints = new List<Hardpoint>();
            _engine = engine;
            _factionId = factionId;
        }

        public Matrix WorldTransform{
            set{
                foreach (var hardPoint in _hardPoints){
                    hardPoint.ShipTranslationMtx = value;
                }
            }
        }

        #region IDisposable Members

        public void Dispose(){
            foreach (var hardpoint in _hardPoints){
                hardpoint.Dispose();
            }
        }

        #endregion

        public void AddWeapons(IEnumerable<GameObject> weapons){
            foreach (var weapon in weapons){
                var jobj = Resource.GameObjectLoader.LoadGameObject((int) GameObjectFamily.Cannons, weapon.ObjectUid);

                long projectileUid = long.Parse(weapon.Parameters);
                float firingForce = jobj["FiringForce"].ToObject<float>();
                var projectileEmitterOffset = jobj["ProjectileEmitterOffset"].ToObject<Vector3>();
                var m = Matrix.CreateRotationY(weapon.Rotation);

                var hardPoint = new Hardpoint
                    (
                    weapon.ModelspacePosition + projectileEmitterOffset,
                    m.Backward, //DONT ASK QUESTIONS ITS MATRIXES
                    new ProjectileEmitter
                        (
                        projectileUid,
                        firingForce,
                        _factionId,
                        _engine
                        )
                    );
                _hardPoints.Add(hardPoint);
            }
        }

        public void Fire(){
            foreach (var hardpoint in _hardPoints){
                hardpoint.Fire();
            }
        }
    }
}