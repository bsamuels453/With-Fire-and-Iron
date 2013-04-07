#region

using System;
using System.Collections.Generic;
using BulletSharp;
using Forge.Framework;
using Microsoft.Xna.Framework;

#endregion

namespace Forge.Core.Logic{
    internal class ProjectilePhysics{
        #region ObjectVariant enum

        public enum ObjectVariant{
            EnemyShip,
            AllyShip
        }

        #endregion

        const float _firingForce = 5000;
        const float _shotRadius = 0.5f;
        const float _shotMass = 1;
        readonly List<BoundingObjectData> _boundingObjData;
        readonly RigidBodyConstructionInfo _defaultShotCtor;
        readonly List<RigidBody> _projectiles;
        readonly DiscreteDynamicsWorld _worldDynamics;

        public ProjectilePhysics(){
            const float gravity = -10;
            var broadphase = new DbvtBroadphase();
            var collisionConfig = new DefaultCollisionConfiguration();
            var dispatcher = new CollisionDispatcher(collisionConfig);
            var constraintSolver = new SequentialImpulseConstraintSolver();
            _worldDynamics = new DiscreteDynamicsWorld(dispatcher, broadphase, constraintSolver, collisionConfig);

            _worldDynamics.Gravity = new Vector3(0, gravity, 0);

            _boundingObjData = new List<BoundingObjectData>();
            _projectiles = new List<RigidBody>();

            var shape = new SphereShape(_shotRadius);
            var nullMotion = new DefaultMotionState(Matrix.Identity);
            _defaultShotCtor = new RigidBodyConstructionInfo(_shotMass, nullMotion, shape);
        }

        public BoundingObject AddBoundingObject(BoundingSphere[] spheres, ObjectVariant variant, Action<Vector3, Vector3> collisionCallback){
            var objInternalData = new BoundingObjectData(spheres, variant);

            var objPublicInterface = new BoundingObject(
                setObjectMatrix: matrix => objInternalData.WorldMatrix = matrix,
                terminate: () => _boundingObjData.Remove(objInternalData)
                );

            objInternalData.CollisionEventDispatcher = collisionCallback;
            _boundingObjData.Add(objInternalData);

            return objPublicInterface;
        }

        public Projectile AddProjectile(Vector3 position, Vector3 angle, ObjectVariant collisionFilter){
            var worldMatrix = Common.GetWorldTranslation(position, angle, _shotRadius*2);
            _defaultShotCtor.MotionState = new DefaultMotionState(worldMatrix);

            var body = new RigidBody(_defaultShotCtor);
            body.ApplyCentralForce(angle*_firingForce);
            _worldDynamics.AddRigidBody(body);

            _projectiles.Add(body);

            var retInterface = new Projectile(
                getPosition: () => body.CenterOfMassPosition,
                terminate: () => _projectiles.Remove(body)
                );

            return retInterface;
        }

        public void Update(double timeDelta){
            _worldDynamics.StepSimulation((float) timeDelta, 10);

            //check for collisions
            foreach (var projectileDat in _projectiles){
                foreach (var shipDat in _boundingObjData){
                    var projectilePos = projectileDat.MotionState.WorldTransform;
                    var shipPos = shipDat.WorldMatrix;

                    var translatedPos = projectilePos - shipPos;
                    var transposedPos = Matrix.Transpose(shipPos);
                    var localPos = translatedPos*transposedPos;
                    var localPosVec = localPos.Translation;

                    //it's possible that this will be parallelizable on gpu
                    bool collisionDetected = false;
                    Vector3 sphereCenter = Vector3.Zero;
                    foreach (var sphere in shipDat.BoundingSpheres){
                        if (Vector3.Distance(localPosVec, sphere.Center) < sphere.Radius){
                            sphereCenter = sphere.Center;
                            collisionDetected = true;
                            break;
                        }
                    }
                    if (collisionDetected){
                        var velocity = projectileDat.GetVelocityInLocalPoint(sphereCenter);
                        shipDat.CollisionEventDispatcher.Invoke(sphereCenter, velocity);
                    }

                    /*
                    var ship = Matrix.CreateWorld(new Vector3(-1, 0, 0), new Vector3(-1, 0, 0), Vector3.Up);
                    var projectile = Matrix.CreateWorld(new Vector3(-2, 0, -2), Vector3.Forward, Vector3.Up);

                    Vector3 v1, translation;
                    Quaternion rot;
                    ship.Decompose(out v1, out rot, out translation);
                    var matrixTrans = Matrix.CreateTranslation(translation);

                    rot.W *= -1;//reverse rotation direction
                    var revRotMatrix = Matrix.CreateFromQuaternion(rot);

                    finalProj = projectile * revRotMatrix;
                    matrixTrans = matrixTrans * revRotMatrix;
                    finalProj = finalProj - matrixTrans;
                    */
                }
            }
        }

        #region Nested type: BoundingObject

        public class BoundingObject{
            public readonly Action<Matrix> SetObjectMatrix;
            public readonly Action Terminate;

            public BoundingObject(Action<Matrix> setObjectMatrix, Action terminate){
                SetObjectMatrix = setObjectMatrix;
                Terminate = terminate;
            }
        }

        #endregion

        #region Nested type: BoundingObjectData

        class BoundingObjectData{
            public readonly BoundingSphere[] BoundingSpheres;
            public readonly ObjectVariant Type;

            /// <summary>
            ///   Position of target sphere, velocity of projectile relative to sphere Implement projectile relative speed multiplier here
            /// </summary>
            public Action<Vector3, Vector3> CollisionEventDispatcher;

            public Matrix WorldMatrix;

            public BoundingObjectData(BoundingSphere[] boundingSpheres, ObjectVariant type){
                BoundingSpheres = boundingSpheres;
                Type = type;
                WorldMatrix = Matrix.Identity;
            }
        }

        #endregion

        #region Nested type: Projectile

        public class Projectile{
            public readonly Func<Vector3> GetPosition;
            public readonly Action Terminate; //not sure when this is actually needed. might be better to do a timeout
            //public event Action<float, Vector3, Vector3> OnCollision; //theres no real reason for the projectile to care about OnCollision (yet)
            public Projectile(Func<Vector3> getPosition, Action terminate){
                GetPosition = getPosition;
                Terminate = terminate;
            }
        }

        #endregion
    }
}