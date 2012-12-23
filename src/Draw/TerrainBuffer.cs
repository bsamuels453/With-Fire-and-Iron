#region

using Gondola.Common;
using Gondola.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Gondola.Draw{
    class TerrainBuffer : BaseBufferObject<VertexPositionNormalTexture>{
        public TerrainBuffer(int numIndicies, int numVerticies, int numPrimitives, PrimitiveType primitiveType) :
            base(numIndicies, numVerticies, numPrimitives, primitiveType){
            BufferEffect = Gbl.LoadContent<Effect>("TerrainEffect").Clone();
            BufferEffect.Parameters["GrassTexture"].SetValue(Gbl.LoadContent<Texture2D>("TRend_GrassTexture"));
            BufferEffect.Parameters["DirtTexture"].SetValue(Gbl.LoadContent<Texture2D>("TRend_DirtTexture"));
            BufferEffect.Parameters["RockTexture"].SetValue(Gbl.LoadContent<Texture2D>("TRend_RockTexture"));
            BufferEffect.Parameters["IceTexture"].SetValue(Gbl.LoadContent<Texture2D>("TRend_IceTexture"));
            BufferEffect.Parameters["TreeTexture"].SetValue(Gbl.LoadContent<Texture2D>("TRend_TreeTexture"));
            BufferEffect.Parameters["TreeBumpTexture"].SetValue(Gbl.LoadContent<Texture2D>("TRend_TreeTexNormalMap"));
            BufferEffect.Parameters["RockBumpTexture"].SetValue(Gbl.LoadContent<Texture2D>("TRend_RockTexNormalMap"));

            BufferEffect.Parameters["Projection"].SetValue(Gbl.ProjectionMatrix);
            BufferEffect.Parameters["World"].SetValue(Matrix.Identity);

            BufferEffect.Parameters["AmbientColor"].SetValue(Gbl.LoadContent<Vector4>("TRend_AmbientColor"));
            BufferEffect.Parameters["AmbientIntensity"].SetValue(Gbl.LoadContent<float>("TRend_AmbientIntensity"));
            BufferEffect.Parameters["TextureScalingFactor"].SetValue(Gbl.LoadContent<int>("TRend_TexScaleFactor"));

            BufferEffect.Parameters["DiffuseIntensity"].SetValue(Gbl.LoadContent<float>("TRend_DiffuseIntensity"));
            BufferEffect.Parameters["DiffuseLightDirection"].SetValue(Gbl.LoadContent<Vector3>("TRend_DiffuseDirection"));
        }

        public CullMode CullMode{
            set{
                BufferRasterizer = new RasterizerState();
                BufferRasterizer.CullMode = value;
            }
        }

        public Texture2D NormalMapTex{
            set { BufferEffect.Parameters["NormalMapTexture"].SetValue(value); }
        }

        public Texture2D BiNormalMapTex{
            set { BufferEffect.Parameters["BinormalMapTexture"].SetValue(value); }
        }

        public Texture2D TangentMapTex{
            set { BufferEffect.Parameters["TangentMapTexture"].SetValue(value); }
        }

        public Texture2D TerrainMask{
            set { BufferEffect.Parameters["TerrainCanvasTexture"].SetValue(value); }
        }

        public Texture2D InorganicMask{
            set { BufferEffect.Parameters["InorganicCanvasTexture"].SetValue(value); }
        }

        public Texture2D FoliageMask{
            set { BufferEffect.Parameters["FoliageCanvasTexture"].SetValue(value); }
        }

        public Texture2D TreeMask{
            set { BufferEffect.Parameters["TreeCanvasTexture"].SetValue(value); }
        }
    }
}