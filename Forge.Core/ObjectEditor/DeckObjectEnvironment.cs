#region

using System;
using System.Collections.Generic;
using System.Linq;
using MonoGameUtility;

#endregion

namespace Forge.Core.ObjectEditor{
    /// <summary>
    /// provides an interface through which objects can be added and removed from the airship.
    /// </summary>
    internal class DeckObjectEnvironment{
        /// <summary>
        /// Represents the limits of the silhouette that make up the outline of the airship deck.
        /// This is used to prevent objects from being placed in the "rectangle" that makes up the
        /// occupationGrid while being outside the actual limits of  the hull.
        /// </summary>
        readonly int[][] _gridLimitMax;

        readonly int[][] _gridLimitMin;

        /// <summary>
        /// These offsets are used to convert from model space to grid space.
        /// </summary>
        readonly Point[] _gridOffsets;

        /// <summary>
        /// Tables of booleans that represent whether the "area" the table maps to is occupied
        /// by an object or not. In order to index an occupation grid, the model space value of
        /// area to be queried must be converted to grid space, which can be obtained using
        /// ConvertToGridSpace()
        /// </summary>
        readonly bool[][,] _occupationGrids;

        public DeckObjectEnvironment(HullDataManager hullData){
            //decksections for ladders/objects

            //grid representing avail spots
            _occupationGrids = new bool[hullData.NumDecks][,];
            _gridOffsets = new Point[hullData.NumDecks];

            var vertexes = hullData.DeckSectionContainer.DeckVertexesByDeck;

            SetupObjectOccupationGrids(vertexes);
            CalculateGridLimits(vertexes, out _gridLimitMax, out _gridLimitMin);
            int gfgf = 4;
        }

        Point ConvertToGridSpace(Vector3 modelSpacePos, int deck){
            modelSpacePos *= 2;
            Point offset = _gridOffsets[deck];
            int gridX = (int) (offset.X + modelSpacePos.X);
            int gridZ = (int) (offset.Y + modelSpacePos.Z);
            return new Point(gridX, gridZ);
        }

        void SetupObjectOccupationGrids(List<Vector3>[] vertexes){
            for (int i = 0; i < vertexes.Length; i++){
                var layerVerts = vertexes[i];
                float maxX = float.MinValue;
                float maxZ = float.MinValue;
                float minX = float.MaxValue;

                foreach (var vert in layerVerts){
                    if (vert.X > maxX)
                        maxX = vert.X;
                    if (vert.Z > maxZ)
                        maxZ = vert.Z;
                    if (vert.X < minX)
                        minX = vert.X;
                }

                var layerLength = (int) ((maxX - minX)*2);
                var layerWidth = (int) ((maxZ)*4);
                _gridOffsets[i] = new Point((int) (minX*2), (int) (maxZ*2));

                var grid = new bool[layerLength,layerWidth];
                _occupationGrids[i] = grid;
            }
        }

        void CalculateGridLimits(List<Vector3>[] vertexes, out int[][] gridLimitMax, out int[][] gridLimitMin){
            gridLimitMax = new int[vertexes.Length][];
            gridLimitMin = new int[vertexes.Length][];
            for (int deck = 0; deck < vertexes.Length; deck++){
                var layerVerts = vertexes[deck];

                var vertsByRow =
                    from vert in layerVerts
                    where vert.Z >= 0
                    group vert by vert.X;

                var sortedVertsByRow = (
                    from pairing in vertsByRow
                    orderby pairing.Key ascending
                    select pairing
                    ).ToArray();

                var layerLimitMin = new int[sortedVertsByRow.Length];
                var layerLimitMax = new int[sortedVertsByRow.Length];

                var rowArrayMax = _occupationGrids[deck].GetLength(1);
                for (int row = 0; row < sortedVertsByRow.Length; row++){
                    float max = sortedVertsByRow[row].Max(v => v.Z);
                    var converted = ConvertToGridSpace(new Vector3(0, 0, -max), deck).Y;
                    layerLimitMin[row] = converted;
                    layerLimitMax[row] = rowArrayMax - converted;
                }
                gridLimitMax[deck] = layerLimitMax;
                gridLimitMin[deck] = layerLimitMin;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position">The model space position of the object to be placed</param>
        /// <param name="gridDimensions">The unit-grid dimensions of the object. (1,1) cooresponds to a size of (0.5, 0.5) meters.</param>
        /// <param name="deck"></param>
        /// <param name="clearAboveObject">Whether or not the deck tiles above this object should be removed. This is used for multi-story object like ladders.</param>
        void ValidateObjectPlacement(Vector3 position, Point gridDimensions, int deck, bool clearAboveObject = false){
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="position">Model space position</param>
        /// <param name="deck"></param>
        /// <param name="clearAboveObject">Whether or not the deck tiles above this object should be removed. This is used for multi-story object like ladders.</param>
        /// <returns></returns>
        ObjectIdentifier AddObject(string model, Vector3 position, int deck, bool clearAboveObject = false){
            throw new NotImplementedException();
        }

        void RemoveObject(ObjectIdentifier obj){
            throw new NotImplementedException();
        }
    }
}