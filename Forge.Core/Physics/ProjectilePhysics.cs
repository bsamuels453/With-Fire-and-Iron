#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BulletXNA;
using BulletXNA.BulletCollision;
using BulletXNA.BulletDynamics;
using BulletXNA.LinearMath;
using Forge.Framework;
using Forge.Framework.Draw;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;

#endregion

namespace Forge.Core.Physics{
    public class ProjectilePhysics : IDisposable{
        const float _projectileLifetime = 10000; //milliseconds
        const int _maxProjectiles = 500;
        const string _projectileShader = "Config/Shaders/TintedModel.config";
        const string _projectileList = "Config/Projectiles/ProjectileList.config";
        const float _gravity = -10;
        const float _shieldThickness = 3;
        readonly List<Projectile> _activeProjectiles;
        readonly ObjectModelBuffer<Projectile> _buffer;
        readonly Dictionary<ProjectileAttributes, RigidBodyConstructionInfo> _bulletCtorLookup;
        readonly List<CollisionObjectCollection> _collObjCollections;
        readonly DebugDraw _debugDraw;
        readonly Dictionary<string, ProjectileAttributes> _projVariants;
        readonly Stopwatch _projectileTimer;
        readonly List<RigidBody> _reflectionShields;
        readonly RigidBodyConstructionInfo _shieldCtor;

        readonly DiscreteDynamicsWorld _worldDynamics;
        bool _disposed;

        #region ctor

        public ProjectilePhysics(){
            _projectileTimer = new Stopwatch();
            _projectileTimer.Start();
            _projVariants = LoadProjectileVariants();
            _buffer = new ObjectModelBuffer<Projectile>(_maxProjectiles, _projectileShader);
            _reflectionShields = new List<RigidBody>();

            _worldDynamics = GenerateWorldDynamics(_debugDraw = new DebugDraw());
            _worldDynamics.SetGravity(new IndexedVector3(0, _gravity, 0));
            _bulletCtorLookup = GenerateConstructorLookup(_projVariants);
            _collObjCollections = new List<CollisionObjectCollection>();
            _activeProjectiles = new List<Projectile>();
            _shieldCtor = GenerateShieldCtor();
        }

        DiscreteDynamicsWorld GenerateWorldDynamics(DebugDraw debugDraw){
            var broadphase = new DbvtBroadphase();
            var collisionConfig = new DefaultCollisionConfiguration();
            var dispatcher = new CollisionDispatcher(collisionConfig);
            var constraintSolver = new SequentialImpulseConstraintSolver();
            var dynamics = new DiscreteDynamicsWorld(dispatcher, broadphase, constraintSolver, collisionConfig);
            dynamics.SetDebugDrawer(debugDraw);

            return dynamics;
        }

        RigidBodyConstructionInfo GenerateShieldCtor(){
            var shape = new BoxShape(new IndexedVector3(3, 3, _shieldThickness));
            var nullMotion = new DefaultMotionState();
            var ctor = new RigidBodyConstructionInfo(0, nullMotion, shape);
            return ctor;
        }

        Dictionary<ProjectileAttributes, RigidBodyConstructionInfo> GenerateConstructorLookup(Dictionary<string, ProjectileAttributes> projectileVariants){
            var ret = new Dictionary<ProjectileAttributes, RigidBodyConstructionInfo>(projectileVariants.Count);

            foreach (var variant in projectileVariants){
                var shape = new SphereShape(variant.Value.Radius);
                var nullMotion = new DefaultMotionState(); //new DefaultMotionState(Matrix.Identity);
                var ctor = new RigidBodyConstructionInfo(variant.Value.Mass, nullMotion, shape);
                ret.Add(variant.Value, ctor);
            }
            return ret;
        }

        Dictionary<string, ProjectileAttributes> LoadProjectileVariants(){
            var projectileVariants = new Dictionary<string, ProjectileAttributes>(0);

            var projectileDefList = Resource.LoadConfig(_projectileList);
            foreach (var file in projectileDefList){
                var jObj = Resource.LoadConfig(file.Value.ToObject<string>());

                var attributes = new ProjectileAttributes(jObj);

                projectileVariants.Add(file.Key, attributes);
            }
            return projectileVariants;
        }

        #endregion

        #region IDisposable Members

        public void Dispose(){
            Debug.Assert(!_disposed);
            foreach (var projectile in _activeProjectiles){
                _worldDynamics.RemoveRigidBody(projectile.Body);
                projectile.Body.Cleanup();
            }
            //dont have to clean up construction info?

            _worldDynamics.Cleanup();
            _buffer.Dispose();
            _debugDraw.Dispose();
            _disposed = true;
        }

