using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gondola.Draw;
using Gondola.Logic;
using Gondola.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Gondola.GameState.Airship {
    internal class Airship{
        public float Length;
        public float MaxAscentSpeed;
        public float MaxMovementSpeed;
        public float MaxTurnSpeed;

        public Vector3 Position;
        public Vector3 Angle;

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

        void SetAirshipPosition(Vector3 position, Vector3 angle){
            var worldMatrix = Matrix.Identity;
            worldMatrix *= Matrix.CreateTranslation(Length/2, 0, 0);
            worldMatrix *= Matrix.CreateRotationX(angle.X) * Matrix.CreateRotationY(angle.Y) * Matrix.CreateRotationZ(angle.Z);
            worldMatrix *= Matrix.CreateTranslation(position.X, position.Y, position.Z);

            foreach (var deck in Decks){
                deck.WorldMatrix = worldMatrix;
            }
            foreach (var layer in HullLayers){
                layer.WorldMatrix = worldMatrix;
            }
        }

    }
}
