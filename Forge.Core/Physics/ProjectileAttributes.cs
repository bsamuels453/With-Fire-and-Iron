#region

using System;
using Newtonsoft.Json.Linq;

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

        public ProjectileAttributes(JObject jobj){
            Model = jobj["Model"].ToObject<string>();
            Radius = jobj["Radius"].ToObject<float>();
            Mass = jobj["Mass"].ToObject<float>();
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