        #endregion

        ~ProjectilePhysics(){
            if (!_disposed){
                throw new ResourceNotDisposedException();
            }
        }

        public
            void AddProjectile
            (
            ProjectileAttributes projectileVariant,
            Vector3 position,
            Vector3 aimDir,
            int factionId,
            float firingForce){
            _projectileTimer.Stop();
            long creationTime = _projectileTimer.ElapsedMilliseconds;
            _projectileTimer.Start();

            var worldTransform = Common.GetWorldTranslation(position, aimDir, projectileVariant.Radius*2);
            var ctor = _bulletCtorLookup[projectileVariant];
            ctor.m_motionState = new DefaultMotionState(new IndexedMatrix(worldTransform), IndexedMatrix.Identity);

            var body = new RigidBody(ctor);
            var forceI = (IndexedVector3) aimDir*firingForce;
            body.ApplyCentralForce(ref forceI);
            _worldDynamics.AddRigidBody(body);

            var projectile = new Projectile
                (
                body,
                getPosition: () => body.GetCenterOfMassPosition(),
                terminate: proj =>{
                               _activeProjectiles.Remove(proj);
                               _worldDynamics.RemoveRigidBody(body);
                               _buffer.RemoveObject(proj);
                               body.Cleanup();
                               foreach (var collisionObjectCollection in _collObjCollections){
                                   collisionObjectCollection.BlacklistedProjectiles.Remove(proj);
                               }
                           },
                creationTime: creationTime
                );

            _activeProjectiles.Add(projectile);

            if (_activeProjectiles.Count > _maxProjectiles){
                throw new Exception("IMPLEMENT PROJECTILE CIRCULAR BUFFER");
            }

            var translation = Matrix.CreateTranslation(position);
            _buffer.AddObject(projectile, Resource.LoadContent<Model>(projectileVariant.Model), translation);
        }

        public
            CollisionObjectHandle AddCollisionObjectCollection
            (
            CollisionObject[] collisionObjects,
            BoundingSphere soi,
            int factionId,
            CollisionCallback collisionCallback){
            var collection = new CollisionObjectCollection(collisionObjects, factionId, soi, collisionCallback);
            var handle = new CollisionObjectHandle
                (
                setObjectMatrix: matrix => collection.WorldTransform = matrix,
                terminate: () => _collObjCollections.Remove(collection)
                );

            _collObjCollections.Add(collection);
            return handle;
        }

        public void Update(double timeDelta){
#if PROFILE_PHYSICS
            var sw = new Stopwatch();
            sw.Start();
#endif
            float timeDeltaSec = (float) timeDelta/1000f;
            _worldDynamics.StepSimulation(20/1000f, 1);
            UpdateCollisions();

#if PROFILE_PHYSICS
            sw.Stop();
            DebugConsole.WriteLine("Active: " + _projectiles.Count);
            DebugConsole.WriteLine("Physics loop: "+sw.ElapsedMilliseconds + " ms");
#endif

            UpdateProjectilePositions();
            CleanupExpiredProjectiles();
            _debugDraw.Clear();
            _worldDynamics.DebugDrawWorld();
        }

