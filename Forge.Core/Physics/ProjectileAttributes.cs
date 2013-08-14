#region

using System;
using Forge.Core.GameObjects;

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

        public ProjectileAttributes(GameObjectType gameObjectType){
            Model = gameObjectType.Attribute<string>(GameObjectAttr.ModelName);
            Radius = gameObjectType.Attribute<float>(GameObjectAttr.Radius);
            Mass = gameObjectType.Attribute<float>(GameObjectAttr.Mass);
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