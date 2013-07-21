#region

using System;
using MonoGameUtility;

#endregion

namespace Forge.Core.ObjectEditor{
    public struct DeckPlateIdentifier : IEquatable<DeckPlateIdentifier>{
        public readonly int Deck;
        public readonly Point Origin;

        /// <summary>
        /// Initialize the identifier using the deck and scaled origin.
        /// Scaled origin refers to an origin in model space, scaled so that
        /// its coordinates are integers.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="deck"></param>
        public DeckPlateIdentifier(Point origin, int deck){
            Origin = origin;
            Deck = deck;
        }

        public DeckPlateIdentifier(Vector3 origin, int deck){
            //deck plate step is 0.5f
            var scaled = new Point((int) (origin.X*2), (int) (origin.Z*2));
            Origin = scaled;
            Deck = deck;
        }

        #region IEquatable<DeckPlateIdentifier> Members

        public bool Equals(DeckPlateIdentifier other){
            if (Origin == other.Origin){
                if (Deck == other.Deck){
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}