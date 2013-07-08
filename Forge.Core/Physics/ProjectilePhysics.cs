#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        #region Delegates

        public delegate void CollisionCallback(int id, Vector3 intersectPos, Vector3 velocity);

        #endregion

        const float _projectileLifetime = 10000; //milliseconds
        const int _maxProjectiles = 500;
        const string _projectileShader = "Config/Shaders/TintedModel.config";
        const string _projectileList = "Config/Projectiles/ProjectileList.config";
        const float _gravity = -10;
        readonly Dictionary<ProjectileAttributes, RigidBodyConstructionInfo> _bulletCtorLookup;
        readonly List<CollisionObjectCollection> _collObjCollections;
        readonly ObjectModelBuffer<Projectile> _buffer;
        readonly Stopwatch _projectileTimer;
        readonly Dictionary<string, ProjectileAttributes> _projVariants;
        readonly List<Projectile> _activeProjectiles;

        readonly DiscreteDynamicsWorld _worldDynamics;
        bool _disposed;

        #region ctor

        public ProjectilePhysics(){
            _projectileTimer = new Stopwatch();
            _projectileTimer.Start();
            _projVariants = LoadProjectileVariants();
            _buffer = new ObjectModelBuffer<Projectile>(_maxProjectiles, _projectileShader);

            _worldDynamics = GenerateWorldDynamics();
            _worldDynamics.SetGravity(new IndexedVector3(0, _gravity, 0));
            _bulletCtorLookup = GenerateConstructorLookup(_projVariants);
            _collObjCollections = new List<CollisionObjectCollection>();
            _activeProjectiles = new List<Projectile>();
        }

        DiscreteDynamicsWorld GenerateWorldDynamics(){
            var broadphase = new DbvtBroadphase();
            var collisionConfig = new DefaultCollisionConfiguration();
            var dispatcher = new CollisionDispatcher(collisionConfig);
            var constraintSolver = new SequentialImpulseConstraintSolver();
            return new DiscreteDynamicsWorld(dispatcher, broadphase, constraintSolver, collisionConfig);
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

                    if (Vector3.Distance(projectilePos, Vector3.Zero) > shipDat.ShipSOI.Radius)
                        continue;

                    foreach (var boundingObj in shipDat.CollisionObjects){
                        bool break2 = false;
                        //fast check to see if the projectile is in same area as the object
                        foreach (var point in boundingObj.Vertexes){
                            if (Vector3.Distance(projectilePos, point) < 1f){
                                //object confirmed to be in general area
                                //now check to see if its movement path intersects the object's triangles
                                var worldPt = Common.MultMatrix(shipMtx, point);
                                var worldPtI = (IndexedVector3) worldPt;
                                var velocity = projectile.Body.GetVelocityInLocalPoint(ref worldPtI);
                                if (Math.Abs(velocity.Length() - 0) < 0.0000000001f)
                                    continue;
                                var rawvel = velocity;
                                velocity.Normalize();
                                var velocityRay = new Ray(projectilePos, velocity);

                                bool intersectionConfirmed = true; //false
                                //for now this is disabled because havent implemented a way to represent the entire projectile rather than just its central velocity vector
                                /*
                                for (int i = 0; i < boundingObj.Vertexes.Length; i += 3){
                                    float? dist;
                                    Common.RayIntersectsTriangle(
                                        ref velocityRay,
                                        ref boundingObj.Vertexes[i],
                                        ref boundingObj.Vertexes[i + 1],
                                        ref boundingObj.Vertexes[i + 2],
                                        out dist);
                                    if (dist != null){
                                        intersectionConfirmed = true;
                                        break;
                                    }
                                }
                                */
                                if (intersectionConfirmed){
                                    //xxxx these params are not correct (point transform)
                                    shipDat.BlacklistedProjectiles.Add(projectile);
                                    shipDat.CollisionEventDispatcher(boundingObj.Id, point, rawvel); //add id
                                    break2 = true;
                                }

                                break;
                            }
                        }
                        if (break2){
                            break;
                        }
                    }
                }
            }
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