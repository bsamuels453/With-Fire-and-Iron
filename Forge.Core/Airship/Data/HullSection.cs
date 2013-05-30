using System;
using Forge.Framework.Draw;
using Microsoft.Xna.Framework;

namespace Forge.Core.Airship.Data {
    class HullSection : IEquatable<HullSection> {
        public int Uid { get; private set; }
        public Vector3[] AliasedVertexes { get; private set; }
        public int Deck { get; private set; }
        public int YPanel { get; private set; }
        public Quadrant.Side Side { get; private set; }

        readonly Action _hideSection;
        readonly Action _unhideSection;

        public HullSection(int uid, Vector3[] aliasedVertexes, HullSectionIdentifier identifier, ObjectBuffer<int> hullBuffer){
            Uid = uid;
            AliasedVertexes = aliasedVertexes;
            Deck = identifier.Deck;
            Side = identifier.Side;
            YPanel = identifier.YPanel;
            _hideSection = () => hullBuffer.DisableObject(uid);
            _unhideSection = () => hullBuffer.EnableObject(uid);
        }

        public HullSection(int uid, Vector3[] aliasedVertexes, int deck, Quadrant.Side side, int yPanel, ObjectBuffer<int> hullBuffer) {
            Uid = uid;
            AliasedVertexes = aliasedVertexes;
            Deck = deck;
            Side = side;
            YPanel = yPanel;
            _hideSection = () => hullBuffer.DisableObject(uid);
            _unhideSection = () => hullBuffer.EnableObject(uid);
        }

        public void Hide(){
            _hideSection.Invoke();
        }

        public void UnHide(){
            _unhideSection.Invoke();
        }

        public bool Equals(HullSection other){
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Uid == Uid;
        }
        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (HullSection)) return false;
            return Equals((HullSection) obj);
        }

        public override int GetHashCode(){
            unchecked{
                return Uid;
            }
        }
    }
}
