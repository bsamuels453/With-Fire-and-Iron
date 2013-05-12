using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forge.Core.Airship {
    static public class Quadrant{
        /// <summary>
        /// z>0; port
        /// </summary>
        public enum Side{
            Starboard,
            Port
        }

        static public Side PointToSide(float z){
            return z > 0 ? Side.Port : Side.Starboard;
        }

    }
}
