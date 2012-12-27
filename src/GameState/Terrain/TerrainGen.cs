#region

using System;
using System.Collections.Generic;
using System.Drawing;
using Cloo;
using Gondola.Logic;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Gondola.GameState.Terrain{
    internal class TerrainGen{

        readonly int _blockWidth;
        readonly int _chunkWidthInVerts;
        readonly ComputeCommandQueue _cmdQueue;

        readonly ComputeBuffer<float> _constants;
        readonly ComputeContext _context;
        readonly List<ComputeDevice> _devices;
        readonly ComputeBuffer<float> _geometry;
        readonly ComputeKernel _kernel;
        readonly ComputeProgram _program;
        readonly ComputeContextPropertyList _properties;

        readonly ComputeBuffer<byte> _normals;
        readonly ComputeBuffer<byte> _binormals;
        readonly ComputeBuffer<byte> _tangents;

        public TerrainGen(){
            _chunkWidthInVerts = Gbl.LoadContent<int>("TGen_ChunkWidthInBlocks") + 1;
            _blockWidth = Gbl.LoadContent<int>("TGen_BlockWidthInMeters");
            float lacunarity = Gbl.LoadContent<float>("TGen_Lacunarity");
            float gain = Gbl.LoadContent<float>("TGen_Gain");
            int octaves = Gbl.LoadContent<int>("TGen_Octaves");
            float offset = Gbl.LoadContent<float>("TGen_Offset");
            float hScale = Gbl.LoadContent<float>("TGen_HScale");
            float vScale = Gbl.LoadContent<float>("TGen_VScale");

            var platform = ComputePlatform.Platforms[0];
            _devices = new List<ComputeDevice>();
            _devices.Add(platform.Devices[0]);

            _properties = new ComputeContextPropertyList(platform);
            _context = new ComputeContext(_devices, _properties, null, IntPtr.Zero);

            _cmdQueue = new ComputeCommandQueue(_context, _devices[0], ComputeCommandQueueFlags.None);

            _constants = new ComputeBuffer<float>(_context, ComputeMemoryFlags.ReadOnly, 7);
            var constArr = new[]{
                lacunarity,
                gain,
                offset,
                octaves,
                hScale,
                vScale,
                _blockWidth
            };

            _cmdQueue.WriteToBuffer(constArr, _constants, false, null);

            _program = new ComputeProgram(_context, Gbl.LoadScript("TGen_GenScript"));
            _program.Build(null, null, null, IntPtr.Zero);
            _kernel = _program.CreateKernel("GenTerrain");

            //despite the script using float3 for these fields, we need to consider it to be float4 because the 
            //implementation is basically a float4 wrapper that uses zero for the last variable
            _geometry = new ComputeBuffer<float>(_context, ComputeMemoryFlags.ReadWrite, _chunkWidthInVerts*_chunkWidthInVerts*4);
            _normals = new ComputeBuffer<byte>(_context, ComputeMemoryFlags.WriteOnly, _chunkWidthInVerts * _chunkWidthInVerts * 4);
            _binormals = new ComputeBuffer<byte>(_context, ComputeMemoryFlags.WriteOnly, _chunkWidthInVerts * _chunkWidthInVerts * 4);
            _tangents = new ComputeBuffer<byte>(_context, ComputeMemoryFlags.WriteOnly, _chunkWidthInVerts * _chunkWidthInVerts * 4);

            _kernel.SetMemoryArgument(0, _constants);
            _kernel.SetMemoryArgument(3, _geometry);
            _kernel.SetMemoryArgument(4, _normals);
            _kernel.SetMemoryArgument(5, _binormals);
            _kernel.SetMemoryArgument(6, _tangents);

            _cmdQueue.Finish();
        }

        public void Generate(int offsetX, int offsetZ){
            var rawGeometry = new float[_chunkWidthInVerts*_chunkWidthInVerts * 4];
            var rawNormals = new byte[_chunkWidthInVerts * _chunkWidthInVerts * 4];
            var rawBinormals = new byte[_chunkWidthInVerts * _chunkWidthInVerts * 4];
            var rawTangents = new byte[_chunkWidthInVerts * _chunkWidthInVerts * 4];

            _kernel.SetValueArgument(1, offsetX);
            _kernel.SetValueArgument(2, offsetZ);

            _cmdQueue.Execute(_kernel, null, new long[]{_chunkWidthInVerts, _chunkWidthInVerts}, null, null);
            _cmdQueue.ReadFromBuffer(_geometry, ref rawGeometry, true, null);
            _cmdQueue.ReadFromBuffer(_normals, ref rawNormals, true, null);
            _cmdQueue.ReadFromBuffer(_binormals, ref rawBinormals, true, null);
            _cmdQueue.ReadFromBuffer(_tangents, ref rawTangents, true, null);


            _cmdQueue.Finish();

            var colorNormals = new Color[_chunkWidthInVerts * _chunkWidthInVerts];
            var colorBinormals = new Color[_chunkWidthInVerts * _chunkWidthInVerts];
            var colorTangents = new Color[_chunkWidthInVerts * _chunkWidthInVerts];

            //be careful not to thrash the cache
            int rawIndex = 0;
            for (int i = 0; i < _chunkWidthInVerts * _chunkWidthInVerts; i++) {
                colorNormals[i] = Color.FromArgb(
                    rawNormals[rawIndex],
                    rawNormals[rawIndex + 1],
                    rawNormals[rawIndex + 2]
                    );
                rawIndex += 4;
            }

            rawIndex = 0;
            for (int i = 0; i < _chunkWidthInVerts * _chunkWidthInVerts; i++) {
                colorBinormals[i] = Color.FromArgb(
                    rawBinormals[rawIndex],
                    rawBinormals[rawIndex + 1],
                    rawBinormals[rawIndex + 2]
                    );
                rawIndex += 4;
            }

            rawIndex = 0;
            for (int i = 0; i < _chunkWidthInVerts * _chunkWidthInVerts; i++) {
                colorTangents[i] = Color.FromArgb(
                    rawTangents[rawIndex],
                    rawTangents[rawIndex + 1],
                    rawTangents[rawIndex + 2]
                    );
                rawIndex += 4;
            }


            var texNormal = new Texture2D(Gbl.Device, _chunkWidthInVerts, _chunkWidthInVerts, false, SurfaceFormat.Vector4);
            var texBinormal = new Texture2D(Gbl.Device, _chunkWidthInVerts, _chunkWidthInVerts, false, SurfaceFormat.Vector4);
            var texTangent = new Texture2D(Gbl.Device, _chunkWidthInVerts, _chunkWidthInVerts, false, SurfaceFormat.Vector4);

            texNormal.SetData(colorNormals);
            texBinormal.SetData(colorBinormals);
            texTangent.SetData(colorTangents);

            int f = 3;
        }
    }
}