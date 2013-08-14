#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Forge.Core.Airship.Data;
using Forge.Core.GameObjects;
using Forge.Framework.Draw;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;

#endregion

namespace Forge.Core.ObjectEditor{
    /// <summary>
    /// provides an interface through which objects can be added and removed from the airship.
    /// </summary>
    public class GameObjectEnvironment : IDisposable{
        #region SideEffect enum

        public enum SideEffect{
            None,
            CutsIntoCeiling,
            CutsIntoNearestHull
        }

        #endregion

        const string _objectModelShader = "Config/Shaders/TintedModel.config";
        const int _maxObjectsPerLayer = 100;

        /// <summary>
        /// used by the wall-construction component to make sure none of the constructed walls bisect placed objects
        /// </summary>
        public readonly Dictionary<ObjectIdentifier, XZPoint>[] ObjectFootprints;

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
        readonly XZPoint _gridOffset;

        readonly HullEnvironment _hullEnvironment;

        readonly List<OnObjectAddRemove> _objectAddedEvent;
        readonly ObjectModelBuffer<ObjectIdentifier>[] _objectModelBuffer;
        readonly List<ObjectPlacementTest> _objectPlacementTestDelegs;
        readonly List<OnObjectAddRemove> _objectRemovedEvent;

        /// <summary>
        /// used to keep track of side effects such as removing deck plates or removing hull sections
        /// </summary>
        readonly List<Tuple<GameObject, SideEffect>>[] _objectSideEffects;

        /// <summary>
        /// Tables of booleans that represent whether the "area" the table maps to is occupied
        /// by an object or not. In order to index an occupation grid, the model space value of
        /// area to be queried must be converted to grid space, which can be obtained using
        /// ConvertToGridspace()
        /// </summary>
        readonly bool[][,] _occupationGrids;

        public GameObjectEnvironment(HullEnvironment hullEnv){
            _hullEnvironment = hullEnv;
            _deckSectionContainer = hullEnv.DeckSectionContainer;
            _objectSideEffects = new List<Tuple<GameObject, SideEffect>>[hullEnv.NumDecks];
            _objectAddedEvent = new List<OnObjectAddRemove>();
            _objectRemovedEvent = new List<OnObjectAddRemove>();
            _objectPlacementTestDelegs = new List<ObjectPlacementTest>();

            for (int i = 0; i < hullEnv.NumDecks; i++){
                _objectSideEffects[i] = new List<Tuple<GameObject, SideEffect>>();
            }
            _occupationGrids = new bool[hullEnv.NumDecks][,];

            //we have to manually yank vertexes
            var vertexes = (
                from deck in hullEnv.DeckSectionContainer.DeckBufferByDeck
                select
                    (
                        from DeckPlateIdentifier id in deck
                        where id.Origin.X > int.MinValue
                        select new XZPoint(id.Origin.X, id.Origin.Y)
                        ).ToList()
                ).ToArray();

            _gridOffset = SetupObjectOccupationGrids(hullEnv.DeckSectionContainer.DeckVertexesByDeck);
            CalculateGridLimits(vertexes, out _gridLimitMax, out _gridLimitMin);

            _hullEnvironment.OnCurDeckChange += OnVisibleDeckChange;

            _objectModelBuffer = new ObjectModelBuffer<ObjectIdentifier>[hullEnv.NumDecks];
            ObjectFootprints = new Dictionary<ObjectIdentifier, XZPoint>[hullEnv.NumDecks];
            for (int i = 0; i < hullEnv.NumDecks; i++){
                _objectModelBuffer[i] = new ObjectModelBuffer<ObjectIdentifier>(_maxObjectsPerLayer, _objectModelShader);
                ObjectFootprints[i] = new Dictionary<ObjectIdentifier, XZPoint>(_maxObjectsPerLayer);
            }

            OnVisibleDeckChange(0, 0);
        }

        public InternalWallEnvironment InternalWallEnvironment { private get; set; }

