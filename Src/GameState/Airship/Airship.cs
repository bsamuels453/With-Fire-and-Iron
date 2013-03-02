using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gondola.Draw;
using Gondola.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gondola.GameState.Airship {
    internal class Airship {
        public float Length;
        public float AscentSpeed;
        public float MovementSpeed;
        public float TurnSpeed;

        public Vector3 Position;

        public Vector3 Centroid;
        public GeometryBuffer<VertexPositionNormalTexture>[] Decks;
        public GeometryBuffer<VertexPositionNormalTexture>[] HullLayers;

        public AscentState CurAscentState;
        public TurnState CurTurnState;

        public bool DepthPumpsActive;
        public bool EnginesActive;

        public Airship(){
            Length = 50;
            AscentSpeed = 1;
            MovementSpeed = 1;
            TurnSpeed = 0.02f;
        }


        public enum AscentState {
            Ascending,
            Descending,
            Stable
        }

        public enum TurnState {
            TurningLeft,
            TurningRight,
            Stable
        }

        public void Update(ref InputState state, double timeDelta){

        }
    }
}
