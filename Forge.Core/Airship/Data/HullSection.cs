#region

using MonoGameUtility;
using ProtoBuf;

#endregion

namespace Forge.Core.Airship.Data{
    [ProtoContract]
    public class HullSection{
        public HullSection(Vector3[] aliasedVertexes, HullSectionIdentifier identifier){
            AliasedVertexes = aliasedVertexes;
            Deck = identifier.Deck;
            Side = identifier.Side;
            YPanel = identifier.YPanel;
        }

        public HullSection(Vector3[] aliasedVertexes, int deck, Quadrant.Side side, int yPanel){
            AliasedVertexes = aliasedVertexes;
            Deck = deck;
            Side = side;
            YPanel = yPanel;
        }

        public HullSection(){
        }

        [ProtoMember(1)]
        public Vector3[] AliasedVertexes { get; private set; }

        [ProtoMember(2)]
        public int Deck { get; private set; }

        [ProtoMember(3)]
        public int YPanel { get; private set; }

        [ProtoMember(4)]
        public Quadrant.Side Side { get; private set; }
    }
}