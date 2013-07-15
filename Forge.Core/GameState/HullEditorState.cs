#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Forge.Core.HullEditor;
using Forge.Framework;
using Forge.Framework.Draw;
using Forge.Framework.Resources;
using Forge.Framework.UI;
using Microsoft.Xna.Framework.Input;
using MonoGameUtility;

#endregion

namespace Forge.Core.GameState{
    public class HullEditorState : IGameState{
        readonly BackEditorPanel _backpanel;
        readonly UIElementCollection _elementCollection;

        readonly PreviewRenderer _previewRenderer;
        readonly SideEditorPanel _sidepanel;
        readonly TopEditorPanel _toppanel;
        readonly RenderTarget test;

        public HullEditorState(){
            test = new RenderTarget();
            test.Bind();
            _elementCollection = new UIElementCollection(GamestateManager.MouseManager);
            _elementCollection.Bind();
            test.Unbind();

            _sidepanel = new SideEditorPanel(0, 0, Resource.ScreenSize.GetScreenValueX(0.5f), Resource.ScreenSize.GetScreenValueY(0.5f), "Data/side.xml");
            _toppanel = new TopEditorPanel
                (0, Resource.ScreenSize.GetScreenValueY(0.5f), Resource.ScreenSize.GetScreenValueX(0.5f), Resource.ScreenSize.GetScreenValueY(0.5f),
                    "Data/top.xml");
            _backpanel = new BackEditorPanel
                (Resource.ScreenSize.GetScreenValueX(0.5f), 0, Resource.ScreenSize.GetScreenValueX(0.25f), Resource.ScreenSize.GetScreenValueY(0.5f),
                    "Data/back.xml");

            _sidepanel.BackPanel = _backpanel;
            _sidepanel.TopPanel = _toppanel;

            _toppanel.BackPanel = _backpanel;
            _toppanel.SidePanel = _sidepanel;

            _backpanel.TopPanel = _toppanel;
            _backpanel.SidePanel = _sidepanel;

            _previewRenderer = new PreviewRenderer(_sidepanel.Curves, _toppanel.Curves, _backpanel.Curves);

            //_elementCollection.Unbind();
        }

        #region IGameState Members

        public void Dispose(){
            _previewRenderer.Dispose();
            _sidepanel.Dispose();
            _backpanel.Dispose();
            _toppanel.Dispose();
            _elementCollection.Dispose();
        }

        public void Update(double timeDelta){
            //force end early
            var sideInfo = _sidepanel.Curves.GetControllerInfo();
            var backInfo = _backpanel.Curves.GetControllerInfo();
            var topInfo = _toppanel.Curves.GetControllerInfo();

            GamestateManager.ClearState();
            GamestateManager.AddGameState(new ObjectEditorState(backInfo, sideInfo, topInfo));
            return;
            throw new Exception();

            /*
            _elementCollection.Bind();
            _sidepanel.Update();
            _toppanel.Update();
            _backpanel.Update();
            _previewRenderer.Update(ref state);
            UIElementCollection.BoundCollection.UpdateInput(ref state);
            UIElementCollection.BoundCollection.UpdateLogic(timeDelta);
            _elementCollection.Unbind();
            HandleEditorKeyboardInput(ref state);
             */
        }

        public void Draw(){
            _sidepanel.Draw();
            _toppanel.Draw();
            _backpanel.Draw();
            _previewRenderer.Draw();
        }

        #endregion

        void HandleEditorKeyboardInput(ref InputState state){
            if (state.KeyboardState.IsKeyDown(Keys.LeftControl) && state.KeyboardState.IsKeyDown(Keys.S)){
                SaveCurves("save/");
            }

            if (state.KeyboardState.IsKeyDown(Keys.LeftControl) && state.KeyboardState.IsKeyDown(Keys.N)){
                var sideInfo = _sidepanel.Curves.GetControllerInfo();
                var backInfo = _backpanel.Curves.GetControllerInfo();
                var topInfo = _toppanel.Curves.GetControllerInfo();

                GamestateManager.ClearState();
                GamestateManager.AddGameState(new ObjectEditorState(backInfo, sideInfo, topInfo));
            }
        }

