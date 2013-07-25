#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Forge.Core.Airship.Controllers;
using Forge.Core.Airship.Controllers.AutoPilot;
using Forge.Core.Airship.Data;
using Forge.Core.ObjectEditor;
using Forge.Core.Physics;
using Forge.Framework;
using MonoGameUtility;

#endregion

namespace Forge.Core.Airship{
    public class Airship : IDisposable{
        public readonly AirshipController Controller;
        public readonly int FactionId;
        public readonly int Uid;
        readonly Battlefield _battlefield;
        readonly List<Hardpoint> _hardPoints;
        readonly HullIntegrityMesh _hullIntegrityMesh;
        readonly AirshipObjectContainer _objContainer;
        readonly bool _playerairship;
        bool _disposed;

        public Airship(
            ModelAttributes airshipModel,
            DeckSectionContainer deckSectionContainer,
            HullSectionContainer hullSectionContainer,
            AirshipStateData stateData,
            List<GameObject> containedObjects,
            Battlefield battlefield
            ){
            var sw = new Stopwatch();
            sw.Start();
            ModelAttributes = airshipModel;
            HullSectionContainer = hullSectionContainer;
            DeckSectionContainer = deckSectionContainer;
            _battlefield = battlefield;
            _objContainer = new AirshipObjectContainer(containedObjects);

            _hardPoints = new List<Hardpoint>();
            var emitter = new ProjectileEmitter("Config/Projectiles/TestShot.config", 12000, 0, _battlefield.ProjectileEngine);
            _hardPoints.Add(new Hardpoint(new Vector3(25, 0, 0), new Vector3(1, 0, 0), emitter));

            FactionId = stateData.FactionId;
            Uid = stateData.AirshipId;

            switch (stateData.ControllerType){
                case AirshipControllerType.AI:
                    Controller = new AIAirshipController
                        (
                        ModelAttributes,
                        stateData,
                        _hardPoints,
                        _battlefield.ShipsOnField
                        );
                    break;

                case AirshipControllerType.Player:
                    _playerairship = true;
                    Controller = new PlayerAirshipController
                        (
                        ModelAttributes,
                        stateData,
                        _hardPoints,
                        _battlefield.ShipsOnField
                        );
                    break;
            }

            if (!_playerairship){
                _hullIntegrityMesh = new HullIntegrityMesh(HullSectionContainer, _battlefield.ProjectileEngine, Controller.Position, ModelAttributes.Length);
            }

            sw.Stop();

            DebugConsole.WriteLine("Airship class assembled in " + sw.ElapsedMilliseconds + " ms");
        }

        public AirshipStateData StateData{
            get { return Controller.StateData; }
        }

        public ModelAttributes ModelAttributes { get; private set; }

        public ModelAttributes BuffedModelAttributes{
            get { return Controller.GetBuffedAttributes(); }
        }

        //public Vector3 Centroid { get; private set; }
        public HullSectionContainer HullSectionContainer { get; private set; }
        public DeckSectionContainer DeckSectionContainer { get; private set; }

        #region IDisposable Members

        public void Dispose(){
            Debug.Assert(!_disposed);
            if (!_playerairship){
                _hullIntegrityMesh.Dispose();
            }

            DeckSectionContainer.Dispose();
            HullSectionContainer.Dispose();
            _objContainer.Dispose();

            foreach (var hardPoint in _hardPoints){
                hardPoint.Dispose();
            }
            _disposed = true;
        }

        #endregion

        public void SetAutoPilot(AirshipAutoPilot autoPilot){
            Controller.SetAutoPilot(autoPilot);
            _objContainer.Update();
        }

        public void Update(double timeDelta){
            Controller.Update(timeDelta);
            SetAirshipWMatrix(Controller.WorldTransform);
        }

        public void AddVisibleLayer(int _){
            int newDeck = HullSectionContainer.TopExpIdx - 1;
            HullSectionContainer.SetTopVisibleDeck(newDeck);
            DeckSectionContainer.SetTopVisibleDeck(newDeck);
            _objContainer.SetTopVisibleDeck(newDeck);
        }

        public void RemoveVisibleLayer(int _){
            int newDeck = HullSectionContainer.TopExpIdx + 1;
            HullSectionContainer.SetTopVisibleDeck(newDeck);
            DeckSectionContainer.SetTopVisibleDeck(newDeck);
            _objContainer.SetTopVisibleDeck(newDeck);
        }

        void SetAirshipWMatrix(Matrix worldTransform){
            if (!_playerairship){
                _hullIntegrityMesh.WorldTransform = worldTransform;
            }

            _objContainer.WorldTransform = worldTransform;

            foreach (var hullLayer in HullSectionContainer.HullBuffersByDeck){
                hullLayer.WorldTransform = worldTransform;
            }

            foreach (var deckLayer in DeckSectionContainer.DeckBufferByDeck){
                deckLayer.WorldTransform = worldTransform;
            }

            foreach (var hardPoint in _hardPoints){
                hardPoint.ShipTranslationMtx = worldTransform;
            }
        }

        ~Airship(){
            Debug.Assert(_disposed);
        }
    }
}