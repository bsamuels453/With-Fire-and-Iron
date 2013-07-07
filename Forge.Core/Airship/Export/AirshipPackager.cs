﻿#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Forge.Core.Airship.Data;
using Forge.Core.Airship.Generation;
using Forge.Core.Util;
using Forge.Framework;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProtoBuf;

#endregion

namespace Forge.Core.Airship.Export{
    public static class AirshipPackager{
        const string _stateDataSerializerVersion = "0.1";

        /// <summary>
        ///   A cache of the previously loaded airship models.
        /// </summary>
        static readonly Dictionary<string, AirshipSerializationStruct> _modelCache;

        /// <summary>
        ///   The next id value to be used in a generated airship.
        /// </summary>
        static readonly AirshipUidGenerator _uidGenerator;

        static AirshipPackager(){
            _modelCache = new Dictionary<string, AirshipSerializationStruct>();
            _uidGenerator = new AirshipUidGenerator();
        }

        /// <summary>
        ///   Loads an airship's model/state and instantiates it.
        /// </summary>
        /// <param name="stateName"> The filename of the airship's state. </param>
        /// <param name="battlefield"> Class that contains data pertaining to the state of the battlefield. </param>
        /// <returns> </returns>
        public static Airship LoadAirship(string stateName, Battlefield battlefield){
            DebugConsole.WriteLine("Loading airship as defined by: " + stateName + ".json");
            var stateReader = new StreamReader(Directory.GetCurrentDirectory() + "/Data/" + stateName + ".json");
            var stateData = DeserializeStateFromReader(stateReader);
            stateReader.Close();

            var airship = LoadAirshipModel(stateData.Model);

            var hullSections = new HullSectionContainer(airship.HullSections);
            var deckSections = new DeckSectionContainer(airship.DeckSections);
            var modelAttribs = airship.ModelAttributes;

            var ret = new Airship(modelAttribs, deckSections, hullSections, stateData, battlefield);
            DebugConsole.WriteLine(stateName + " loading completed");
            return ret;
        }

        /// <summary>
        ///   Generates an airship using provided model and statedata.
        /// </summary>
        /// <param name="stateDataName"> </param>
        /// <param name="stateData"> The state data that will be used to initalize the airship. If you specify an Id in this structure, it will be ignored. </param>
        /// <param name="battlefield"> Class that contains data pertaining to the state of the battlefield. </param>
        /// <returns> </returns>
        public static Airship GenerateNewAirship(string stateDataName, AirshipStateData stateData, Battlefield battlefield){
            stateData.AirshipId = _uidGenerator.NextUid();
            DebugConsole.WriteLine("New airship being generated with uid " + stateData.AirshipId);

            var model = LoadAirshipModel(stateData.Model);

            var writer = new StreamWriter(Directory.GetCurrentDirectory() + "/Data/" + stateDataName + ".json");

            SerializeStateToWriter(stateData, writer);

            return InstantiateAirshipFromSerialized(model, stateData, battlefield);
        }

        /// <summary>
        ///   Converts an airship to protocol format based on its hullSection/deckSection/modelAttributes
        /// </summary>
        /// <param name="fileName"> The name of the output file, no extension. Base directory is /Data/AirshipSchematics/ </param>
        /// <param name="hullSectionContainer"> </param>
        /// <param name="deckSectionContainer"> </param>
        /// <param name="attributes"> </param>
        public static void ExportToProtocolFile(string fileName, HullSectionContainer hullSectionContainer, DeckSectionContainer deckSectionContainer,
            ModelAttributes attributes){
            var sw = new Stopwatch();
            sw.Start();
            var fs = new FileStream(Directory.GetCurrentDirectory() + "/Data/AirshipSchematics/" + fileName + ".protocol", FileMode.Create);
            var sections = hullSectionContainer.ExtractSerializationStruct();
            var decks = deckSectionContainer.ExtractSerializationStruct();
            var aship = new AirshipSerializationStruct();
            aship.DeckSections = decks;
            aship.HullSections = sections;
            aship.ModelAttributes = attributes;
            Serializer.Serialize(fs, aship);
            fs.Close();
            sw.Stop();
            DebugConsole.WriteLine("Airship serialized to protocol in " + sw.ElapsedMilliseconds + " ms");
        }

