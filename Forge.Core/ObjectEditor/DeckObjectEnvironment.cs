#region

using System;
using System.Collections.Generic;
using System.Linq;
using Forge.Core.Airship.Data;
using Forge.Framework.Draw;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;

#endregion

namespace Forge.Core.ObjectEditor{
    /// <summary>
    /// provides an interface through which objects can be added and removed from the airship.
    /// </summary>
    public class DeckObjectEnvironment : IDisposable{
        #region SideEffect enum

        public enum SideEffect{
            None,
            CutsIntoCeiling,
            CutsIntoStarboardHull,
            CutsIntoPortHull
        }

        #endregion

        const string _objectModelShader = "Config/Shaders/TintedModel.config";
        const int _maxObjectsPerLayer = 100;
        readonly DeckSectionContainer _deckSectionContainer;

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

        readonly HullEnvironment _hullEnvironment;

        readonly ObjectModelBuffer<ObjectIdentifier>[] _objectModelBuffer;

        readonly List<ObjectSideEffect>[] _objectSideEffects;

        /// <summary>
        /// Tables of booleans that represent whether the "area" the table maps to is occupied
        /// by an object or not. In order to index an occupation grid, the model space value of
        /// area to be queried must be converted to grid space, which can be obtained using
        /// ConvertToGridspace()
        /// </summary>
        readonly bool[][,] _occupationGrids;

        public DeckObjectEnvironment(HullEnvironment hullEnv){
            _hullEnvironment = hullEnv;
            _deckSectionContainer = hullEnv.DeckSectionContainer;
            _objectSideEffects = new List<ObjectSideEffect>[hullEnv.NumDecks];
            for (int i = 0; i < hullEnv.NumDecks; i++){
                _objectSideEffects[i] = new List<ObjectSideEffect>();
            }
            _occupationGrids = new bool[hullEnv.NumDecks][,];
            _gridOffsets = new Point[hullEnv.NumDecks];

            var vertexes = hullEnv.DeckSectionContainer.DeckVertexesByDeck;

            SetupObjectOccupationGrids(vertexes);
            CalculateGridLimits(vertexes, out _gridLimitMax, out _gridLimitMin);

            _hullEnvironment.OnCurDeckChange += OnVisibleDeckChange;

            _objectModelBuffer = new ObjectModelBuffer<ObjectIdentifier>[hullEnv.NumDecks];
            for (int i = 0; i < hullEnv.NumDecks; i++){
                _objectModelBuffer[i] = new ObjectModelBuffer<ObjectIdentifier>(_maxObjectsPerLayer, _objectModelShader);
            }

            OnVisibleDeckChange(0, 0);
        }

        #region IDisposable Members

        public void Dispose(){
            foreach (var buffer in _objectModelBuffer){
                buffer.Dispose();
            }
        }

        #endregion

        Point ConvertToGridspace(Vector3 modelSpacePos, int deck){
            modelSpacePos *= 2;
            Point offset = _gridOffsets[deck];
            int gridX = (int) (offset.X + modelSpacePos.X);
            int gridZ = (int) (offset.Y + modelSpacePos.Z);
            return new Point(gridX, gridZ);
        }

