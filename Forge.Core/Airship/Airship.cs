#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Forge.Core.Airship.Controllers;
using Forge.Core.Airship.Controllers.AutoPilot;
using Forge.Core.Airship.Data;
using Forge.Core.Physics;
using Forge.Framework;
using Forge.Framework.Draw;
using MonoGameUtility;

#endregion

namespace Forge.Core.Airship{
    internal class Airship : IDisposable{
        public readonly int FactionId;
        public readonly int Uid;
        readonly Battlefield _battlefield;
        readonly AirshipController _controller;
        readonly List<Hardpoint> _hardPoints;
        readonly HullIntegrityMesh _hullIntegrityMesh;
        bool _disposed;

        public Airship(
            ModelAttributes airshipModel,
            DeckSectionContainer deckSectionContainer,
            HullSectionContainer hullSectionContainer,
            AirshipStateData stateData,
            Battlefield battlefield
            ){
            var sw = new Stopwatch();
            sw.Start();
            ModelAttributes = airshipModel;
            HullSectionContainer = hullSectionContainer;
            DeckSectionContainer = deckSectionContainer;

            _battlefield = battlefield;

            _hardPoints = new List<Hardpoint>();
            _hardPoints.Add(new Hardpoint(new Vector3(5, 0, 0), new Vector3(1, 0, 0), _battlefield.ProjectileEngine, ProjectilePhysics.EntityVariant.EnemyShip));

            FactionId = stateData.FactionId;
            Uid = stateData.AirshipId;

            switch (stateData.ControllerType){
                case AirshipControllerType.AI:
                    _controller = new AIAirshipController
                        (
                        ModelAttributes,
                        stateData,
                        _hardPoints,
                        _battlefield.ShipsOnField
                        );
                    break;

                case AirshipControllerType.Player:
                    _controller = new PlayerAirshipController
                        (
                        ModelAttributes,
                        stateData,
                        _hardPoints,
                        _battlefield.ShipsOnField
                        );
                    break;
            }

            _hullIntegrityMesh = new HullIntegrityMesh(HullSectionContainer, _battlefield.ProjectileEngine, _controller.Position, ModelAttributes.Length);

            DebugText.CreateText("x:", 0, 0);
            DebugText.CreateText("y:", 0, 15);
            DebugText.CreateText("z:", 0, 30);

            sw.Stop();

            DebugConsole.WriteLine("Airship class assembled in " + sw.ElapsedMilliseconds + " ms");
        }

        public AirshipStateData StateData{
            get { return _controller.StateData; }
        }

        public ModelAttributes ModelAttributes { get; private set; }

        public ModelAttributes BuffedModelAttributes{
            get { return _controller.GetBuffedAttributes(); }
        }

        //public Vector3 Centroid { get; private set; }
        public HullSectionContainer HullSectionContainer { get; private set; }
        public DeckSectionContainer DeckSectionContainer { get; private set; }

        #region IDisposable Members

        public void Dispose(){
            Debug.Assert(!_disposed);
            _hullIntegrityMesh.Dispose();

            DeckSectionContainer.Dispose();
            HullSectionContainer.Dispose();

            foreach (var hardPoint in _hardPoints){
                hardPoint.Dispose();
            }
            _disposed = true;
        }

        #endregion

        public void SetAutoPilot(AirshipAutoPilot autoPilot){
            _controller.SetAutoPilot(autoPilot);
        }

        public void Update(ref InputState state, double timeDelta){
            DebugText.SetText("x:", "x:" + _controller.StateData.Position.X);
            DebugText.SetText("y:", "y:" + _controller.StateData.Position.Y);
            DebugText.SetText("z:", "z:" + _controller.StateData.Position.Z);


            _controller.Update(ref state, timeDelta);
            SetAirshipWMatrix(_controller.WorldMatrix);

            foreach (var hardPoint in _hardPoints){
                hardPoint.Update(timeDelta);
            }
        }

        public void AddVisibleLayer(int _){
            HullSectionContainer.SetTopVisibleDeck(HullSectionContainer.TopExpIdx - 1);
            DeckSectionContainer.SetTopVisibleDeck(DeckSectionContainer.TopExpIdx - 1);
        }

        public void RemoveVisibleLayer(int _){
            HullSectionContainer.SetTopVisibleDeck(HullSectionContainer.TopExpIdx + 1);
            DeckSectionContainer.SetTopVisibleDeck(DeckSectionContainer.TopExpIdx + 1);
        }

        void SetAirshipWMatrix(Matrix worldMatrix){
            _hullIntegrityMesh.WorldMatrix = worldMatrix;

            foreach (var hullLayer in HullSectionContainer.HullBuffersByDeck){
                hullLayer.WorldMatrix = worldMatrix;
            }

            foreach (var deckLayer in DeckSectionContainer.DeckBufferByDeck){
                deckLayer.WorldMatrix = worldMatrix;
            }

            foreach (var hardPoint in _hardPoints){
                hardPoint.ShipTranslationMtx = worldMatrix;
            }
        }

        ~Airship(){
            Debug.Assert(_disposed);
        }
    }
}