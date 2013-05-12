using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Forge.Core.Airship {
    public class HullSection : IEquatable<HullSection> {
        readonly float _xStart;
        readonly float _xEnd;
        readonly int _yPanel;
        readonly Vector3 _centroid;//notimpl
        readonly float _width;//notimpl
        readonly Quadrant.Side _side;

        public HullSection(float xStart, float xEnd, int yPanel, Quadrant.Side side) {
            //throw new NotImplementedException();
            _xStart = xStart;
            _xEnd = xEnd;
            _yPanel = yPanel;
            _side = side;
        }

        #region IEquatable<HullSection> Members

        public bool Equals(HullSection other) {
            if (Math.Abs(_xStart - other._xStart) < 0.01f &&
                Math.Abs(_xEnd - other._xEnd) < 0.01f &&
                _yPanel == other._yPanel &&
                _side == other._side) {
                return true;
            }
            return false;
        }

        #endregion
    }
}
