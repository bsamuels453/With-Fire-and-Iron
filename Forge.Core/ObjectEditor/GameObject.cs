#region

using System;
using MonoGameUtility;

#endregion

namespace Forge.Core.ObjectEditor{
    internal struct GameObject : IEquatable<ObjectIdentifier>{
        public readonly int Deck;
        public readonly XZPoint GridDimensions;
        public readonly ObjectIdentifier Identifier;
        public readonly long ObjectUid;
        public readonly XZPoint Position;
        public readonly GameObjectEnvironment.SideEffect SideEffect;
        public readonly GameObjectType Type;

        public GameObject(
            ObjectIdentifier identifier,
            XZPoint gridDimensions,
            XZPoint gridPosition,
            GameObjectEnvironment.SideEffect sideEffect,
            long objectUid,
            int deck, GameObjectType type){
            Identifier = identifier;
            GridDimensions = gridDimensions;
            Position = gridPosition;
            SideEffect = sideEffect;
            ObjectUid = objectUid;
            Deck = deck;
            Type = type;
        }

        #region IEquatable<ObjectIdentifier> Members

        public bool Equals(ObjectIdentifier other){
            return Identifier == other;
        }

        #endregion
    }
}