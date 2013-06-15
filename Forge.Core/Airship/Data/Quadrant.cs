namespace Forge.Core.Airship.Data{
    public static class Quadrant{
        #region Side enum

        /// <summary>
        ///   z>0; port
        /// </summary>
        public enum Side{
            Starboard,
            Port
        }

        #endregion

        public static Side PointToSide(float z){
            return z < 0 ? Side.Port : Side.Starboard;
        }
    }
}