using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoGameUtility;

namespace Forge.Core.Logic {
    internal class AirshipObjectIdentifier : IEquatable<AirshipObjectIdentifier> {
        public readonly ObjectType ObjectType;
        public readonly Vector3 Position;

        public AirshipObjectIdentifier(ObjectType objectType, Vector3 position) {
            ObjectType = objectType;
            Position = position;
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
