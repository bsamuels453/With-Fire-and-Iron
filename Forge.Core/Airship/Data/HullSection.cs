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
        public HullSection(Vector3[] aliasedVertexes, Vector2[] damagemapCoords, int deck, Quadrant.Side side, int yPanel){
            AliasedVertexes = aliasedVertexes;
            Deck = deck;
            Side = side;
            YPanel = yPanel;
            DamagemapCoords = damagemapCoords;
        }

        public HullSection(){
        }

        /// <summary>
        /// The 4 vertexes that make up this hull plate. These are referred to as aliased vertexes because
        /// occasionally a hull plate will consist of more than one quad, and may have subquads or holes in it.
        /// While the aliasedVertexes will always stay the same no matter what the contents of the hullSection is,
        /// it's important to remember that they only represent where the plane on which the geometry lies.
        /// </summary>
        [ProtoMember(1)]
        public Vector3[] AliasedVertexes { get; private set; }

        /// <summary>
        /// This field represents the texture coordinates of this hull section on the damagemap texture.
        /// </summary>
        [ProtoMember(2)]
        public Vector2[] DamagemapCoords { get; private set; }

        /// <summary>
        /// The deck that this hullsection exists on.
        /// </summary>
        [ProtoMember(3)]
        public int Deck { get; private set; }

        /// <summary>
        /// The vertical panel id of this hullSection. Each deck has its own yPanel set.
        /// </summary>
        [ProtoMember(4)]
        public int YPanel { get; private set; }

        /// <summary>
        /// The side of the airship this panel is on.
        /// </summary>
        [ProtoMember(5)]
        public Quadrant.Side Side { get; private set; }

        public override int GetHashCode(){
            return 0;
        }
    }
}