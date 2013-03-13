using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Gondola.Draw;
using Gondola.Util;
using Microsoft.Xna.Framework;

namespace Gondola.GameState.HullEditor {
    internal class BezierCurveCollection : IEnumerable<BezierCurve> {
        #region fields

        public readonly float PixelsPerMeter;
        readonly List<BezierCurve> _curveList;
        readonly RenderTarget _target;

        public double MaxX;
        public double MaxY;

        public BezierCurve MaxYCurve;
        public double MinX;
        public double MinY;
        double[] _lenList;
        double _totalArcLen;

        #endregion

        public BezierCurveCollection(RenderTarget target, string defaultConfig, FloatingRectangle areaToFill, PanelAlias panelType) {
            _target = target;
            var reader = XmlReader.Create(defaultConfig);
            reader.ReadToFollowing("NumControllers");
            int numControllers = int.Parse(reader.ReadString());
            reader.Close();
            var curveInitData = new List<CurveInitalizeData>(numControllers);
            _curveList = new List<BezierCurve>(numControllers);

            for (int i = 0; i < numControllers; i++) {
                curveInitData.Add(new CurveInitalizeData(defaultConfig, i));
            }

            //now get meters per pixel and scales
            float maxX = 0;
            float maxY = 0;
            foreach (var data in curveInitData) {
                if (data.HandlePosX > maxX) {
                    maxX = data.HandlePosX;
                }
                if (data.HandlePosY > maxY) {
                    maxY = data.HandlePosY;
                }
            }
            float scaleX = areaToFill.Width / maxX;
            float scaleY = areaToFill.Height / maxY;
            float scale = scaleX > scaleY ? scaleY : scaleX; //scale can also be considered pixels per meter
            PixelsPerMeter = scale;

            float offsetX = (areaToFill.Width - maxX * scale) / 2;
            float offsetY = (areaToFill.Height - maxY * scale) / 2;

            foreach (var data in curveInitData) {
                data.HandlePosX *= scale;
                data.HandlePosY *= scale;
                data.HandlePosX += offsetX + areaToFill.X;
                data.HandlePosY += offsetY + areaToFill.Y;
                data.Length1 *= scale;
                data.Length2 *= scale;
            }

            for (int i = 0; i < numControllers; i++) {
                _curveList.Add(new BezierCurve(_target, 0, 0, curveInitData[i]));
            }
            for (int i = 1; i < numControllers - 1; i++) {
                _curveList[i].SetPrevCurve(_curveList[i - 1]);
                _curveList[i].SetNextCurve(_curveList[i + 1]);
            }

            //set curve symmetry
            switch (panelType) {
                case PanelAlias.Side:
                    _curveList[0].Handle.SymmetricHandle = _curveList[_curveList.Count - 1].Handle;
                    _curveList[_curveList.Count - 1].Handle.SymmetricHandle = _curveList[0].Handle;

                    foreach (BezierCurve t in _curveList) {
                        t.Handle.SetReflectionType(
                            PanelAlias.Side,
                            CurveHandle.HandleMovementRestriction.Vertical
                            );
                    }
                    _curveList[0].Handle.SetReflectionType(
                        PanelAlias.Side,
                        CurveHandle.HandleMovementRestriction.Quadrant
                        );
                    _curveList[_curveList.Count - 1].Handle.SetReflectionType(
                        PanelAlias.Side,
                        CurveHandle.HandleMovementRestriction.Quadrant
                        );

                    break;
                case PanelAlias.Top:
                    for (int i = 0; i < _curveList.Count / 2; i++) {
                        _curveList[i].Handle.SymmetricHandle = _curveList[_curveList.Count - 1 - i].Handle;
                        _curveList[_curveList.Count - 1 - i].Handle.SymmetricHandle = _curveList[i].Handle;

                        _curveList[i].Handle.SetReflectionType(
                            PanelAlias.Top,
                            CurveHandle.HandleMovementRestriction.Vertical
                            );
                        _curveList[_curveList.Count - 1 - i].Handle.SetReflectionType(
                            PanelAlias.Top,
                            CurveHandle.HandleMovementRestriction.Vertical
                            );
                    }
                    _curveList[_curveList.Count / 2].Handle.SetReflectionType(
                        PanelAlias.Top,
                        CurveHandle.HandleMovementRestriction.NoRotationOnX,
                        true
                        );
                    break;

                case PanelAlias.Back:
                    for (int i = 0; i < _curveList.Count / 2; i++) {
                        _curveList[i].Handle.SymmetricHandle = _curveList[_curveList.Count - 1 - i].Handle;
                        _curveList[_curveList.Count - 1 - i].Handle.SymmetricHandle = _curveList[i].Handle;

                        _curveList[i].Handle.SetReflectionType(
                            PanelAlias.Back,
                            CurveHandle.HandleMovementRestriction.Vertical
                            );
                        _curveList[_curveList.Count - 1 - i].Handle.SetReflectionType(
                            PanelAlias.Back,
                            CurveHandle.HandleMovementRestriction.Vertical
                            );
                    }
                    _curveList[_curveList.Count / 2].Handle.SetReflectionType(
                        PanelAlias.Back,
                        CurveHandle.HandleMovementRestriction.NoRotationOnY,
                        true
                        );
                    _curveList[0].Handle.SetReflectionType(
                        PanelAlias.Back,
                        CurveHandle.HandleMovementRestriction.Quadrant
                        );
                    _curveList[_curveList.Count - 1].Handle.SetReflectionType(
                        PanelAlias.Back,
                        CurveHandle.HandleMovementRestriction.Quadrant
                        );

                    break;
            }
        }

