namespace Forge.Core.Physics{
    /// <summary>
    ///   This class is used to define the attributes of each projectile.
    /// </summary>
    public struct ProjectileAttributes{
        public readonly float Mass;
        public readonly string Model;
        public readonly float ModelScale;
        public readonly float Radius;

        public ProjectileAttributes(string model, float radius, float mass, float modelScale){
            Model = model;
            Radius = radius;
            Mass = mass;
            ModelScale = modelScale;
        }
    }
}