using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Forge.Core.Airship.Data;
using Forge.Framework;
using Forge.Framework.Draw;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProtoBuf;

namespace Forge.Core.Airship.Export {
    static class AirshipExporter {
        public static void Import(){
            AirshipExport airship;

            var sw = new Stopwatch();
            sw.Start();
            
            using (var file = File.Open("airship.bin", FileMode.Open)) {
                airship = Serializer.Deserialize<AirshipExport>(file);
            }

            sw.Stop();
            DebugConsole.WriteLine("Airship deserialized in " + sw.ElapsedMilliseconds + " ms");
        }

        public static void Export(string fileName, HullSectionContainer hull, DeckSectionContainer decks, ModelAttributes modelAttribs) {
            DebugConsole.WriteLine("Starting airship serialization...");
            var sw = new Stopwatch();
            sw.Start();

            //hull sections
            var sections = hull.HullSections;
            var hullSectionCtorDat = new List<HullSectionCtorDat>(sections.Length);
            foreach (var section in sections){
                hullSectionCtorDat.Add(new HullSectionCtorDat(section));
            }

            //hull buffers
            var hullBufferCtorDat = new List<HullBufferObjectDataContainer<int>>(hull.NumDecks);
            foreach (var buffer in hull.HullBuffersByDeck) {
                var bufferDump = buffer.DumpObjectData();

                var array = new List<HullBufferObjectData<int>>(bufferDump.Length);
                foreach (var obj in bufferDump){
                    array.Add(new HullBufferObjectData<int>(obj));
                }
                hullBufferCtorDat.Add(new HullBufferObjectDataContainer<int>(array));
            }






            var airship = new AirshipExport();
            airship.HullBufferCtorDat = hullBufferCtorDat;
            airship.HullSectionCtorData = hullSectionCtorDat;
            using (var file = File.Create("airship.bin")) {
                Serializer.Serialize(file, airship);
            }

            sw.Stop();
            DebugConsole.WriteLine("Airship serialization finished in " + sw.ElapsedMilliseconds + " ms");
        }

        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable NotAccessedField.Local

        [ProtoContract]
        struct AirshipExport{
            [ProtoMember(1)]
            public List<HullSectionCtorDat> HullSectionCtorData;
            [ProtoMember(2)]
            public List<HullBufferObjectDataContainer<int>> HullBufferCtorDat; 

        }
        #region hullbuffer wrappers
        [ProtoContract]
        struct HullBufferObjectDataContainer<T>{
            [ProtoMember(1)]
            public List<HullBufferObjectData<T>> HullBufferObjData; 

            public HullBufferObjectDataContainer(List<HullBufferObjectData<T>> hullBufferObjData){
                HullBufferObjData = hullBufferObjData;
            }
        }

        [ProtoContract]
        struct HullBufferObjectData<T>{
            [ProtoMember(1)]
            public T Identifier;
            [ProtoMember(2)]
            public int[] Indicies;
            [ProtoMember(3)]
            public int ObjectOffset;
            [ProtoMember(4)]
            public VertexWrap[] Verticies;
            [ProtoMember(5)]
            public bool Enabled;

            public HullBufferObjectData(ObjectBuffer<int>.ObjectData objData){
                Identifier = (T)objData.Identifier;
                Indicies = objData.Indicies;
                ObjectOffset = objData.ObjectOffset;
                Verticies = new VertexWrap[objData.Verticies.Length];
                for (int i = 0; i < Verticies.Length; i++) {
                    Verticies[i] = objData.Verticies[i];
                }

                Enabled = objData.Enabled;
            }
        }
        #endregion

        #region hull section wrappers
        [ProtoContract] 
        struct HullSectionCtorDat{
            [ProtoMember(1)]
            public int Uid;
            [ProtoMember(2)]
            public Vec3Wrap[] AliasedVertexes;
            [ProtoMember(3)]
            public int Deck;
            [ProtoMember(4)]
            public Quadrant.Side Side;
            [ProtoMember(6)]
            public int YPanel;
            //public ObjectBuffer<int> HullBuffer;

            public HullSectionCtorDat(HullSection section){
                Uid = section.Uid;
                AliasedVertexes = new Vec3Wrap[section.AliasedVertexes.Length];
                for (int i = 0; i < AliasedVertexes.Length; i++){
                    AliasedVertexes[i] = section.AliasedVertexes[i];
                }
                Deck = section.Deck;
                Side = section.Side;
                YPanel = section.YPanel;
            }
        }
        #endregion

        #region xna class wrappers
        [ProtoContract]
        struct Vec3Wrap{
            [ProtoMember(1)]
            public float X;
            [ProtoMember(2)]
            public float Y;
            [ProtoMember(3)]
            public float Z;

            public static implicit operator Vector3(Vec3Wrap obj) {
                return new Vector3(obj.X, obj.Y, obj.Z);
            }
            public static implicit operator Vec3Wrap(Vector3 obj) {
                var ret = new Vec3Wrap();
                ret.X = obj.X;
                ret.Y = obj.Y;
                ret.Z = obj.Z;
                return ret;
            }

            public Vec3Wrap(Vector3 vec){
                X = vec.X;
                Y = vec.Y;
                Z = vec.Z;
            }
        }

        [ProtoContract]
        struct Vec2Wrap {
            [ProtoMember(1)]
            public float X;
            [ProtoMember(2)]
            public float Y;

            public static implicit operator Vec2Wrap(Vector2 obj) {
                var ret = new Vec2Wrap();
                ret.X = obj.X;
                ret.Y = obj.Y;
                return ret;
            }


            public Vec2Wrap(Vector2 vec){
                X = vec.X;
                Y = vec.Y;
            }
        }

        [ProtoContract]
        struct VertexWrap{
            [ProtoMember(1)]
            public Vec3Wrap Position;
            [ProtoMember(2)]
            public Vec3Wrap Normal;
            [ProtoMember(3)]
            public Vec2Wrap TextureCoordinate;


            public static implicit operator VertexWrap(VertexPositionNormalTexture obj) {
                var ret = new VertexWrap();
                ret.Position = obj.Position;
                ret.Normal = obj.Normal;
                ret.TextureCoordinate = obj.TextureCoordinate;
                return ret;
            }
        }
        #endregion

        // ReSharper restore NotAccessedField.Local
        // ReSharper restore MemberCanBePrivate.Local
    }
}