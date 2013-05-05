#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using BulletSharp;
using Forge.Framework;
using Microsoft.Xna.Framework;
using IDisposable = System.IDisposable;

#endregion

namespace Forge.Core.Logic{
    internal class ProjectilePhysics : IDisposable{
        #region Delegates

        public delegate void CollisionCallback(Vector3 intersectPos, Vector3 velocity);

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
        readonly List<RigidBody> _projectiles;
        readonly DiscreteDynamicsWorld _worldDynamics;
        readonly Stopwatch _updateInterval;

        public ProjectilePhysics(){
            _defProjectile = new ProjectileAttributes(113832, 0.0285f, 2f);
            const float gravity = -10;
            var broadphase = new DbvtBroadphase();
            var collisionConfig = new DefaultCollisionConfiguration();
            var dispatcher = new CollisionDispatcher(collisionConfig);
            var constraintSolver = new SequentialImpulseConstraintSolver();
            _worldDynamics = new DiscreteDynamicsWorld(dispatcher, broadphase, constraintSolver, collisionConfig);

            _worldDynamics.Gravity = new Vector3(0, gravity, 0);

            _boundingObjData = new List<CollisionObjectCollection>();
            _projectiles = new List<RigidBody>();

            var shape = new SphereShape(_defProjectile.Radius);
            var nullMotion = new DefaultMotionState(Matrix.Identity);
            _defaultShotCtor = new RigidBodyConstructionInfo(_defProjectile.Mass, nullMotion, shape);
            _updateInterval = new Stopwatch();
            _updateInterval.Start();
        }

        public CollisionObjectHandle AddCollisionObject(CollisionObject[] collisionObjects, EntityVariant variant, CollisionCallback collisionCallback){
            var objInternalData = new CollisionObjectCollection(collisionObjects, variant);

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
            _defaultShotCtor.MotionState = new DefaultMotionState(worldMatrix);

            var body = new RigidBody(_defaultShotCtor);
            body.ApplyCentralForce(angle*_defProjectile.FiringForce);
            _worldDynamics.AddRigidBody(body);

            _projectiles.Add(body);

            var retInterface = new Projectile(
                getPosition: () => body.CenterOfMassPosition,
                terminate: () =>{
                               _projectiles.Remove(body);
                               _worldDynamics.RemoveRigidBody(body);
                               body.Dispose();
                           }
                );

            return retInterface;
        }

        public void Dispose(){
            foreach (var body in _projectiles) {
                _worldDynamics.RemoveRigidBody(body);
                body.Dispose();
            }
            _defaultShotCtor.Dispose();
            _worldDynamics.Dispose();
        }

        public void Update(){
            _updateInterval.Stop();
            float timeDelta = _updateInterval.ElapsedMilliseconds * 0.001f;
            _worldDynamics.StepSimulation(timeDelta, 100);
            _updateInterval.Restart();

            //check for collisions
            foreach (var projectileDat in _projectiles){
                foreach (var shipDat in _boundingObjData){
                    var projectileMtx = projectileDat.MotionState.WorldTransform;
                    var shipMtx = shipDat.WorldMatrix;

                    var invShipMtx = Matrix.Invert(shipMtx);
                    var projectilePos = Common.MultMatrix(invShipMtx, projectileMtx.Translation);

                    foreach (var boundingObj in shipDat.CollisionObjects){
                        //fast check to see if the projectile is in same area as the object
                        foreach (var point in boundingObj.IntersectPoints){
                            if (Vector3.Distance(projectilePos, point) < _defProjectile.Radius){
                                //object confirmed to be in general area
                                //now check to see if its movement path intersects the object's triangles
                                var worldPt = Common.MultMatrix(shipMtx, point);
                                var velocity = projectileDat.GetVelocityInLocalPoint(worldPt);
                                var velocityRay = new Ray(projectilePos, velocity);

                                bool intersectionConfirmed = false;
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

                                if (intersectionConfirmed){
                                    //xxxx these params are not correct (point)
                                    shipDat.CollisionEventDispatcher.Invoke(point, velocity);
                                }

                                break;
                            }
                        }
                    }
                }
            }
        }

        #region Nested type: CollisionObject

        /// <summary>
        ///   Used to define a collision object. IntersectPoints are used for fast-checking whether or not projectiles intersect this object.
        /// </summary>
        public class CollisionObject{
            public Vector3[] IntersectPoints;
            public Vector3[] Vertexes;
        }

        #endregion

        #region Nested type: CollisionObjectCollection
        /// <summary>
        /// Internal class that's used to group together collision objects, such as the plates on the side of an airship, into one class.
        /// </summary>
        class CollisionObjectCollection{
            public readonly CollisionObject[] CollisionObjects;
            public readonly EntityVariant Type;

            /// <summary>
            ///   Position of target sphere, velocity of projectile relative to sphere Implement projectile relative speed multiplier here
            /// </summary>
            public CollisionCallback CollisionEventDispatcher;

            public Matrix WorldMatrix;

            public CollisionObjectCollection(CollisionObject[] collisionObjects, EntityVariant type){
                CollisionObjects = collisionObjects;
                Type = type;
                WorldMatrix = Matrix.Identity;
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
            public readonly Action Terminate; //not sure when this is actually needed. might be better to do a timeout
            //public event Action<float, Vector3, Vector3> OnCollision; //theres no real reason for the projectile to care about OnCollision (yet)
            public Projectile(Func<Vector3> getPosition, Action terminate){
                GetPosition = getPosition;
                Terminate = terminate;
            }

            public bool Equals(Projectile other){
                //kinda hacky
                return GetPosition == other.GetPosition && Terminate == other.Terminate;
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