using System;
using System.Collections.Generic;
using System.Linq;
using Forge.Core.Logic;
using Forge.Framework.Draw;
using Forge.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Forge.Core.Airship {
    internal class Airship : IDisposable{
        public float Length;
        public float MaxAscentSpeed;
        public float MaxMovementSpeed;
        public float MaxTurnSpeed;

        public ModelAttributes ModelAttributes { get; private set; }

        int _curDeck;
        readonly int _numDecks;
        readonly List<Hardpoint> _hardPoints;
        readonly ProjectilePhysics _projectilePhysics;
        readonly AirshipController _controller;

        public Vector3 Centroid;
        public GeometryBuffer<VertexPositionNormalTexture>[] Decks;
        public GeometryBuffer<VertexPositionNormalTexture>[] HullLayers;

        public Vector3 Position{
            get { return _controller.Position; }
        }

        public float Velocity{
            get { return _controller.Velocity; }
        }

        public Airship(){
            _curDeck = 0;
            _numDecks = 4;

            _projectilePhysics = new ProjectilePhysics();

            _hardPoints = new List<Hardpoint>();
            _hardPoints.Add(new Hardpoint(new Vector3(0, 0, 0), new Vector3(1, 0, 0), _projectilePhysics, ProjectilePhysics.EntityVariant.EnemyShip));

            var modelAttribs = new ModelAttributes();
            modelAttribs.Length = 50;
            modelAttribs.MaxAscentSpeed = 10;
            modelAttribs.MaxForwardSpeed = 30;
            modelAttribs.MaxReverseSpeed = 10;
            modelAttribs.MaxTurnSpeed = 4f;
            ModelAttributes = modelAttribs;

            var movementState = new AirshipMovementState();
            movementState.Angle = new Vector3(0, 0, 0);
            movementState.CurPosition = new Vector3(Length / 3, 1000, 0);

            _controller = new PlayerAirshipController(
                SetAirshipWMatrix,
                modelAttribs,
                movementState
            );
        }

        public void Update(ref InputState state, double timeDelta){
            _controller.Update(ref state, timeDelta);



            foreach (var hardPoint in _hardPoints){
                hardPoint.Update(timeDelta);
            }
            if (state.PrevState.KeyboardState.IsKeyDown(Keys.Space)) {
                foreach (var hardpoint in _hardPoints) {
                    hardpoint.Fire();
                }
            }

            _projectilePhysics.Update();

        }

        public void AddVisibleLayer(int _){
            if (_curDeck != 0){
                var tempFloorBuff = Decks.Reverse().ToArray();
                var tempWallBuff = HullLayers.Reverse().ToArray();
                //var tempWWallBuff = WallBuffers.Reverse().ToArray();
                for (int i = 0; i < tempFloorBuff.Count(); i++){
                    if (tempFloorBuff[i].Enabled == false){
                        _curDeck--;
                        tempFloorBuff[i].Enabled = true;
                        //tempWWallBuff[i].Enabled = true;

                        if (i < _numDecks - 1){
                            tempWallBuff[i + 1].Enabled = true;
                        }
                        tempWallBuff[i].CullMode = CullMode.None;
                        break;
                    }
                }
            }
        }

        public void RemoveVisibleLayer(int _){
            if (_curDeck < _numDecks - 1){
                for (int i = 0; i < Decks.Count(); i++){
                    if (Decks[i].Enabled){
                        _curDeck++;
                        Decks[i].Enabled = false;
                        HullLayers[i].CullMode = CullMode.CullCounterClockwiseFace;
                        if (i > 0){
                            HullLayers[i - 1].Enabled = false;
                        }
                        //WallBuffers[i].Enabled = false;
                        break;
                    }
                }
            }
        }

        void SetAirshipWMatrix(Matrix worldMatrix) {
            foreach (var deck in Decks) {
                deck.WorldMatrix = worldMatrix;
            }
            foreach (var layer in HullLayers) {
                layer.WorldMatrix = worldMatrix;
            }

            foreach (var hardPoint in _hardPoints) {
                hardPoint.ShipTranslationMtx = worldMatrix;
            }
        }

        public void Dispose(){
            _projectilePhysics.Dispose();
        }
    }
}