        /// <summary>
        ///   Exports an airship from bezier to definition format.
        /// </summary>
        /// <param name="fileName"> The name of the output file, no extension. Base directory is /Data/AirshipSchematics/ </param>
        /// <param name="backCurveInfo"> </param>
        /// <param name="sideCurveInfo"> </param>
        /// <param name="topCurveInfo"> </param>
        public static void ExportAirshipDefinitionToFile(string fileName, BezierInfo[] backCurveInfo, BezierInfo[] sideCurveInfo, BezierInfo[] topCurveInfo){
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            JObject jObj = new JObject();
            jObj["FrontBezierSurf"] = JToken.FromObject(backCurveInfo);
            jObj["SideBezierSurf"] = JToken.FromObject(sideCurveInfo);
            jObj["TopBezierSurf"] = JToken.FromObject(topCurveInfo);

            var sw = new StreamWriter(Directory.GetCurrentDirectory() + "/Data/AirshipSchematics/" + fileName + ".def");
            sw.Write(JsonConvert.SerializeObject(jObj, Formatting.Indented));
            sw.Close();
            stopwatch.Stop();
            DebugConsole.WriteLine("Airship serialized to definition in " + stopwatch.ElapsedMilliseconds + " ms");
        }

        /// <summary>
        ///   Loads an airship's model from disc, or cache if avaliable.
        /// </summary>
        /// <param name="modelName"> The name of the model's file, without extension. </param>
        /// <returns> </returns>
        static AirshipSerializationStruct LoadAirshipModel(string modelName){
            AirshipSerializationStruct airship;
            if (_modelCache.ContainsKey(modelName)){
                airship = _modelCache[modelName];
                DebugConsole.WriteLine("Airship serialization structure loaded from cache");
            }
            else{
#if CONVERT_TO_PROTOCOL
                ConvertDefToProtocolFile(modelName);
#endif
                DebugConsole.WriteLine("Airship serialization structure not in cache, importing protocol...");
                _modelCache.Add
                    (
                        modelName,
                        ImportFromProtocolFromFile(modelName)
                    );
                airship = _modelCache[modelName];
            }
            return airship;
        }

        /// <summary>
        ///   Instantiates a new airship using the provided arguments.
        /// </summary>
        /// <param name="model"> </param>
        /// <param name="stateData"> </param>
        /// <param name="battlefield"> Class that contains data pertaining to the state of the battlefield. </param>
        /// <returns> </returns>
        static Airship InstantiateAirshipFromSerialized(AirshipSerializationStruct model, AirshipStateData stateData, Battlefield battlefield){
            var hullSections = new HullSectionContainer(model.HullSections);
            var deckSections = new DeckSectionContainer(model.DeckSections);
            return new Airship(model.ModelAttributes, deckSections, hullSections, stateData, battlefield);
        }

        /// <summary>
        ///   Used to serialize airship state to a stream defined by the StreamWriter parameter. Closes the writer once it's done.
        /// </summary>
        /// <param name="stateData"> </param>
        /// <param name="writer"> </param>
        static void SerializeStateToWriter(AirshipStateData stateData, StreamWriter writer){
            Debug.Assert(writer != null);
            var jObj = new JObject();
            jObj["Version"] = _stateDataSerializerVersion;
            jObj["Model"] = stateData.Model;
            jObj["AirshipId"] = stateData.AirshipId;
            jObj["FactionId"] = stateData.FactionId;
            jObj["Position"] = JToken.FromObject(stateData.Position);
            jObj["Angle"] = JToken.FromObject(stateData.Angle);
            jObj["AscentRate"] = stateData.AscentRate;
            jObj["TurnRate"] = stateData.TurnRate;
            jObj["Velocity"] = stateData.Velocity;
            jObj["ActiveBuffs"] = JToken.FromObject(stateData.ActiveBuffs);
            jObj["ControllerType"] = JToken.FromObject(stateData.ControllerType);
            jObj["CurrentManeuver"] = JToken.FromObject(stateData.CurrentManeuver);
            jObj["ManeuverParameters"] = JToken.FromObject(stateData.ManeuverParameters);

            writer.Write(JsonConvert.SerializeObject(jObj, Formatting.Indented));
            writer.Close();
            DebugConsole.WriteLine("Airship state data serialized successfully.");
        }

        /// <summary>
        ///   Used to deserialize airship state from the provided reader stream.
        /// </summary>
        /// <param name="reader"> </param>
        /// <returns> </returns>
        static AirshipStateData DeserializeStateFromReader(StreamReader reader){
            var sw = new Stopwatch();
            sw.Start();
            Debug.Assert(reader != null);

            var jObj = Resource.LoadJObject(reader);

            if (!jObj["Version"].ToObject<string>().Equals(_stateDataSerializerVersion)){
                throw new Exception("bad file version");
            }
            var ret = new AirshipStateData();

            ret.Model = jObj["Model"].ToObject<string>();
            ret.AirshipId = jObj["AirshipId"].ToObject<int>();
            ret.FactionId = jObj["FactionId"].ToObject<int>();
            ret.Position = jObj["Position"].ToObject<Vector3>();
            ret.Angle = jObj["Angle"].ToObject<Vector3>();
            ret.AscentRate = jObj["AscentRate"].ToObject<float>();
            ret.TurnRate = jObj["TurnRate"].ToObject<float>();
            ret.Velocity = jObj["Velocity"].ToObject<float>();
            ret.ActiveBuffs = jObj["ActiveBuffs"].ToObject<List<AirshipBuff>>();
            ret.ControllerType = jObj["ControllerType"].ToObject<AirshipControllerType>();
            ret.CurrentManeuver = jObj["CurrentManeuver"].ToObject<ManeuverTypeEnum>();
            ret.ManeuverParameters = jObj["ManeuverParameters"].ToObject<object[]>();
            sw.Stop();
            DebugConsole.WriteLine("Airship state data deserialized in " + sw.ElapsedMilliseconds + " ms");
            return ret;
        }

