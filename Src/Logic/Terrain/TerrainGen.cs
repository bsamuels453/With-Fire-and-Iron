﻿#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Cloo;
using Gondola.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = System.Drawing.Color;

#endregion

namespace Gondola.Logic.Terrain{
    internal class TerrainGen{

        readonly int _blockWidth;
        readonly int _chunkWidthInVerts;
        readonly int _chunkWidthInBlocks;
        readonly ComputeCommandQueue _cmdQueue;

        readonly ComputeProgram _generationPrgm;
        readonly ComputeKernel _terrainGenKernel;
        readonly ComputeKernel _normalGenKernel;
        readonly ComputeBuffer<float> _genConstants;
        readonly ComputeBuffer<byte> _binormals;
        readonly ComputeBuffer<byte> _tangents;
        readonly ComputeBuffer<byte> _normals;
        readonly ComputeBuffer<float> _uvCoords;

        readonly ComputeProgram _qTreePrgm;
        readonly ComputeKernel _qTreeKernel;
        readonly ComputeKernel _crossCullKernel;
        readonly ComputeBuffer<byte> _activeVerts;
        readonly ComputeBuffer<int> _dummy;

        readonly ComputeProgram _winderPrgm;
        readonly ComputeKernel _winderKernel;
        readonly ComputeBuffer<int> _indicies; 

        readonly ComputeContext _context;
        readonly List<ComputeDevice> _devices;
        readonly ComputeBuffer<float> _geometry;
        readonly ComputeContextPropertyList _properties;

