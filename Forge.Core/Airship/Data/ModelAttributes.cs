using MonoGameUtility;
using ProtoBuf;

namespace Forge.Core.Airship.Data{
    [ProtoContract]
    public struct ModelAttributes {
        [ProtoMember(1)]
        public int NumDecks;
        [ProtoMember(2)]
        public Vector3 Centroid;
        [ProtoMember(3)]
        public float Length;
        [ProtoMember(4)]
        public float Berth;
        [ProtoMember(5)]
        public float MaxAscentSpeed;
        [ProtoMember(6)]
        public float MaxForwardSpeed;
        [ProtoMember(7)]
        public float MaxReverseSpeed;
        [ProtoMember(8)]
        public float MaxTurnSpeed;

        //public float AscentAcceleration;
        //public float TurnAcceleration;
        //public float EngineAcceleration;

    }
}