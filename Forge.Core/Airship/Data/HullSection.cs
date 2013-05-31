#region

using System;
using System.Diagnostics;
using Forge.Framework.Draw;
using MonoGameUtility;
using ProtoBuf;

#endregion

namespace Forge.Core.Airship.Data{
    [ProtoContract]
    internal class HullSection : IEquatable<HullSection>{
        Action _hideSection;
        Action _unhideSection;

        public HullSection(int uid, Vector3[] aliasedVertexes, HullSectionIdentifier identifier, ObjectBuffer<int> hullBuffer){
            Uid = uid;
            AliasedVertexes = aliasedVertexes;
            Deck = identifier.Deck;
            Side = identifier.Side;
            YPanel = identifier.YPanel;
            _hideSection = () => hullBuffer.DisableObject(uid);
            _unhideSection = () => hullBuffer.EnableObject(uid);
        }

        public HullSection(int uid, Vector3[] aliasedVertexes, int deck, Quadrant.Side side, int yPanel, ObjectBuffer<int> hullBuffer){
            Uid = uid;
            AliasedVertexes = aliasedVertexes;
            Deck = deck;
            Side = side;
            YPanel = yPanel;
            _hideSection = () => hullBuffer.DisableObject(uid);
            _unhideSection = () => hullBuffer.EnableObject(uid);
        }

        public HullSection(){
        }

        [ProtoMember(1)]
        public int Uid { get; private set; }

        [ProtoMember(2)]
        public Vector3[] AliasedVertexes { get; private set; }

        [ProtoMember(3)]
        public int Deck { get; private set; }

        [ProtoMember(4)]
        public int YPanel { get; private set; }

        [ProtoMember(5)]
        public Quadrant.Side Side { get; private set; }

        #region IEquatable<HullSection> Members

        public bool Equals(HullSection other){
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Uid == Uid;
        }

        #endregion

        public void SetDelegates(ObjectBuffer<int> hullBuffer){
            Debug.Assert(_hideSection == null && _unhideSection == null);
            _hideSection = () => hullBuffer.DisableObject(Uid);
            _unhideSection = () => hullBuffer.EnableObject(Uid);
        }

        public void Hide(){
            _hideSection.Invoke();
        }

        public void UnHide(){
            _unhideSection.Invoke();
        }

        public override bool Equals(object obj){
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