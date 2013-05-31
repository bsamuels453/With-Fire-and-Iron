using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoGameUtility;
using ProtoBuf;

namespace Forge.Core.Logic {
    [ProtoContract]
    internal class AirshipObjectIdentifier : IEquatable<AirshipObjectIdentifier> {
        [ProtoMember(1)]
        public readonly ObjectType ObjectType;
        [ProtoMember(2)]
        public readonly Vector3 Position;

        public AirshipObjectIdentifier(ObjectType objectType, Vector3 position) {
            ObjectType = objectType;
            Position = position;
        }

        public AirshipObjectIdentifier(){

        }

        #region IEquatable<AirshipObjectIdentifier> Members

        public bool Equals(AirshipObjectIdentifier other) {
            return ObjectType == other.ObjectType && Position == other.Position;
        }

        #endregion
    }

    internal enum ObjectType {
        Ladder,
        Deckboard,
        Misc
    }
}
