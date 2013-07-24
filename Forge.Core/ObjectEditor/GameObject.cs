#region

using System;
using MonoGameUtility;

#endregion

namespace Forge.Core.ObjectEditor{
    public struct GameObject : IEquatable<ObjectIdentifier>{
        public readonly int Deck;
        public readonly XZPoint GridDimensions;
        public readonly ObjectIdentifier Identifier;
        public readonly string ModelName;
        public readonly Vector3 ModelspacePosition;
        public readonly long ObjectUid;
        public readonly XZPoint Position;
        public readonly float Rotation;
        public readonly GameObjectEnvironment.SideEffect SideEffect;
        public readonly GameObjectType Type;

        public GameObject(
            Vector3 modelspacePosition,
            int deck,
            XZPoint gridDimensions,
            GameObjectEnvironment.SideEffect sideEffect,
            long objectUid,
            GameObjectType type,
            string modelName,
            float rotation
            ){
            Identifier = new ObjectIdentifier(modelspacePosition, deck);
            GridDimensions = gridDimensions;
            Position = Identifier.Origin;
            SideEffect = sideEffect;
            ObjectUid = objectUid;
            Deck = deck;
            Type = type;
            ModelName = modelName;
            Rotation = rotation;
            ModelspacePosition = modelspacePosition;
        }

        #region IEquatable<ObjectIdentifier> Members

        public bool Equals(ObjectIdentifier other){
            return Identifier == other;
        }

        #endregion
    }
}