        public TerrainGen(){
            var platform = ComputePlatform.Platforms[1];
            _devices = new List<ComputeDevice>();
            _devices.Add(platform.Devices[0]);

            _properties = new ComputeContextPropertyList(platform);
            _context = new ComputeContext(_devices, _properties, null, IntPtr.Zero);
            _cmdQueue = new ComputeCommandQueue(_context, _devices[0], ComputeCommandQueueFlags.None);

            #region setup generator kernel

            bool loadFromSource = Gbl.HasRawHashChanged[Gbl.RawDir.Scripts];

            _chunkWidthInBlocks = Gbl.LoadContent<int>("TGen_ChunkWidthInBlocks");
            _chunkWidthInVerts = _chunkWidthInBlocks + 1;
            _blockWidth = Gbl.LoadContent<int>("TGen_BlockWidthInMeters");
            float lacunarity = Gbl.LoadContent<float>("TGen_Lacunarity");
            float gain = Gbl.LoadContent<float>("TGen_Gain");
            int octaves = Gbl.LoadContent<int>("TGen_Octaves");
            float offset = Gbl.LoadContent<float>("TGen_Offset");
            float hScale = Gbl.LoadContent<float>("TGen_HScale");
            float vScale = Gbl.LoadContent<float>("TGen_VScale");

            _genConstants = new ComputeBuffer<float>(_context, ComputeMemoryFlags.ReadOnly, 7);
            var genArr = new[]{
                lacunarity,
                gain,
                offset,
                octaves,
                hScale,
                vScale,
                _blockWidth
            };

            _cmdQueue.WriteToBuffer(genArr, _genConstants, false, null);
            if (loadFromSource) {
                _generationPrgm = new ComputeProgram(_context, Gbl.LoadScript("TGen_Generator"));
                //_generationPrgm.Build(null, "", null, IntPtr.Zero);//use option -I + scriptDir for header search
                _generationPrgm.Build(null, @"-g -s D:\Projects\Gondola\Raw\Scripts\GenTerrain.cl", null, IntPtr.Zero);//use option -I + scriptDir for header search
                Gbl.SaveBinary(_generationPrgm.Binaries, "TGen_Generator");
            }
            else{
                var binary = Gbl.LoadBinary("TGen_Generator");
                _generationPrgm = new ComputeProgram(_context, binary, _devices);
                _generationPrgm.Build(null, "", null, IntPtr.Zero);
            }
            //loadFromSource = false;
            
            _terrainGenKernel = _generationPrgm.CreateKernel("GenTerrain");
            _normalGenKernel = _generationPrgm.CreateKernel("GenNormals");


            //despite the script using float3 for these fields, we need to consider it to be float4 because the 
            //implementation is basically a float4 wrapper that uses zero for the last variable
            _geometry = new ComputeBuffer<float>(_context, ComputeMemoryFlags.None, _chunkWidthInVerts * _chunkWidthInVerts * 4);
            _normals = new ComputeBuffer<byte>(_context, ComputeMemoryFlags.None, _chunkWidthInVerts * _chunkWidthInVerts * 4);
            _binormals = new ComputeBuffer<byte>(_context, ComputeMemoryFlags.None, _chunkWidthInVerts * _chunkWidthInVerts * 4);
            _tangents = new ComputeBuffer<byte>(_context, ComputeMemoryFlags.None, _chunkWidthInVerts * _chunkWidthInVerts * 4);
            _uvCoords = new ComputeBuffer<float>(_context, ComputeMemoryFlags.None, _chunkWidthInVerts * _chunkWidthInVerts * 2);

            _terrainGenKernel.SetMemoryArgument(0, _genConstants);
            _terrainGenKernel.SetMemoryArgument(3, _geometry);
            _terrainGenKernel.SetMemoryArgument(4, _uvCoords);

            _normalGenKernel.SetMemoryArgument(0, _genConstants);
            _normalGenKernel.SetMemoryArgument(3, _geometry);
            _normalGenKernel.SetMemoryArgument(4, _normals);
            _normalGenKernel.SetMemoryArgument(5, _binormals);
            _normalGenKernel.SetMemoryArgument(6, _tangents);


            #endregion

            #region setup quadtree kernel

            if (loadFromSource) {
                _qTreePrgm = new ComputeProgram(_context, Gbl.LoadScript("TGen_QTree"));
                _qTreePrgm.Build(null, "", null, IntPtr.Zero);
                Gbl.SaveBinary(_qTreePrgm.Binaries, "TGen_QTree");
            }
            else {
                var binary = Gbl.LoadBinary("TGen_QTree");
                _qTreePrgm = new ComputeProgram(_context, binary, _devices);
                _qTreePrgm.Build(null, "", null, IntPtr.Zero);
            }

            _qTreeKernel = _qTreePrgm.CreateKernel("QuadTree");
            _crossCullKernel = _qTreePrgm.CreateKernel("CrossCull");

            _activeVerts = new ComputeBuffer<byte>(_context, ComputeMemoryFlags.None, _chunkWidthInVerts * _chunkWidthInVerts);

            _dummy = new ComputeBuffer<int>(_context, ComputeMemoryFlags.None, 50);
            var rawNormals = new byte[_chunkWidthInVerts*_chunkWidthInVerts*4];
            var dsada = new byte[_chunkWidthInVerts * _chunkWidthInVerts];
            for (int i = 0; i < dsada.Length; i++){
                dsada[i] = 1;
            }
            _cmdQueue.WriteToBuffer(rawNormals, _normals, true, null);
            _cmdQueue.WriteToBuffer(dsada, _activeVerts, true, null);

            _qTreeKernel.SetValueArgument(0, _chunkWidthInBlocks);
            _qTreeKernel.SetMemoryArgument(2, _normals);
            _qTreeKernel.SetMemoryArgument(3, _activeVerts);
            _qTreeKernel.SetMemoryArgument(4, _dummy);

            _crossCullKernel.SetValueArgument(0, _chunkWidthInBlocks);
            _crossCullKernel.SetMemoryArgument(2, _normals);
            _crossCullKernel.SetMemoryArgument(3, _activeVerts);
            _crossCullKernel.SetMemoryArgument(4, _dummy);

            #endregion

            #region setup winding kernel

            if (loadFromSource) {
                _winderPrgm = new ComputeProgram(_context, Gbl.LoadScript("TGen_VertexWinder"));
                _winderPrgm.Build(null, "", null, IntPtr.Zero);
                //_winderPrgm.Build(null, @"-g -s D:\Projects\Gondola\Raw\Scripts\VertexWinder.cl", null, IntPtr.Zero);
                Gbl.SaveBinary(_winderPrgm.Binaries, "TGen_VertexWinder");
            }
            else {
                var binary = Gbl.LoadBinary("TGen_VertexWinder");
                _winderPrgm = new ComputeProgram(_context, binary, _devices);
                _winderPrgm.Build(null, "", null, IntPtr.Zero);
            }
            
            _winderKernel = _winderPrgm.CreateKernel("VertexWinder");

            _indicies = new ComputeBuffer<int>(_context, ComputeMemoryFlags.None, (_chunkWidthInBlocks) * (_chunkWidthInBlocks) * 8);

            _winderKernel.SetMemoryArgument(0, _activeVerts);
            _winderKernel.SetMemoryArgument(1, _indicies);

            #endregion

            if (loadFromSource){
                Gbl.AllowMD5Refresh[Gbl.RawDir.Scripts] = true;
            }

            _cmdQueue.Finish();
        }

