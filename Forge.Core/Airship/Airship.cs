﻿#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Forge.Core.Airship.Controllers;
using Forge.Core.Airship.Data;
using Forge.Core.Physics;
using Forge.Framework;
using MonoGameUtility;

#endregion

namespace Forge.Core.Airship{
    internal class Airship : IDisposable{
        public readonly int FactionId;
        public readonly int Uid;
        readonly AirshipController _controller;
        readonly List<Hardpoint> _hardPoints;
        readonly HullIntegrityMesh _hullIntegrityMesh;
        readonly Battlefield _battlefield;
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
                        _hardPoints
                        );
                    break;

                case AirshipControllerType.Player:
                    _controller = new PlayerAirshipController
                        (
                        ModelAttributes,
                        stateData,
                        _hardPoints
                        );
                    break;
            }

            _hullIntegrityMesh = new HullIntegrityMesh(HullSectionContainer, _battlefield.ProjectileEngine, _controller.Position, ModelAttributes.Length);

            sw.Stop();

            DebugConsole.WriteLine("Airship class assembled in " + sw.ElapsedMilliseconds + " ms");
        }

        public ModelAttributes ModelAttributes { get; private set; }

        //public Vector3 Centroid { get; private set; }
        public HullSectionContainer HullSectionContainer { get; private set; }
        public DeckSectionContainer DeckSectionContainer { get; private set; }

        /// <summary>
        ///   Reflects the current position of the airship, as measured from its center.
        /// </summary>
        public Vector3 Position{
            get { return _controller.Position; }
        }

        public float Velocity{
            get { return _controller.Velocity; }
        }

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

        public void Update(ref InputState state, double timeDelta){
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