#region

using Forge.Core.Airship.Data;
using MonoGameUtility;

#endregion

namespace Forge.Core.Airship.Generation{
    /// <summary>
    /// Calculates the attributes of an airship model based on its geometry.
    /// </summary>
    internal static class HullAttributeGenerator{
        public static ModelAttributes Generate(int numDecks){
            var modelAttribs = new ModelAttributes();
            //in the future these attributes will be defined based off analyzing the hull
            modelAttribs.Length = 50;
            modelAttribs.MaxAscentRate = 25;
            modelAttribs.MaxForwardVelocity = 40;
            modelAttribs.MaxReverseVelocity = 20;
            modelAttribs.MaxTurnSpeed = 0.87265f;
            modelAttribs.Berth = 13.95f;
            modelAttribs.Depth = 7.250177f;
            modelAttribs.NumDecks = numDecks;
            modelAttribs.Centroid = new Vector3(modelAttribs.Length/3, 0, 0);
            modelAttribs.MaxAcceleration = 10;
            modelAttribs.MaxAscentAcceleration = 7f;
            modelAttribs.MaxTurnAcceleration = 0.22685f;
            return modelAttribs;
        }
    }
}