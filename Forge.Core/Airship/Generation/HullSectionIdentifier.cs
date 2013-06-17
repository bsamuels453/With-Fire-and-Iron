#region

using System;
using Forge.Core.Airship.Data;

#endregion

namespace Forge.Core.Airship.Generation{
    /// <summary>
    /// Used to identify chunks of the airship's hull. Ever since the switch to texture painting
    /// based decals, this class has only been in use in airship generation.
    /// </summary>
    public struct HullSectionIdentifier : IEquatable<HullSectionIdentifier>{
        public readonly int Deck;
        public readonly float XStart;
        public readonly int YPanel;
        public Quadrant.Side Side;

        public HullSectionIdentifier(float xStart, int yPanel, Quadrant.Side side, int deck){
            XStart = xStart;
            YPanel = yPanel;
            Side = side;
            Deck = deck;
        }

        #region IEquatable<HullSectionIdentifier> Members

        public bool Equals(HullSectionIdentifier other){
            if (Math.Abs(XStart - other.XStart) < 0.01f &&
                YPanel == other.YPanel &&
                    Side == other.Side &&
                        Deck == other.Deck){
                return true;
            }
            return false;
        }

        #endregion
    }
}