        public void SaveCurves(string directory){
            throw new Exception();
            //dear mother of god why does this have to be hardcoded
            //top set
            /*
            var bowPointTop = _toppanel.Curves.ToMeters(_toppanel.Curves[1].CenterHandlePos);
            float bowNLengthTop = _toppanel.Curves[1].NextHandleLength/_toppanel.Curves.PixelsPerMeter;
            float bowAngleTop = -MathHelper.Pi/2;

            var starboardPointTop = _toppanel.Curves.ToMeters(_toppanel.Curves[0].CenterHandlePos);
            float starboardNLengthTop = _toppanel.Curves[0].NextHandleLength/_toppanel.Curves.PixelsPerMeter;
            float starboardAngleTop = _toppanel.Curves[0].Handle.Angle;

            //side set
            var bowPointSide = _sidepanel.Curves.ToMeters(_sidepanel.Curves[0].CenterHandlePos);
            float bowNLengthSide = (_sidepanel.Curves[0].NextHandleLength)/_sidepanel.Curves.PixelsPerMeter;
            float bowAngleSide = (_sidepanel.Curves[0].Handle.Angle);

            var sternPointSide = _sidepanel.Curves.ToMeters(_sidepanel.Curves[2].CenterHandlePos);
            float sternPLengthSide = _sidepanel.Curves[2].PrevHandleLength/_sidepanel.Curves.PixelsPerMeter;
            float sternAngleSide = _sidepanel.Curves[2].Handle.Angle;

            var hullPointSide = _sidepanel.Curves.ToMeters(_sidepanel.Curves[1].CenterHandlePos);
            float hullPLengthSide = _sidepanel.Curves[1].PrevHandleLength/_sidepanel.Curves.PixelsPerMeter;
            float hullNLengthSide = _sidepanel.Curves[1].NextHandleLength/_sidepanel.Curves.PixelsPerMeter;
            float hullAngleSide = _sidepanel.Curves[1].Handle.Angle;

            //back set
            var starboardPointBack = _backpanel.Curves.ToMeters(_backpanel.Curves[2].CenterHandlePos);
            float starboardPLengthBack = _backpanel.Curves[2].PrevHandleLength/_backpanel.Curves.PixelsPerMeter;
            float starboardAngleBack = _backpanel.Curves[2].Handle.Angle;

            var hullPointBack = _backpanel.Curves.ToMeters(_backpanel.Curves[1].CenterHandlePos);
            float hullPLengthBack = _backpanel.Curves[1].PrevHandleLength/_backpanel.Curves.PixelsPerMeter;
            float hullNLengthBack = _backpanel.Curves[1].NextHandleLength/_backpanel.Curves.PixelsPerMeter;
            float hullAngleBack = -MathHelper.Pi;

            //now force validate the points
            bowPointSide.X = bowPointTop.X;
            bowPointSide.Y = 0;
            sternPointSide.X = starboardPointTop.X;
            starboardPointTop.Y = 0;
            sternPointSide.Y = 0;

            starboardPointBack.X = bowPointTop.Y*2;
            starboardPointBack.Y = 0;
            hullPointBack.Y = hullPointSide.Y;
            hullPointBack.X = bowPointTop.Y;
            //now save everything

            //may the programming gods have mercy on my soul TODO: make all this hull saving crap softcoded
            var sideData = new List<CurveData>(3);
            var topData = new List<CurveData>(3);
            var backData = new List<CurveData>(3);

            sideData.Add(new CurveData());
            sideData[0].CenterHandlePos = bowPointSide;
            sideData[0].Angle = bowAngleSide;
            sideData[0].NextHandleLength = bowNLengthSide;
            sideData[0].PrevHandleLength = 5;

            sideData.Add(new CurveData());
            sideData[1].CenterHandlePos = hullPointSide;
            sideData[1].Angle = hullAngleSide;
            sideData[1].NextHandleLength = hullNLengthSide;
            sideData[1].PrevHandleLength = hullPLengthSide;

            sideData.Add(new CurveData());
            sideData[2].CenterHandlePos = sternPointSide;
            sideData[2].Angle = sternAngleSide;
            sideData[2].NextHandleLength = 5;
            sideData[2].PrevHandleLength = sternPLengthSide;

            backData.Add(new CurveData());
            backData[0].CenterHandlePos = new Vector2(0, 0);
            backData[0].Angle = (2*MathHelper.Pi - starboardAngleBack);
            backData[0].NextHandleLength = starboardPLengthBack;
            backData[0].PrevHandleLength = 5;

            backData.Add(new CurveData());
            backData[1].CenterHandlePos = hullPointBack;
            backData[1].Angle = hullAngleBack;
            backData[1].NextHandleLength = hullNLengthBack;
            backData[1].PrevHandleLength = hullPLengthBack;

            backData.Add(new CurveData());
            backData[2].CenterHandlePos = starboardPointBack;
            backData[2].Angle = starboardAngleBack;
            backData[2].NextHandleLength = 5;
            backData[2].PrevHandleLength = starboardPLengthBack;

            topData.Add(new CurveData());
            topData[0].CenterHandlePos = starboardPointTop;
            topData[0].Angle = starboardAngleTop;
            topData[0].NextHandleLength = starboardNLengthTop;
            topData[0].PrevHandleLength = 5;

            topData.Add(new CurveData());
            topData[1].CenterHandlePos = bowPointTop;
            topData[1].Angle = bowAngleTop;
            topData[1].NextHandleLength = bowNLengthTop;
            topData[1].PrevHandleLength = bowNLengthTop;

            topData.Add(new CurveData());
            topData[2].CenterHandlePos = new Vector2(starboardPointTop.X, bowPointTop.Y*2);
            topData[2].Angle = (MathHelper.Pi - starboardAngleTop);
            topData[2].NextHandleLength = 5;
            topData[2].PrevHandleLength = starboardNLengthTop;

            SaveCurve(directory + "back.xml", backData);
            SaveCurve(directory + "top.xml", topData);
            SaveCurve(directory + "side.xml", sideData);
             */
        }

        void SaveCurve(string filename, List<CurveData> curveData){
            var settings = new XmlWriterSettings();
            settings.Indent = true;

            Stream outputStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            var writer = XmlWriter.Create(outputStream, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("Data");
            writer.WriteElementString("NumControllers", null, curveData.Count.ToString());

            for (int i = 0; i < curveData.Count; i++){
                writer.WriteStartElement("Handle" + i, null);
                writer.WriteElementString("PosX", null, curveData[i].CenterHandlePos.X.ToString());
                writer.WriteElementString("PosY", null, curveData[i].CenterHandlePos.Y.ToString());
                writer.WriteElementString("Angle", null, curveData[i].Angle.ToString());
                writer.WriteElementString("PrevLength", null, curveData[i].PrevHandleLength.ToString());
                writer.WriteElementString("NextLength", null, curveData[i].NextHandleLength.ToString());
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.Close();
            outputStream.Close();
        }

        #region Nested type: CurveData

        class CurveData{
            public Vector2 CenterHandlePos { get; set; }
            public float Angle { get; set; }
            public float PrevHandleLength { get; set; }
            public float NextHandleLength { get; set; }
        }

        #endregion
    }
}