using System.Linq;
using Forge.Framework.Draw;
using Forge.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Forge.Core.Airship {
    internal class Airship{
        public float Length;
        public float MaxAscentSpeed;
        public float MaxMovementSpeed;
        public float MaxTurnSpeed;

        public Vector3 Position;
        public Vector3 Angle;
        int _curDeck;
        int _numDecks;
        Hardpoint[] _hardPoints;
        HullIntegrityMesh _integrityMesh;

        public Vector3 Centroid;
        public GeometryBuffer<VertexPositionNormalTexture>[] Decks;
        public GeometryBuffer<VertexPositionNormalTexture>[] HullLayers;

        public TurnState CurTurnState;

        int _engineSpeed;

        public int EngineSpeed{
            get { return _engineSpeed; }
            set{
                if (value <= 3 && value >= -2){
                    _engineSpeed = value;
                }
            }
        }

        public Airship(){
            _curDeck = 0;
            _numDecks = 4;
            Length = 50;
            MaxAscentSpeed = 1;
            MaxMovementSpeed = 1;
            MaxTurnSpeed = 0.005f;
            Angle = new Vector3(0, 0, 0);
            Position = new Vector3(Length/3, 1000, 0);
        }

        public enum TurnState{
            TurningLeft,
            TurningRight,
            Stable
        }

        public void Update(ref InputState state, double timeDelta){
            #region input

            var keyState = state.KeyboardState;
            var prevKeyState = state.PrevState.KeyboardState;

            if (keyState.IsKeyUp(Keys.W) && prevKeyState.IsKeyDown(Keys.W)){
                EngineSpeed++;
            }
            if (keyState.IsKeyUp(Keys.S) && prevKeyState.IsKeyDown(Keys.S)){
                if (EngineSpeed > 0) {
                    EngineSpeed = 0;
                }
                else{
                    EngineSpeed--;
                }
            }

            int altitudeSpeed = 0;
            if (keyState.IsKeyDown(Keys.LeftShift)) {
                altitudeSpeed = 1;
            }
            if (keyState.IsKeyDown(Keys.LeftControl)) {
                altitudeSpeed = -1;
            }
            bool isTurning = false;
            if (keyState.IsKeyDown(Keys.A)) {
                CurTurnState = TurnState.TurningLeft;
                isTurning = true;
            }
            if (keyState.IsKeyDown(Keys.D)) {
                CurTurnState = TurnState.TurningRight;
                isTurning = true;
            }
            if (!isTurning){
                CurTurnState = TurnState.Stable;
            }

            #endregion

            float engineDutyCycle = (float) _engineSpeed/3;
            float altitudeDutyCycle = altitudeSpeed;

            int turnValue = 0;
            switch (CurTurnState){
                case TurnState.Stable:
                    turnValue = 0;
                    break;
                case TurnState.TurningLeft:
                    turnValue = 1;
                    break;
                case TurnState.TurningRight:
                    turnValue = -1;
                    break;
            }

            Angle.Y += turnValue * MaxTurnSpeed;
            var unitVec = Common.GetComponentFromAngle(Angle.Y, 1);
            Position.X += unitVec.X * engineDutyCycle* MaxMovementSpeed;
            Position.Z += -unitVec.Y * engineDutyCycle* MaxMovementSpeed;
            Position.Y += altitudeDutyCycle * MaxAscentSpeed;
            SetAirshipPosition(Position, Angle);
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

        void SetAirshipPosition(Vector3 position, Vector3 angle){
            var worldMatrix = Common.GetWorldTranslation(position, angle, Length);

            foreach (var deck in Decks){
                deck.WorldMatrix = worldMatrix;
            }
            foreach (var layer in HullLayers){
                layer.WorldMatrix = worldMatrix;
            }
        }

    }
}
