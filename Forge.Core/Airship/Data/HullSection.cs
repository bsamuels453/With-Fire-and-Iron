#region

using MonoGameUtility;
using ProtoBuf;

#endregion

namespace Forge.Core.Airship.Data{
    /// <summary>
    /// This class is used to provide metadata about hull geometry. One hullsection represents a plate
    /// that makes up the hull of the airship. This is going to be used mostly for metadata and collision stuff.
    /// </summary>
    [ProtoContract]
    public class HullSection{
        /// <summary>
        /// The 4 vertexes that make up this hull plate. These are referred to as aliased vertexes because
        /// occasionally a hull plate will consist of more than one quad, and may have subquads or holes in it.
        /// While the aliasedVertexes will always stay the same no matter what the contents of the hullSection is,
        /// it's important to remember that they only represent where the plane on which the geometry lies.
        /// </summary>
        [ProtoMember(1)] public readonly Vector3[] AliasedVertexes; 

        /// <summary>
        /// This field represents the texture coordinates of this hull section on the damagemap texture.
        /// </summary>
        [ProtoMember(2)] public readonly Vector2[] DamagemapCoords;

        /// <summary>
        /// The side of the airship this panel is on.
        /// </summary>
        [ProtoMember(3)] public readonly Quadrant.Side Side;

        public HullSection(Vector3[] aliasedVertexes, Vector2[] damagemapCoords, Quadrant.Side side){
            AliasedVertexes = aliasedVertexes;
            Side = side;
            DamagemapCoords = damagemapCoords;
        }

        public HullSection(){
        }

        public override int GetHashCode(){
            //the top left of each hullsection is garaunteed to be different for each section
            int hash = (int) ((DamagemapCoords[0].X*1000) + (DamagemapCoords[0].Y*1000000));
            return hash;
        }
    }
}