        void UpdateCollisions(){
            //check for collisions
            foreach (var projectile in _activeProjectiles){
                foreach (var shipDat in _collObjCollections){
                    //make sure this projectile is allowed to collide with this ship
                    if (shipDat.BlacklistedProjectiles.Contains(projectile)){
                        continue;
                    }

                    IndexedMatrix projectileMtxI;

                    projectile.Body.GetMotionState().GetWorldTransform(out projectileMtxI);
                    var projectileMtx = (Matrix) projectileMtxI;
                    var shipMtx = shipDat.WorldTransform;

                    var invShipMtx = Matrix.Invert(shipMtx);
                    var projectilePos = Common.MultMatrix(invShipMtx, (projectileMtx).Translation);

                    if (Vector3.Distance(projectilePos, Vector3.Zero) > shipDat.ShipSOI.Radius){
                        continue;
                    }

                    var inRangeObjects = from obj in shipDat.CollisionObjects
                        where obj.IsInRange(projectilePos, 4.5f)
                        select obj;

                    foreach (var obj in inRangeObjects){
                        //object confirmed to be in general area
                        //now check to see if its movement path intersects the object's triangles
                        var worldPt = Common.MultMatrix(shipMtx, obj.Centroid);
                        var worldPtI = (IndexedVector3) worldPt;
                        var velocity = projectile.Body.GetVelocityInLocalPoint(ref worldPtI);

                        //need a big mess of crap to translate velocity from world to object space
                        //possible optimize: cache worldToObj matrix
                        var untranslatedVelMtx = Common.GetWorldTranslation(velocity, new Vector3(0, 0, 0), 0);
                        var untranslatedVel = untranslatedVelMtx.Translation;
                        var translatedAngle = TranslateAngle(untranslatedVel, invShipMtx);

                        var velocityRay = new Ray(projectilePos, translatedAngle);
                        /*
                        var originalPos = Common.MultMatrix(shipMtx, projectilePos);
                        var originalPos2 = Common.MultMatrix(shipMtx, projectilePos + velocityRay.Direction);
                        _debugDraw.DrawLineImmediate(originalPos,originalPos2) ;
                         */
                        bool intersectionConfirmed = false; //false
                        //for now this is disabled because havent implemented a way to represent the entire projectile rather than just its central velocity vector
                        for (int i = 0; i < obj.Vertexes.Length; i += 3){
                            float? dist;
                            Common.RayIntersectsTriangle
                                (
                                    ref velocityRay,
                                    ref obj.Vertexes[i],
                                    ref obj.Vertexes[i + 1],
                                    ref obj.Vertexes[i + 2],
                                    out dist);
                            if (dist != null){
                                intersectionConfirmed = true;
                                break;
                            }
                        }

                        if (intersectionConfirmed){
                            /*
                            _debugDraw.DrawLineImmediate(Common.MultMatrix(shipMtx, obj.Vertexes[0]*1.01f), Common.MultMatrix(shipMtx, obj.Vertexes[1]*1.01f));
                            _debugDraw.DrawLineImmediate(Common.MultMatrix(shipMtx, obj.Vertexes[0]*1.01f), Common.MultMatrix(shipMtx, obj.Vertexes[2]*1.01f));
                            _debugDraw.DrawLineImmediate(Common.MultMatrix(shipMtx, obj.Vertexes[1]*1.01f), Common.MultMatrix(shipMtx, obj.Vertexes[2]*1.01f));
                            _debugDraw.DrawLineImmediate(Common.MultMatrix(shipMtx, projectilePos), Common.MultMatrix(shipMtx, projectilePos + velocityRay.Direction*5));
                            */
                            Vector3 intersectPt = Common.MultMatrix(shipMtx, obj.Centroid);

                            ApplyCollision
                                (
                                    projectile,
                                    shipDat,
                                    obj,
                                    TranslateAngle(obj.Normal, shipMtx),
                                    new Ray(obj.Centroid, translatedAngle),
                                    new Ray(intersectPt, untranslatedVel)
                                );
                            break;
                        }
                    }
                }
            }
        }

        Vector3 TranslateAngle(Vector3 angle, Matrix translationMtx){
            Vector3 _, __;
            Quaternion q;
            translationMtx.Decompose(out _, out q, out __);
            var worldToObj = Matrix.CreateFromQuaternion(q);
            var translatedAngle = Common.MultMatrix(worldToObj, angle);
            return translatedAngle;
        }

        void ApplyCollision(
            Projectile projectile,
            CollisionObjectCollection collection,
            CollisionObject collidee,
            Vector3 collideeNormal,
            Ray localCollideRay,
            Ray globalCollideRay){
            collection.BlacklistedProjectiles.Add(projectile);
            collection.CollisionEventDispatcher
                (
                    collidee.Id,
                    collidee.Centroid,
                    localCollideRay,
                    globalCollideRay
                );

            var shield = new RigidBody(_shieldCtor);

            var state = Matrix.CreateWorld(globalCollideRay.Position - collideeNormal*_shieldThickness, collideeNormal, Vector3.Up);
            var motion = new DefaultMotionState(state, IndexedMatrix.Identity);

            _reflectionShields.Add(shield);
            _worldDynamics.AddRigidBody(shield);

            shield.SetMotionState(motion);
        }

        void UpdateProjectilePositions(){
            foreach (var projectile in _activeProjectiles){
                var translation = Matrix.CreateTranslation(projectile.GetPosition());
                _buffer.SetObjectTransform(projectile, translation);
            }
        }

        void CleanupExpiredProjectiles(){
            _projectileTimer.Stop();
            long timeIndex = _projectileTimer.ElapsedMilliseconds;
            _projectileTimer.Start();

            var projectilesToTerminate = new List<Projectile>();

            foreach (var projectile in _activeProjectiles){
                if (timeIndex - projectile.TimeCreationIndex > _projectileLifetime){
                    projectilesToTerminate.Add(projectile);
                }
            }
            foreach (var projectile in projectilesToTerminate){
                projectile.Terminate();
            }
        }
    }
}