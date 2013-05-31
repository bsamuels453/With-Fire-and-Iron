//#define PROFILE_PHYSICS

#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using BulletXNA;
using BulletXNA.BulletCollision;
using BulletXNA.BulletDynamics;
using BulletXNA.LinearMath;
using Forge.Framework;
using MonoGameUtility;
using IDisposable = System.IDisposable;

#endregion

namespace Forge.Core.Physics{
    internal class ProjectilePhysics : IDisposable{
        #region Delegates

        public delegate void CollisionCallback(int id, Vector3 intersectPos, Vector3 velocity);

        #endregion

        #region EntityVariant enum

        public enum EntityVariant{
            EnemyShip,
            AllyShip
        }

        #endregion

        ProjectileAttributes _defProjectile;

        readonly List<CollisionObjectCollection> _boundingObjData;
        readonly RigidBodyConstructionInfo _defaultShotCtor;
        readonly List<Projectile> _projectiles;
        readonly DiscreteDynamicsWorld _worldDynamics;

        public ProjectilePhysics(){
            _defProjectile = new ProjectileAttributes(832, 0.0285f, 2f);
            const float gravity = -10;
            var broadphase = new DbvtBroadphase();
            var collisionConfig = new DefaultCollisionConfiguration();
            var dispatcher = new CollisionDispatcher(collisionConfig);
            var constraintSolver = new SequentialImpulseConstraintSolver();
            _worldDynamics = new DiscreteDynamicsWorld(dispatcher, broadphase, constraintSolver, collisionConfig);

            //_worldDynamics.Gravity = new Vector3(0, gravity, 0);
            _worldDynamics.SetGravity(new IndexedVector3(0, gravity, 0));

            _boundingObjData = new List<CollisionObjectCollection>();
            _projectiles = new List<Projectile>();

            var shape = new SphereShape(_defProjectile.Radius);
            var nullMotion = new DefaultMotionState(); //new DefaultMotionState(Matrix.Identity);
            _defaultShotCtor = new RigidBodyConstructionInfo(_defProjectile.Mass, nullMotion, shape);
        }

        public CollisionObjectHandle AddShipCollisionObjects(CollisionObject[] collisionObjects, BoundingSphere soi,  EntityVariant variant, CollisionCallback collisionCallback){
            var objInternalData = new CollisionObjectCollection(collisionObjects, variant, soi);

            var objPublicInterface = new CollisionObjectHandle(
                setObjectMatrix: matrix => objInternalData.WorldMatrix = matrix,
                terminate: () => _boundingObjData.Remove(objInternalData)
                );

            objInternalData.CollisionEventDispatcher = collisionCallback;
            _boundingObjData.Add(objInternalData);

            return objPublicInterface;
        }

        public Projectile AddProjectile(Vector3 position, Vector3 angle, EntityVariant collisionFilter){
            var worldMatrix = Common.GetWorldTranslation(position, angle, _defProjectile.Radius*2);
            //_defaultShotCtor.MotionState = new DefaultMotionState(worldMatrix);
            _defaultShotCtor.m_motionState = new DefaultMotionState(new IndexedMatrix(worldMatrix), IndexedMatrix.Identity);

            var body = new RigidBody(_defaultShotCtor);
            body.ApplyCentralForce(angle*_defProjectile.FiringForce);
            _worldDynamics.AddRigidBody(body);

            var projectile = new Projectile(
                body,
                getPosition: () => body.CenterOfMassPosition,
                terminate: (proj) =>{
                               _projectiles.Remove(proj);
                               _worldDynamics.RemoveRigidBody(body);
                               body.Dispose();
                               foreach (var collisionObjectCollection in _boundingObjData){
                                   collisionObjectCollection.BlacklistedProjectiles.Remove(proj);
                               }
                           }
                );

            _projectiles.Add(projectile);
            return projectile;
        }

        bool _disposed;

        public void Dispose(){
            Debug.Assert(!_disposed);
            foreach (var projectile in _projectiles) {
                _worldDynamics.RemoveRigidBody(projectile.Body);
                projectile.Body.Dispose();
            }
            _defaultShotCtor.Dispose();
            _worldDynamics.Dispose();
            _disposed = true;
        }

        ~ProjectilePhysics(){
            if (!_disposed)
                throw new ResourceNotDisposedException();
        }