        public TerrainChunk GenerateChunk(XZPair id) {
            var sw = new Stopwatch();
            sw.Start();

            int offsetX = id.X * _blockWidth * (_chunkWidthInBlocks);
            int offsetZ = id.Z * _blockWidth * (_chunkWidthInBlocks);

            var rawGeometry = new float[_chunkWidthInVerts*_chunkWidthInVerts*4];
            var rawNormals = new byte[_chunkWidthInVerts*_chunkWidthInVerts*4];
            var rawBinormals = new byte[_chunkWidthInVerts*_chunkWidthInVerts*4];
            var rawTangents = new byte[_chunkWidthInVerts*_chunkWidthInVerts*4];
            var rawUVCoords = new float[_chunkWidthInVerts * _chunkWidthInVerts * 2];
            var indicies = new int[(_chunkWidthInBlocks) * (_chunkWidthInBlocks) * 8];

            _terrainGenKernel.SetValueArgument(1, offsetX);
            _terrainGenKernel.SetValueArgument(2, offsetZ);
            _normalGenKernel.SetValueArgument(1, offsetX);
            _normalGenKernel.SetValueArgument(2, offsetZ);

            _cmdQueue.Execute(_terrainGenKernel, null, new long[] { _chunkWidthInVerts, _chunkWidthInVerts }, null, null);
            _cmdQueue.AddBarrier();
            _cmdQueue.Execute(_normalGenKernel, null, new long[] { _chunkWidthInVerts, _chunkWidthInVerts }, null, null);

            for (int depth = 0; depth < 1; depth++){
                _qTreeKernel.SetValueArgument(1, depth);
                _crossCullKernel.SetValueArgument(1, depth);
                int cellWidth = depth * 2 + 2;
                int qTreeWidth = _chunkWidthInBlocks / (cellWidth * 2);
                _cmdQueue.Execute(_qTreeKernel, null, new long[] { (qTreeWidth*2) - 1, (qTreeWidth) }, null, null);
                _cmdQueue.Execute(_crossCullKernel, null, new long[] { _chunkWidthInBlocks / cellWidth, _chunkWidthInBlocks / cellWidth }, null, null);
            }
            _cmdQueue.Execute(_winderKernel, null, new long[] { (_chunkWidthInBlocks), (_chunkWidthInBlocks * 2) }, null, null);

            _cmdQueue.ReadFromBuffer(_geometry, ref rawGeometry, true, null);
            _cmdQueue.ReadFromBuffer(_normals, ref rawNormals, true, null);
            _cmdQueue.ReadFromBuffer(_binormals, ref rawBinormals, true, null);
            _cmdQueue.ReadFromBuffer(_tangents, ref rawTangents, true, null);
            _cmdQueue.ReadFromBuffer(_uvCoords, ref rawUVCoords, true, null);
            _cmdQueue.ReadFromBuffer(_indicies, ref indicies, true, null);
            _cmdQueue.Finish();

            var texNormal = new Texture2D(Gbl.Device, _chunkWidthInVerts, _chunkWidthInVerts, false, SurfaceFormat.Color);
            var texBinormal = new Texture2D(Gbl.Device, _chunkWidthInVerts, _chunkWidthInVerts, false, SurfaceFormat.Color);
            var texTangent = new Texture2D(Gbl.Device, _chunkWidthInVerts, _chunkWidthInVerts, false, SurfaceFormat.Color);

            for (int v = 3; v < rawNormals.Length; v+=4){
                rawNormals[v] = 1;
            }
            for (int v = 3; v < rawBinormals.Length; v += 4) {
                rawBinormals[v] = 1;
            }
            for (int v = 3; v < rawTangents.Length; v += 4) {
                rawTangents[v] = 1;
            }

            texNormal.SetData(rawNormals);
            texBinormal.SetData(rawBinormals);
            texTangent.SetData(rawTangents);

            var verts = new Vector3[_chunkWidthInVerts*_chunkWidthInVerts];
            var uv = new Vector2[_chunkWidthInBlocks*_chunkWidthInBlocks*2];

            int si = 0;
            for (int i = 0; i < _chunkWidthInVerts * _chunkWidthInVerts; i++){
                verts[i] = new Vector3(rawGeometry[si], rawGeometry[si + 1], rawGeometry[si + 2]);
                si += 4;
            }

            //var maxHeight = verts.Aggregate((agg, next) => next.Y > agg.Y ? next : agg).Y;
            //var minHeight = verts.Aggregate((agg, next) => next.Y < agg.Y ? next : agg).Y;

            si = 0;
            for (int v = 0; v < _chunkWidthInVerts * _chunkWidthInVerts; v++) {
                uv[v]= new Vector2(rawUVCoords[si],rawUVCoords[si+1]);
                si += 2;
            }
            var fixedInds = new List<int>();
            for(int idx=0; idx<indicies.Length; idx+=4){
                fixedInds.Add(indicies[idx]);
                 fixedInds.Add(indicies[idx+1]);
                 fixedInds.Add(indicies[idx+2]);
            }

            var vertexList = new VertexPositionTexture[_chunkWidthInVerts * _chunkWidthInVerts];
            for (int i = 0; i < _chunkWidthInVerts * _chunkWidthInVerts; i++){
                vertexList[i] = new VertexPositionTexture(verts[i],uv[i] );
            }

            var chunkData = new TerrainChunk(id, vertexList, fixedInds.ToArray(), texNormal, texBinormal, texTangent);

            sw.Stop();
            double elapsed = sw.ElapsedMilliseconds;
            return chunkData;
        }
    }
}