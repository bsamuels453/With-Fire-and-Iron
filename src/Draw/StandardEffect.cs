#region

using Gondola.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Gondola.Draw{
    internal abstract class StandardEffect : BaseBufferObject<VertexPositionNormalTexture>{
        protected StandardEffect(int numIndicies, int numVerticies, int numPrimitives, string textureName) :
            base(numIndicies, numVerticies, numPrimitives, PrimitiveType.TriangleList){
            var texture = Gbl.ContentManager.Load<Texture2D>(Gbl.RawLookup[textureName]);
            BufferEffect = Gbl.ContentManager.Load<Effect>(Gbl.RawLookup["StandardEffect"]).Clone();
            BufferEffect.Parameters["Projection"].SetValue(Gbl.ProjectionMatrix);
            BufferEffect.Parameters["World"].SetValue(Matrix.Identity);
            BufferEffect.Parameters["Texture"].SetValue(texture);
            BufferEffect.Parameters["AmbientIntensity"].SetValue(1);
            BufferEffect.Parameters["DiffuseIntensity"].SetValue(1);
            BufferEffect.Parameters["DiffuseLightDirection"].SetValue(new Vector3(0, -1, 1));
        }

        public CullMode CullMode{
            set{
                BufferRasterizer = new RasterizerState();
                BufferRasterizer.CullMode = value;
            }
        }

        public Vector3 DiffuseDirection{
            set { BufferEffect.Parameters["DiffuseLightDirection"].SetValue(value); }
        }

        public float AmbientIntensity{
            set { BufferEffect.Parameters["AmbientIntensity"].SetValue(value); }
        }

        public Vector3 AmbientColor{
            set { BufferEffect.Parameters["AmbientColor"].SetValue(value); }
        }

        public float DiffuseColor{
            set { BufferEffect.Parameters["DiffuseColor"].SetValue(value); }
        }

        public Texture2D Texture{
            set { BufferEffect.Parameters["Texture"].SetValue(value); }
        }
    }
}