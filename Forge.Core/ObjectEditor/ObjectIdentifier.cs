﻿#region

using System;
using MonoGameUtility;

#endregion

namespace Forge.Core.ObjectEditor{
    /// <summary>
    ///this is a pretty shitty hack for a identifier class, but for the purposes of the object editor, it is fine.
    ///the object editor uses this to identify objects to be added/removed from the environment
    /// </summary>
    public class ObjectIdentifier : IEquatable<ObjectIdentifier>{
        public readonly int Deck;
        public readonly Point Origin;

        /// <summary>
        /// Initialize the identifier using the deck and scaled origin.
        /// Scaled origin refers to an origin in model space, scaled so that
        /// its coordinates are integers.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="deck"></param>
        public ObjectIdentifier(Point origin, int deck){
            Origin = origin;
            Deck = deck;
        }

        public ObjectIdentifier(Vector3 origin, int deck){
            //deck plate step is 0.5f
            var scaled = new Point((int) (origin.X*2), (int) (origin.Y*2));
            Origin = scaled;
            Deck = deck;
        }

        #region IEquatable<ObjectIdentifier> Members

        public bool Equals(ObjectIdentifier other){
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