#region

using System;
using Forge.Core.GameObjects;
using Forge.Core.GameObjects.Statistics;

#endregion

namespace Forge.Core.Physics{
    /// <summary>
    ///   This class is used to define the attributes of each projectile.
    /// </summary>
    public struct ProjectileAttributes : IEquatable<ProjectileAttributes>{
        public readonly float Mass;
        public readonly string Model;
        public readonly float Radius;

        public ProjectileAttributes(string model, float radius, float mass){
            Model = model;
            Radius = radius;
            Mass = mass;
        }

        public ProjectileAttributes(GameObjectFamily family, long uid){
            Model = ObjectStatisticProvider.GetModelString(family, uid);
            Radius = ObjectStatisticProvider.GetRadius(family, uid);
            Mass = ObjectStatisticProvider.GetMass(family, uid);
        }

        #region IEquatable<ProjectileAttributes> Members

        public bool Equals(ProjectileAttributes other){
            if (Mass == other.Mass &&
                Model.Equals(other.Model) &&
                    Radius == other.Radius){
                return true;
            }
            return false;
        }

        #endregion
    }
}