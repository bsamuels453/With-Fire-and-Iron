using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forge.Framework.Draw;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Forge.Core.Logic {
    class ProjectilePathTracer {
        GeometryBuffer<VertexPositionNormalTexture> _pathBuff;

        public Matrix WorldMatrix { get; set; } //propagate to buffer

        public ProjectilePathTracer(Vector3 origin, Vector3 initlVelocity){
            //setup private physics instance and simulate path

            //then generate the path using GenerateCube+matrixing to rotate path breadcrumbs
            throw new NotImplementedException();
        }

        public bool Enabled { get; set; }
    }
}
