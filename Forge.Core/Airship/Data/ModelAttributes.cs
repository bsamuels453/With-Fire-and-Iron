#region

using MonoGameUtility;
using ProtoBuf;

#endregion

namespace Forge.Core.Airship.Data{
    [ProtoContract]
    public struct ModelAttributes{
        [ProtoMember(1)] public float Berth;
        [ProtoMember(2)] public Vector3 Centroid;
        [ProtoMember(3)] public float Length;
        [ProtoMember(4)] public float MaxAcceleration;
        [ProtoMember(5)] public float MaxAscentAcceleration;
        [ProtoMember(6)] public float MaxAscentRate;
        [ProtoMember(7)] public float MaxForwardVelocity;
        [ProtoMember(8)] public float MaxReverseVelocity;
        [ProtoMember(9)] public float MaxTurnAcceleration;
        [ProtoMember(10)] public float MaxTurnSpeed;
        [ProtoMember(11)] public int NumDecks;
    }
}