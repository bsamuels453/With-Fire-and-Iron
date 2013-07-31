#region

using System;
using System.Diagnostics;
using System.Linq;
using Forge.Core.Util;
using Forge.Framework;
using Forge.Framework.Draw;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;

#endregion

namespace Forge.Core.Terrain{
    public class TerrainChunk : IDisposable{
        public XZPoint Identifier;

        readonly GeometryBuffer<VertexPositionTexture> _buffer;
#if WIREFRAME_OVERLAY
        readonly GeometryBuffer<VertexPositionTexture> _wbuff;
#endif
        readonly VertexPositionTexture[] _verticies;
        readonly int[] _indicies;
        readonly Texture2D _normals;
        readonly Texture2D _binormals;
        readonly Texture2D _tangents;

        public TerrainChunk(XZPoint identifier, VertexPositionTexture[] verticies, int[] indicies, Texture2D normals, Texture2D binormals, Texture2D tangents) {
            Identifier = identifier;
            _verticies = verticies;
            _indicies = indicies;
            _normals = normals;
            _binormals = binormals;
            _tangents = tangents;
            lock (Resource.Device){
                _buffer = new GeometryBuffer<VertexPositionTexture>(indicies.Length, verticies.Count(), indicies.Count()/3, "Config/Shaders/Terrain.config");
            }
#if WIREFRAME_OVERLAY
            _wbuff = new GeometryBuffer<VertexPositionTexture>(indicies.Count()*2, verticies.Count(), indicies.Count(), "Shader_Wireframe", PrimitiveType.LineList);
            _wbuff.ShaderParams["Alpha"].SetValue(0.25f);
#endif

            _buffer.SetIndexBufferData((int[]) _indicies.Clone());
            _buffer.SetVertexBufferData(_verticies);
            _buffer.ShaderParams["tex_Normalmap"].SetValue(_normals);
            _buffer.ShaderParams["tex_Binormalmap"].SetValue(_binormals);
            _buffer.ShaderParams["tex_Tangentmap"].SetValue(_tangents);
#if WIREFRAME_OVERLAY
    //we need to explode the indice list from triangle list to line list
            var wireframeInds = new int[_indicies.Length * 2];
            int srcIdx = 0;
            for (int i = 0; i < _indicies.Length * 2; i += 6) {
                wireframeInds[i] = _indicies[srcIdx];
                wireframeInds[i + 1] = _indicies[srcIdx + 1];
                wireframeInds[i + 2] = _indicies[srcIdx + 1];
                wireframeInds[i + 3] = _indicies[srcIdx + 2];
                wireframeInds[i + 4] = _indicies[srcIdx + 2];
                wireframeInds[i + 5] = _indicies[srcIdx];
                srcIdx += 3;
            }

            _wbuff.IndexBuffer.SetData(wireframeInds);
            _wbuff.VertexBuffer.SetData(_verticies);
#endif

            _normals.Dispose();
            _binormals.Dispose();
            _tangents.Dispose();
        }

        bool _disposed;

        public void Dispose(){
            Debug.Assert(!_disposed);
            _buffer.Dispose();
#if WIREFRAME_OVERLAY
            _wbuff.Dispose();
#endif
            _disposed = true;
        }

        ~TerrainChunk(){
            if (!_disposed){
                DebugConsole.WriteLine("Chunk finalized before being disposed:" + Identifier.X + "," + Identifier.Z);
                Debug.Assert(_disposed);
                //Dispose();
            }
        }
    }
}