        public void Update() {
            foreach (var curve in _curveList) {
                curve.Update();
            }
            MinX = _curveList[0].CenterHandlePos.X;
            MinY = _curveList[0].CenterHandlePos.Y;
            MaxX = 0;
            MaxY = 0;
            MaxYCurve = _curveList[0];
            foreach (var curve in _curveList) {
                if (curve.CenterHandlePos.X < MinX) {
                    MinX = curve.CenterHandlePos.X;
                }
                if (curve.CenterHandlePos.Y < MinY) {
                    MinY = curve.CenterHandlePos.Y;
                }
                if (curve.CenterHandlePos.X > MaxX) {
                    MaxX = curve.CenterHandlePos.X;
                }
                if (curve.CenterHandlePos.Y > MaxY) {
                    MaxY = curve.CenterHandlePos.Y;
                    MaxYCurve = curve;
                }
            }
        }

        public List<BezierInfo> GetControllerInfo(float scaleX = 1, float scaleY = 1) {
            var li = new List<BezierInfo>(_curveList.Count / 2 + 1);
            for (int i = 0; i < _curveList.Count; i++) {
                li.Add(
                    new BezierInfo(
                        pos: new Vector2((float)ToMetersX(_curveList[i].CenterHandlePos.X) * scaleX, (float)ToMetersY(_curveList[i].CenterHandlePos.Y) * scaleY),
                        prev: new Vector2((float)ToMetersX(_curveList[i].PrevHandlePos.X) * scaleX, (float)ToMetersY(_curveList[i].PrevHandlePos.Y) * scaleY),
                        next: new Vector2((float)ToMetersX(_curveList[i].NextHandlePos.X) * scaleX, (float)ToMetersY(_curveList[i].NextHandlePos.Y) * scaleY)
                        )
                    );
            }
            return li;
        }

        #region ienumerable members + accessors

        public BezierCurve this[int index] {
            get { return _curveList[index]; }
        }

        public int Count {
            get { return _curveList.Count; }
        }

        public IEnumerator<BezierCurve> GetEnumerator() {
            return ((IEnumerable<BezierCurve>)_curveList).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _curveList.GetEnumerator();
        }

        #endregion

        #region curve information retrieval methods

