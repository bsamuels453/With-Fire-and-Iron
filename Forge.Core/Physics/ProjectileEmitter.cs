#region

using Forge.Core.GameObjects;
using Forge.Core.ObjectEditor;
using Forge.Framework.Resources;
using MonoGameUtility;

#endregion

namespace Forge.Core.Physics{
    public class ProjectileEmitter{
        readonly ProjectileAttributes _attributes;
        readonly int _factionId;
        readonly float _firingForce;
        readonly ProjectilePhysics _projectileEngine;

        public ProjectileEmitter(long projectileUid, float firingForce, int factionId, ProjectilePhysics projectileEngine){
            var jobj = Resource.GameObjectLoader.LoadGameObject((int) GameObjectFamily.Projectiles, projectileUid);
            _attributes = new ProjectileAttributes(jobj);
            _firingForce = firingForce;
            _projectileEngine = projectileEngine;
            _factionId = factionId;
        }

        public void CreateProjectile(Vector3 position, Vector3 firingdirection){
            _projectileEngine.AddProjectile
                (
                    _attributes,
                    position,
                    firingdirection,
                    _factionId,
                    _firingForce
                );
        }
    }
}