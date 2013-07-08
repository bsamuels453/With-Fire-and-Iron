﻿#region

using System;
using BulletXNA.BulletDynamics;
using MonoGameUtility;

#endregion

namespace Forge.Core.Physics{
    public class Projectile : IEquatable<Projectile>{
        //god help us if we ever run out of these uids
        static long _maxUid;

        public readonly RigidBody Body;
        public readonly Func<Vector3> GetPosition;
        public readonly long Uid;
        readonly Action<Projectile> _terminate; //not sure when this is actually needed. might be better to do a timeout
        //public event Action<float, Vector3, Vector3> OnCollision; //theres no real reason for the projectile to care about OnCollision (yet)

        public Projectile(RigidBody body, Func<Vector3> getPosition, Action<Projectile> terminate){
            Body = body;
            GetPosition = getPosition;
            _terminate = terminate;
            Uid = _maxUid;
            _maxUid++;
        }

        #region IEquatable<Projectile> Members

        public bool Equals(Projectile other){
            return Uid == other.Uid;
        }

        #endregion

        public void Terminate(){
            _terminate.Invoke(this);
        }
    }
}