        /// <summary>
        ///   Returns the point on the curve associated with the parameter t
        /// </summary>
        /// <param name="t"> range from 0-1f </param>
        /// <param name="regenerateMethodCache"> </param>
        /// <returns> </returns>
        public Vector2 GetParameterizedPoint(double t, bool regenerateMethodCache = false) {
            if (regenerateMethodCache) {
                _lenList = new double[_curveList.Count - 1];
                _totalArcLen = 0;
                for (int i = 0; i < _lenList.Count(); i++) {
                    _lenList[i] = _curveList[i].GetNextArcLength() + _curveList[i + 1].GetPrevArcLength();
                    _totalArcLen += _lenList[i];
                }
            }

            double pointArcLen = _totalArcLen * t;
            double tempLen = pointArcLen;

            //figure out which curve is going to contain point t
            int segmentIndex;
            for (segmentIndex = 0; segmentIndex < _lenList.Count(); segmentIndex++) {
                tempLen -= _lenList[segmentIndex];
                if (tempLen < 0) {
                    tempLen += _lenList[segmentIndex];
                    tempLen /= _lenList[segmentIndex]; //this turns tempLen into a t(0-1)
                    break;
                }
            }

            if (segmentIndex == _curveList.Count - 1) { //clamp it 
                segmentIndex--;
                tempLen = 1;
            }
            Vector2 point = GetBezierValue(_curveList[segmentIndex], _curveList[segmentIndex + 1], tempLen);

            //now we need to normalize the point to meters
            point.X = (float)(point.X - MinX) / PixelsPerMeter;
            point.Y = (float)(point.Y - MinY) / PixelsPerMeter;

            return point;
        }

        public Vector2 GetBezierValue(BezierCurve prevCurve, BezierCurve nextCurve, double t) {
            Vector2 retVal;

            Bezier.GetBezierValue(
                out retVal,
                prevCurve.CenterHandlePos,
                prevCurve.NextHandlePos,
                nextCurve.PrevHandlePos,
                nextCurve.CenterHandlePos,
                (float)t
                );

            return retVal;
        }

        /// <summary>
        ///   converts a point from screen pixels to meters.
        /// </summary>
        /// <param name="point"> </param>
        /// <returns> </returns>
        public Vector2 ToMeters(Vector2 point) {
            point.X = (float)(point.X - MinX) / PixelsPerMeter;
            point.Y = (float)(point.Y - MinY) / PixelsPerMeter;
            return point;
        }

        public Vector2 ToPixels(Vector2 point) {
            point.X = (point.X * PixelsPerMeter) + (float)MinX;
            point.Y = (point.Y * PixelsPerMeter) + (float)MinY;

            return point;
        }

        public double ToMetersY(double y) {
            return (y - MinY) / PixelsPerMeter;
        }

        public double ToMetersX(double x) {
            return (x - MinX) / PixelsPerMeter;
        }

        #endregion

        /*public override InterruptState OnLocalLeftClick(MouseState state, MouseState? prevState = null){
            //this is broken right now
            /*if (Keyboard.GetState().IsKeyDown(Keys.LeftControl)){
                Vector2 pos;
                float t;
                for (int i = 1; i < CurveList.Count; i++){
                    if ((pos = CurveList[i].PrevContains(state, out t)) != Vector2.Zero){
                        CurveList.Insert(i, new BezierCurve(pos.X, pos.Y, ElementCollection));
                        CurveList[i].InsertBetweenCurves(CurveList[i - 1], CurveList[i + 1], t);
                        return InterruptState.InterruptEventDispatch;
                    }
                }
                for (int i = 0; i < CurveList.Count - 1; i++){


                    if ((pos = CurveList[i].NextContains(state, out t)) != Vector2.Zero){
                        i += 1;
                        CurveList.Insert(i, new BezierCurve(pos.X, pos.Y, ElementCollection ));
                        CurveList[i].InsertBetweenCurves(CurveList[i - 1], CurveList[i + 1], t);
                        return InterruptState.InterruptEventDispatch;
                    }
                }
            }*/
        /*
            return InterruptState.AllowOtherEvents;
        }
    */
    }

    #region nested struct

    internal class CurveInitalizeData {
        public float Angle;
        public float HandlePosX;
        public float HandlePosY;
        public float Length1;
        public float Length2;

        public CurveInitalizeData(string xmlFile, int i) {
            var reader = XmlReader.Create(xmlFile);
            reader.ReadToFollowing("Handle" + i);
            reader.ReadToFollowing("PosX");
            HandlePosX = float.Parse(reader.ReadString());
            reader.ReadToFollowing("PosY");
            HandlePosY = float.Parse(reader.ReadString());
            reader.ReadToFollowing("Angle");
            Angle = float.Parse(reader.ReadString());
            reader.ReadToFollowing("PrevLength");
            Length1 = float.Parse(reader.ReadString());
            reader.ReadToFollowing("NextLength");
            Length2 = float.Parse(reader.ReadString());
            reader.Close();
        }
    }

    #endregion
}