        #region IDisposable Members

        public void Dispose(){
            foreach (var buffer in _objectModelBuffer){
                buffer.Dispose();
            }
        }

        #endregion

        OccupationGridPos ConvertToGridspace(Vector3 modelSpacePos){
            modelSpacePos *= 2;
            int gridX = (int) (_gridOffset.X + modelSpacePos.X);
            int gridZ = (int) (_gridOffset.Z + modelSpacePos.Z);
            return new OccupationGridPos(gridX, gridZ);
        }

        OccupationGridPos ConvertToGridspace(XZPoint gridPos){
            int gridX = (_gridOffset.X + gridPos.X);
            int gridZ = (_gridOffset.Z + gridPos.Z);
            return new OccupationGridPos(gridX, gridZ);
        }

        XZPoint SetupObjectOccupationGrids(List<Vector3>[] vertexes){
            var layerVerts = vertexes[0];
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

            for (int i = 0; i < vertexes.Length; i++){
                var grid = new bool[layerLength + 1,layerWidth];
                _occupationGrids[i] = grid;
            }

            var ret = new XZPoint(-(int) (minX*2), (int) (maxZ*2));
            return ret;
        }

        void CalculateGridLimits(List<XZPoint>[] vertexes, out int[][] gridLimitMax, out int[][] gridLimitMin){
            gridLimitMax = new int[vertexes.Length][];
            gridLimitMin = new int[vertexes.Length][];

            float minX = vertexes[0].Min(v => v.X);
            float maxX = vertexes[0].Max(v => v.X);
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
                    ).ToList();

                int southPadding = (int) ((sortedVertsByRow[0].Key - minX));
                int northPadding = (int) ((maxX - sortedVertsByRow.Last().Key));
                int length = southPadding + northPadding + sortedVertsByRow.Count;

                var layerLimitMin = new int[length];
                var layerLimitMax = new int[length];

                var rowArrayMax = _occupationGrids[deck].GetLength(1);
                //fill padding area
                for (int row = 0; row < southPadding; row++){
                    layerLimitMin[row] = int.MaxValue;
                    layerLimitMax[row] = int.MinValue;
                }
                for (int row = southPadding + sortedVertsByRow.Count; row < length; row++){
                    layerLimitMin[row] = int.MaxValue;
                    layerLimitMax[row] = int.MinValue;
                }

