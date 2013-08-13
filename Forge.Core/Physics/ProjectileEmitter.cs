#region

using Forge.Core.GameObjects;
using MonoGameUtility;

#endregion

namespace Forge.Core.Physics{
    public class ProjectileEmitter{
        readonly ProjectileAttributes _attributes;
        readonly int _factionId;
        readonly float _firingForce;
        readonly ProjectilePhysics _projectileEngine;

        public ProjectileEmitter(int projectileUid, float firingForce, int factionId, ProjectilePhysics projectileEngine){
            _attributes = new ProjectileAttributes(new GameObjectType(GameObjectFamily.Projectiles, projectileUid));
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