using System;
using System.Diagnostics;
using System.Linq;
using Gondola.Common;
using Gondola.Draw;
using Microsoft.Xna.Framework.Graphics;

namespace Gondola.Logic.Terrain {
    class TerrainChunk : IDisposable{
        public XZPair Identifier;

        readonly TerrainBuffer _buffer;
        readonly VertexPositionTexture[] _verticies;
        readonly int[] _indicies;
        readonly Texture2D _normals;
        readonly Texture2D _binormals;
        readonly Texture2D _tangents;

        bool _bufferDataSet;

        public TerrainChunk(XZPair identifier, VertexPositionTexture[] verticies, int[] indicies, Texture2D normals, Texture2D binormals, Texture2D tangents){
            Identifier = identifier;
            _verticies = verticies;
            _indicies = indicies;
            _normals = normals;
            _binormals = binormals;
            _tangents = tangents;
            _buffer = new TerrainBuffer(indicies.Count(), verticies.Count(), indicies.Count() / 3, PrimitiveType.TriangleList);
        }

        public void SetBufferData(){
            Debug.Assert(_bufferDataSet == false);
            _buffer.SetData(_verticies, _indicies, _normals, _binormals, _tangents);
            _normals.Dispose();
            _binormals.Dispose();
            _tangents.Dispose();
            _bufferDataSet = true;
        }

        public void Dispose(){
            if (!_bufferDataSet){
                _normals.Dispose();
                _binormals.Dispose();
                _tangents.Dispose();
            }
            _buffer.Dispose();
        }
    }
}