        Point ConvertGridspaceBetweenDecks(Point gridPos, int initlDeck, int destDeck){
            gridPos -= _gridOffsets[initlDeck];
            gridPos += _gridOffsets[destDeck];
            return gridPos;
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
                _gridOffsets[i] = new Point(-(int) (minX*2), (int) (maxZ*2));

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
                    var converted = ConvertToGridspace(new Vector3(0, 0, -max), deck).Y;
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
        public bool IsObjectPlacementValid(Vector3 position, Point gridDimensions, int deck, bool clearAboveObject = false){
            var gridPosition = ConvertToGridspace(position, deck);
            var gridLimitMax = _gridLimitMax[deck];
            var gridLimitMin = _gridLimitMin[deck];
            var occupationGrid = _occupationGrids[deck];
            for (int x = gridPosition.X; x < gridDimensions.X + gridPosition.X; x++){
                if (x < 0 || x >= gridLimitMax.Length){
                    return false;
                }
                for (int z = gridPosition.Y; z < gridDimensions.Y + gridPosition.Y; z++){
                    if (z < gridLimitMin[x] || z >= gridLimitMax[x]){
                        return false;
                    }
                    //confirms there is no object in the section of grid
                    if (occupationGrid[x, z]){
                        return false;
                    }
                }
            }
            if (clearAboveObject){
                if (deck == 0)
                    return true;
                return IsObjectPlacementValid(position, gridDimensions, deck - 1);
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelName"></param>
        /// <param name="position">Model space position</param>
        /// <param name="dimensions">Dimensions of the object in grid-space units </param>
        /// <param name="deck"></param>
        /// <param name="sideEffect"> </param>
        /// <returns></returns>
        public ObjectIdentifier AddObject(
            string modelName,
            Vector3 position,
            Point dimensions,
            int deck,
            SideEffect sideEffect = SideEffect.None){
            var identifier = new ObjectIdentifier(position, deck);

            Matrix trans = Matrix.CreateTranslation(position);
            var model = Resource.LoadContent<Model>(modelName);
            _objectModelBuffer[deck].AddObject(identifier, model, trans);

            var gridPos = ConvertToGridspace(position, deck);
            SetOccupationGridState(gridPos, dimensions, deck, true);
            var objSideEffect = new ObjectSideEffect
                (
                identifier,
                dimensions,
                gridPos,
                sideEffect
                );
            _objectSideEffects[deck].Add(objSideEffect);
            ApplyObjectSideEffect(objSideEffect);

            return identifier;
        }

        void SetOccupationGridState(Point position, Point dims, int deck, bool value){
            var occupationGrid = _occupationGrids[deck];
            for (int x = position.X; x < position.X + dims.X; x++){
                for (int z = position.Y; z < position.Y + dims.Y; z++){
                    occupationGrid[x, z] = value;
                }
            }
        }

        void ApplyObjectSideEffect(ObjectSideEffect sideEffect){
            if (sideEffect.SideEffect == SideEffect.CutsIntoCeiling){
                int deck = sideEffect.Identifier.Deck;
                if (deck != 0){
                    //convert out of the gridspace because gridspace values are only accurate for the deck
                    //they were created for
                    var convertedPos = ConvertGridspaceBetweenDecks(sideEffect.GridPosition, deck, deck - 1);

                    SetOccupationGridState(convertedPos, sideEffect.GridDimensions, sideEffect.Identifier.Deck - 1, true);
                }
            }
            if (sideEffect.SideEffect == SideEffect.CutsIntoPortHull){
                throw new NotImplementedException();
            }
            if (sideEffect.SideEffect == SideEffect.CutsIntoStarboardHull){
                throw new NotImplementedException();
            }
        }

        void RemoveObjectSideEffect(){
            throw new NotImplementedException();
        }

        void RemoveObject(ObjectIdentifier obj){
            throw new NotImplementedException();
        }

        void OnVisibleDeckChange(int old, int newDeck){
            foreach (var buffer in _objectModelBuffer){
                buffer.Enabled = false;
            }
            for (int i = _hullEnvironment.NumDecks - 1; i >= newDeck; i--){
                _objectModelBuffer[i].Enabled = true;
            }
        }

        #region Nested type: ObjectSideEffect

        struct ObjectSideEffect : IEquatable<ObjectIdentifier>{
            public readonly Point GridDimensions;
            public readonly Point GridPosition;
            public readonly ObjectIdentifier Identifier;
            public readonly SideEffect SideEffect;

            public ObjectSideEffect(ObjectIdentifier identifier, Point gridDimensions, Point gridPosition, SideEffect sideEffect){
                Identifier = identifier;
                GridDimensions = gridDimensions;
                GridPosition = gridPosition;
                SideEffect = sideEffect;
            }

            #region IEquatable<ObjectIdentifier> Members

            public bool Equals(ObjectIdentifier other){
                return Identifier == other;
            }

            #endregion
        }

        #endregion
    }
}