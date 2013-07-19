#region

using System;
using System.Collections.Generic;
using MonoGameUtility;

#endregion

namespace Forge.Core.ObjectEditor{
    /// <summary>
    /// provides an interface through which objects can be added and removed from the airship.
    /// </summary>
    internal class DeckObjectEnvironment{
        /// <summary>
        /// These offsets are used to convert from model space to grid space.
        /// </summary>
        readonly Vector2[] _gridOffsets;

        /// <summary>
        /// Tables of booleans that represent whether the "area" the table maps to is occupied
        /// by an object or not. In order to index an occupation grid, the model space value of
        /// area to be queried must be converted to grid space, which can be obtained using
        /// ConvertToGridSpace()
        /// </summary>
        readonly bool[][,] _occupationGrids;

        bool[,] _curOccupationGrid;

        public DeckObjectEnvironment(HullDataManager hullData){
            //decksections for ladders/objects

            //grid representing avail spots
            _occupationGrids = new bool[hullData.NumDecks][,];
            _gridOffsets = new Vector2[hullData.NumDecks];
            SetupObjectOccupationGrids(hullData.DeckSectionContainer.DeckVertexesByDeck);

            hullData.OnCurDeckChange += OnDeckChange;
        }

        Point ConvertToGridSpace(Vector3 modelSpacePos, int deck){
            modelSpacePos *= 2;
            Vector2 offset = _gridOffsets[deck];
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
                _gridOffsets[i] = new Vector2(minX, maxZ);

                var grid = new bool[layerLength,layerWidth];
                _occupationGrids[i] = grid;
            }
        }

        void OnDeckChange(int oldDeck, int newDeck){
            _curOccupationGrid = _occupationGrids[newDeck];
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