                for (int row = southPadding; row < southPadding + sortedVertsByRow.Count; row++){
                    int max = sortedVertsByRow[row - southPadding].Max(v => v.Z);
                    var converted = ConvertToGridspace(new XZPoint(0, -max)).Z;
                    layerLimitMin[row] = converted - 1;
                    layerLimitMax[row] = rowArrayMax - converted + 1;
                }
                gridLimitMax[deck] = layerLimitMax;
                gridLimitMin[deck] = layerLimitMin;
            }
        }

        void SetOccupationGridState(OccupationGridPos origin, List<XZRectangle> areas, int deck, bool value){
            var occupationGrid = _occupationGrids[deck];
            foreach (var area in areas){
                for (int x = origin.X + area.X; x < origin.X + area.Width + area.X; x++){
                    for (int z = origin.Z + area.Z; z < origin.Z + area.Length + area.Z; z++){
                        occupationGrid[x, z] = value;
                    }
                }
            }
        }

        void ModifyDeckPlates(OccupationGridPos origin, XZPoint dims, int deck, bool value){
            var buffer = _deckSectionContainer.DeckBufferByDeck[deck];
            //convert origin point so z axis bisects the ship instead of being left justified to it
            origin.Z -= _gridOffset.Z;

            for (int x = origin.X + 1; x < origin.X + dims.X + 1; x++){
                for (int z = origin.Z; z < origin.Z + dims.Z; z++){
                    var identifier = new DeckPlateIdentifier(new Point(x, z), deck);
                    bool b;
                    if (value){
                        b = buffer.EnableObject(identifier);
                    }
                    else{
                        b = buffer.DisableObject(identifier);
                    }
                    Debug.Assert(b);
                }
            }
        }

        void ApplyObjectSideEffect(GameObject gameObj, SideEffect sideEffect){
            if (sideEffect == SideEffect.CutsIntoCeiling){
                int deck = gameObj.Identifier.Deck;
                if (deck != 0){
                    var gridPos = ConvertToGridspace(gameObj.Position);
                    var areas = new List<XZRectangle>();
                    var dims = gameObj.Type.Attribute<XZPoint>(GameObjectAttr.Dimensions);
                    var interactionArea = gameObj.Type.Attribute<XZRectangle>(GameObjectAttr.InteractionArea);
                    areas.Add(gameObj.Type.Attribute<XZRectangle>(GameObjectAttr.CeilingCutArea));
                    areas.Add(interactionArea);

                    SetOccupationGridState(gridPos, areas, gameObj.Identifier.Deck - 1, true);
                    ModifyDeckPlates(gridPos, dims, deck - 1, false);
                }
            }
            /*
            if (sideEffect == SideEffect.CutsIntoPortHull){
                //throw new NotImplementedException();
            }
            if (sideEffect == SideEffect.CutsIntoStarboardHull){
                throw new NotImplementedException();
            }
             */
        }

        void RemoveObjectSideEffect(){
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

        #region public interfaces

        #region Delegates

        public delegate bool ObjectPlacementTest(
            Vector3 mPosition,
            XZPoint gridDims,
            int deck,
            float rotation,
            GameObjectType gameObject,
            SideEffect pSideEffects);

        public delegate void OnObjectAddRemove(GameObject obj);

        #endregion

        bool IsRectangleFootprintValid(XZRectangle rectangle, int deck){
            var gridLimitMax = _gridLimitMax[deck];
            var gridLimitMin = _gridLimitMin[deck];
            var occupationGrid = _occupationGrids[deck];

            for (int x = rectangle.X; x < rectangle.X + rectangle.Width; x++){
                if (x < 0 || x >= gridLimitMax.Length){
                    return false;
                }
                for (int z = rectangle.Z; z < rectangle.Z + rectangle.Length; z++){
                    if (z < gridLimitMin[x] || z >= gridLimitMax[x]){
                        return false;
                    }
                    //confirms there is no object in the section of grid
                    if (occupationGrid[x, z]){
                        return false;
                    }
                }
            }
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="position">The model space position of the object to be placed</param>
        /// <param name="gridDimensions">The unit-grid dimensions of the object. (1,1) cooresponds to a size of (0.5, 0.5) meters.</param>
        /// <param name="deck"></param>
        /// <param name="gameObject"> </param>
        /// <param name="pSideEffects"> </param>
        /// <param name="rotation"> </param>
        public bool IsObjectPlacementValid(
            Vector3 position,
            XZPoint gridDimensions,
            int deck,
            float rotation,
            GameObjectType gameObject,
            SideEffect pSideEffects){
            var gridPosition = ConvertToGridspace(position);

            var rect = new XZRectangle(gridPosition.X, gridPosition.Z, gridDimensions.X, gridDimensions.Z);

            if (!IsRectangleFootprintValid(rect, deck)){
                return false;
            }

            bool isInteractable = gameObject.Attribute<bool>(GameObjectAttr.IsInteractable);
            if (isInteractable){
                var interactionArea = gameObject.Attribute<XZRectangle>(GameObjectAttr.InteractionArea);
                interactionArea.X += gridPosition.X;
                interactionArea.Z += gridPosition.Z;
                if (!IsRectangleFootprintValid(interactionArea, deck)){
                    return false;
                }
            }

            if (pSideEffects == SideEffect.CutsIntoCeiling){
                if (deck != 0){
                    if (!IsRectangleFootprintValid(rect, deck - 1)){
                        return false;
                    }

                    bool multifloorAccess = gameObject.Attribute<bool>(GameObjectAttr.IsMultifloorInteractable);
                    if (isInteractable && multifloorAccess){
                        var interactionArea = gameObject.Attribute<XZRectangle>(GameObjectAttr.InteractionArea);
                        interactionArea.X += gridPosition.X;
                        interactionArea.Z += gridPosition.Z;
                        if (!IsRectangleFootprintValid(interactionArea, deck - 1)){
                            return false;
                        }
                    }
                }
            }

            if (!InternalWallEnvironment.IsObjectPlacementValid(position, gridDimensions, deck)){
                return false;
            }

            foreach (var deleg in _objectPlacementTestDelegs){
                bool result = deleg.Invoke(position, gridDimensions, deck, rotation, gameObject, pSideEffects);
                if (!result){
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ObjectIdentifier AddObject(GameObject obj, string modelName, SideEffect sideEffect){
            Matrix posTransform = Matrix.CreateTranslation(obj.ModelspacePosition);
            var model = Resource.LoadContent<Model>(modelName);
            var rotTransform = Matrix.CreateFromYawPitchRoll(obj.Rotation, 0, 0);
            posTransform = rotTransform*posTransform;

            _objectModelBuffer[obj.Deck].AddObject(obj.Identifier, model, posTransform);

            var occPos = ConvertToGridspace(obj.ModelspacePosition);

            //apply rotations here when it comes time to implement          
            var occupationAreas = new List<XZRectangle>();

            var dims = obj.Type.Attribute<XZPoint>(GameObjectAttr.Dimensions);
            occupationAreas.Add(new XZRectangle(0, 0, dims.X, dims.Z));
            var accessArea = obj.Type.Attribute<XZRectangle>(GameObjectAttr.InteractionArea);
            occupationAreas.Add(accessArea);

            SetOccupationGridState(occPos, occupationAreas, obj.Deck, true);

            _objectSideEffects[obj.Deck].Add(new Tuple<GameObject, SideEffect>(obj, sideEffect));
            ApplyObjectSideEffect(obj, sideEffect);
            ObjectFootprints[obj.Deck].Add(obj.Identifier, dims);

            foreach (var deleg in _objectAddedEvent){
                deleg.Invoke(obj);
            }

            return obj.Identifier;
        }

        public void RemoveObject(ObjectIdentifier obj){
            throw new NotImplementedException();

            foreach (var deleg in _objectRemovedEvent){
                //deleg.Invoke(obj);
            }
        }

        public void AddOnObjectPlacement(OnObjectAddRemove deleg){
            _objectAddedEvent.Add(deleg);
        }

        public void AddOnObjectRemove(OnObjectAddRemove deleg){
            _objectRemovedEvent.Add(deleg);
        }

        public void AddObjectPlacementTest(ObjectPlacementTest deleg){
            _objectPlacementTestDelegs.Add(deleg);
        }

        public List<GameObject> DumpGameObjects(){
            var ret = (
                from deck in _objectSideEffects
                from entry in deck
                select entry.Item1
                ).ToList();
            return ret;
        }

        #endregion

        #region Nested family: OccupationGridPos

        /// <summary>
        /// Point pseudo-class used for family richness to prevent errors with the conversions common to this class.
        /// </summary>
        struct OccupationGridPos{
            public readonly int X;
            public int Z;

            public OccupationGridPos(int x, int z){
                X = x;
                Z = z;
            }

            public static OccupationGridPos operator +(OccupationGridPos value1, XZPoint value2){
                return new OccupationGridPos(value2.X + value1.X, value2.Z + value1.Z);
            }

            public static OccupationGridPos operator -(OccupationGridPos value1, XZPoint value2){
                return new OccupationGridPos(value1.X - value2.X, value1.Z - value2.Z);
            }

            public override string ToString(){
                return string.Format("{{X:{0} Z:{1}}}", X, Z);
            }
        }

        #endregion
    }
}