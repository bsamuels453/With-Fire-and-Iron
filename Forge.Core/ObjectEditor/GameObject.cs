#region

using System;
using MonoGameUtility;
using ProtoBuf;

#endregion

namespace Forge.Core.ObjectEditor{
    [ProtoContract]
    public struct GameObject : IEquatable<ObjectIdentifier>, IEquatable<GameObject>{
        [ProtoMember(1)] public readonly int Deck;
        [ProtoMember(2)] public readonly XZPoint GridDimensions;
        [ProtoMember(3)] public readonly ObjectIdentifier Identifier;
        [ProtoMember(4)] public readonly Vector3 ModelspacePosition;
        [ProtoMember(5)] public readonly long ObjectUid;

        /// <summary>
        /// Contextual parameters for the game object. Context is based on the GameObjectType.
        /// </summary>
        [ProtoMember(9)] public readonly string Parameters;

        [ProtoMember(6)] public readonly XZPoint Position;
        [ProtoMember(7)] public readonly float Rotation;
        [ProtoMember(8)] public readonly GameObjectType Type;

        public GameObject(
            Vector3 modelspacePosition,
            int deck,
            XZPoint gridDimensions,
            long objectUid,
            GameObjectType type,
            float rotation, string parameters){
            Identifier = new ObjectIdentifier(modelspacePosition, deck);
            GridDimensions = gridDimensions;
            Position = Identifier.Origin;
            ObjectUid = objectUid;
            Deck = deck;
            Type = type;
            Rotation = rotation;
            Parameters = parameters;
            ModelspacePosition = modelspacePosition;
        }

        #region IEquatable<GameObject> Members

        public bool Equals(GameObject other){
            return Identifier.Equals(other.Identifier);
        }

        #endregion

        #region IEquatable<ObjectIdentifier> Members

        public bool Equals(ObjectIdentifier other){
            return Identifier.Equals(other);
        }

        #endregion
    }
}