        /// <summary>
        ///   Converts the airship stored in the specified .def file to .protocol format. Base directory is /Data/AirshipSchematics/
        /// </summary>
        /// <param name="fileName"> The filename of the .def file, without extension </param>
        public static void ConvertDefToProtocolFile(string fileName){
            var sw = new Stopwatch();
            sw.Start();

            var jObj = Resource.LoadJObject(Directory.GetCurrentDirectory() + "/Data/AirshipSchematics/" + fileName + ".def");

            var backInfo = jObj["FrontBezierSurf"].ToObject<List<BezierInfo>>();
            var sideInfo = jObj["SideBezierSurf"].ToObject<List<BezierInfo>>();
            var topInfo = jObj["TopBezierSurf"].ToObject<List<BezierInfo>>();

            var hullData = HullGeometryGenerator.GenerateShip
                (
                    backInfo,
                    sideInfo,
                    topInfo
                );

            var airship = new AirshipSerializationStruct();
            airship.DeckSections = hullData.DeckSectionContainer.ExtractSerializationStruct();
            airship.HullSections = hullData.HullSections.ExtractSerializationStruct();
            airship.ModelAttributes = hullData.ModelAttributes;

            var fs = new FileStream(Directory.GetCurrentDirectory() + "/Data/AirshipSchematics/" + fileName + ".protocol", FileMode.Create);
            Serializer.Serialize(fs, airship);
            fs.Close();

            sw.Stop();

            hullData.DeckSectionContainer.Dispose();
            hullData.HullSections.Dispose();

            DebugConsole.WriteLine("Airship converted from definition  to protocol in " + sw.ElapsedMilliseconds + " ms");
        }

        /// <summary>
        ///   Imports and airship's model from protocol format.
        /// </summary>
        /// <param name="fileName"> The filename of the ship's model, no extension. Base directory is /Data/AirshipSchematics/ </param>
        /// <returns> </returns>
        static AirshipSerializationStruct ImportFromProtocolFromFile(string fileName){
            var sw = new Stopwatch();
            sw.Start();
            var fs = new FileStream(Directory.GetCurrentDirectory() + "/Data/AirshipSchematics/" + fileName + ".protocol", FileMode.Open);
            var serializedStruct = Serializer.Deserialize<AirshipSerializationStruct>(fs);
            fs.Close();

            sw.Stop();

            DebugConsole.WriteLine("Airship protocol deserialized from protocol in " + sw.ElapsedMilliseconds + " ms");

            return serializedStruct;
        }

        static Vector3 CalculateCenter(VertexPositionNormalTexture[][] airshipVertexes){
            var ret = new Vector3(0, 0, 0);
            int numVerts = 0;
            foreach (var layer in airshipVertexes){
                numVerts += layer.Length;
                foreach (var vert in layer){
// ReSharper disable RedundantCast
                    ret += (Vector3) vert.Position;
// ReSharper restore RedundantCast
                }
            }
            ret /= numVerts;
            return ret;
        }

        #region Nested type: AirshipSerializationStruct

        [ProtoContract]
        struct AirshipSerializationStruct{
            [ProtoMember(2)] public DeckSectionContainer.Serialized DeckSections;
            [ProtoMember(1)] public HullSectionContainer.Serialized HullSections;
            [ProtoMember(3)] public ModelAttributes ModelAttributes;
        }

        #endregion

        #region Nested type: AirshipUidGenerator

        class AirshipUidGenerator{
            readonly JObject _jObject;
            int _nextUid;

            public AirshipUidGenerator(){
                _jObject = Resource.LoadJObject(Directory.GetCurrentDirectory() + "/Data/Ids.json");
                _nextUid = _jObject["MaxId"].ToObject<int>();
            }

            public int NextUid(){
                _jObject["MaxId"] = _nextUid + 1;
                var writer = new StreamWriter(Directory.GetCurrentDirectory() + "/Data/Ids.json");
                writer.Write(JsonConvert.SerializeObject(_jObject));
                writer.Close();
                _nextUid++;
                return _nextUid - 1;
            }
        }

        #endregion
    }
}