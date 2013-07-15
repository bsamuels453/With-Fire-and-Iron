#region

using MonoGameUtility;
using ProtoBuf;

#endregion

namespace Forge.Core.Airship.Data{
    [ProtoContract]
    public struct ModelAttributes{
        [ProtoMember(1)] public float Berth;
        [ProtoMember(2)] public Vector3 Centroid;
        [ProtoMember(3)] public float DeckHeight;
        [ProtoMember(4)] public float Depth;
        [ProtoMember(5)] public float Length;
        [ProtoMember(6)] public float MaxAcceleration;
        [ProtoMember(7)] public float MaxAscentAcceleration;
        [ProtoMember(8)] public float MaxAscentRate;
        [ProtoMember(9)] public float MaxForwardVelocity;
        [ProtoMember(10)] public float MaxReverseVelocity;
        [ProtoMember(11)] public float MaxTurnAcceleration;
        [ProtoMember(12)] public float MaxTurnSpeed;
        [ProtoMember(13)] public int NumDecks;
    }
}