        public void Update(double timeDelta){
#if PROFILE_PHYSICS
            var sw = new Stopwatch();
            sw.Start();
#endif
            float timeDeltaSec = (float)timeDelta / 1000f;
            _worldDynamics.StepSimulation(timeDeltaSec, 100);

            //check for collisions
            foreach (var projectile in _projectiles){
                
                foreach (var shipDat in _boundingObjData){
                    //make sure this projectile is allowed to collide with this ship
                    if(shipDat.BlacklistedProjectiles.Contains(projectile)){
                        continue;
                    }


                    var projectileMtx = projectile.Body.MotionState.WorldTransform;
                    var shipMtx = shipDat.WorldMatrix;

                    var invShipMtx = Matrix.Invert(shipMtx);
                    var projectilePos = Common.MultMatrix(invShipMtx, projectileMtx.Translation);

                    if (Vector3.Distance(projectilePos, Vector3.Zero) > shipDat.ShipSOI.Radius)
                        continue;

                    foreach (var boundingObj in shipDat.CollisionObjects){
                        //fast check to see if the projectile is in same area as the object
                        foreach (var point in boundingObj.Vertexes){
                            if (Vector3.Distance(projectilePos, point) < 1f) {
                                //object confirmed to be in general area
                                //now check to see if its movement path intersects the object's triangles
                                var worldPt = Common.MultMatrix(shipMtx, point);
                                var velocity = projectile.Body.GetVelocityInLocalPoint(worldPt);
                                if (velocity.Length() == 0)
                                    continue;
                                var rawvel = velocity;
                                velocity.Normalize();
                                var velocityRay = new Ray(projectilePos, velocity);

                                bool intersectionConfirmed = true;//false
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
                                    shipDat.CollisionEventDispatcher.Invoke(boundingObj.Id, point, rawvel);//add id
                                }

                                break;
                            }
                        }
                    }
                }
                
            }
#if PROFILE_PHYSICS
            sw.Stop();
            DebugConsole.WriteLine("Active: " + _projectiles.Count);
            DebugConsole.WriteLine("Physics loop: "+sw.ElapsedMilliseconds + " ms");
#endif
        }

        #region Nested type: CollisionObject

        /// <summary>
        ///   Used to define a collision object. IntersectPoints are used for fast-checking whether or not projectiles intersect this object.
        /// </summary>
        public struct CollisionObject{
            public int Id;
            public Vector3[] Vertexes;

            public CollisionObject(int id, Vector3[] vertexes){
                Id = id;
                Vertexes = vertexes;
                Debug.Assert(vertexes.Length == 3);
            }
        }

        #endregion

        #region Nested type: CollisionObjectCollection
        /// <summary>
        /// Internal class that's used to group together collision objects, such as the plates on the side of an airship, into one class.
        /// </summary>
        class CollisionObjectCollection{
            public readonly BoundingSphere ShipSOI;
            public readonly CollisionObject[] CollisionObjects;
            public readonly EntityVariant Type;
            public readonly List<Projectile> BlacklistedProjectiles; 

            /// <summary>
            ///   Position of target sphere, velocity of projectile relative to sphere Implement projectile relative speed multiplier here
            /// </summary>
            public CollisionCallback CollisionEventDispatcher;

            public Matrix WorldMatrix;

            public CollisionObjectCollection(CollisionObject[] collisionObjects, EntityVariant type, BoundingSphere soi){
                CollisionObjects = collisionObjects;
                Type = type;
                WorldMatrix = Matrix.Identity;
                ShipSOI = soi;
                BlacklistedProjectiles = new List<Projectile>();
            }
        }

        #endregion

        #region Nested type: CollisionObjectHandle
        
        /// <summary>
        /// Returned on the creation of a collision object. This class allows external classes to manipulate the collision object it created.
        /// </summary>
        public class CollisionObjectHandle{
            public readonly Action<Matrix> SetObjectMatrix;
            public readonly Action Terminate;

            public CollisionObjectHandle(Action<Matrix> setObjectMatrix, Action terminate){
                SetObjectMatrix = setObjectMatrix;
                Terminate = terminate;
            }
        }

        #endregion

        #region Nested type: Projectile
        /// <summary>
        /// Returned on the creation of a projectile. This class is a handle used by external gamestates to manipulate and read data on the projectile it created.
        /// </summary>
        public class Projectile : IEquatable<Projectile> {
            //why these delegates? remove them later
            public readonly Func<Vector3> GetPosition;
            readonly Action<Projectile> _terminate; //not sure when this is actually needed. might be better to do a timeout
            //public event Action<float, Vector3, Vector3> OnCollision; //theres no real reason for the projectile to care about OnCollision (yet)
            public readonly RigidBody Body;
            public Projectile(RigidBody body, Func<Vector3> getPosition, Action<Projectile> terminate){
                Body = body;
                GetPosition = getPosition;
                _terminate = terminate;
            }

            public void Terminate(){
                _terminate.Invoke(this);
            }

            public bool Equals(Projectile other){
                //kinda hacky
                return _terminate == other._terminate;
            }
        }

        #endregion

        /// <summary>
        /// This class is used to define the attributes of each projectile.
        /// </summary>
        public struct ProjectileAttributes{
            public readonly float FiringForce;
            public readonly float Radius;
            public readonly float Mass;
            public ProjectileAttributes(float firingForce, float radius, float mass){
                FiringForce = firingForce;
                Radius = radius;
                Mass = mass;
            }
        }
    }
}