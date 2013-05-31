#region

using System;
using System.Collections.Generic;
using System.Linq;
using Forge.Core.Util;
using Forge.Framework;
using Forge.Framework.Draw;
using Forge.Framework.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Vector2 = MonoGameUtility.Vector2;

#endregion

namespace Forge.Core.HullEditor{
    /// <summary>
    ///   this class is a fucking mess, here's to hoping it never has to be used again
    /// </summary>
    internal class BezierCurve{
        #region private fields

        const int _linesPerSide = 50;
        public readonly CurveHandle Handle;
        LineGenerator _lineTemplate;
        BezierCurve _nextCurve;
        List<Line> _nextLines;
        BezierCurve _prevCurve;
        List<Line> _prevLines;

        #endregion

        #region neighbor info and setters

        public BezierCurve PrevCurveReference{
            set{
                _prevCurve = value;
                Handle.PrevHandle = value.Handle;
                if (_prevLines == null){
                    _prevLines = new List<Line>(_linesPerSide);
                    for (int i = 0; i < _linesPerSide; i++){
                        _prevLines.Add(_lineTemplate.GenerateLine());
                    }
                }
            }
        }

        public BezierCurve NextCurveReference{
            set{
                _nextCurve = value;
                Handle.NextHandle = value.Handle;
                if (_nextLines == null){
                    _nextLines = new List<Line>(_linesPerSide);
                    for (int i = 0; i < _linesPerSide; i++){
                        _nextLines.Add(_lineTemplate.GenerateLine());
                    }
                }
            }
        }

        public void InsertBetweenCurves(BezierCurve prevCurve, BezierCurve nextCurve, float t){
            _prevCurve = prevCurve;
            _nextCurve = nextCurve;

            _prevCurve.NextCurveReference = this;
            _nextCurve.PrevCurveReference = this;


            Vector2 pt1;
            Vector2 pt2;
            Bezier.GetBezierValue(out pt1, _prevCurve.NextHandlePos, _prevCurve.NextHandlePos, PrevHandlePos, CenterHandlePos, t);
            Bezier.GetBezierValue(out pt2, _prevCurve.NextHandlePos, _prevCurve.NextHandlePos, PrevHandlePos, CenterHandlePos, t + 0.001f);
                //limits are for fags

            //get tangent and set to angle
            Vector2 pt3 = pt1 - pt2;
            float angle, magnitude;

            Common.GetAngleFromComponents(out angle, out magnitude, pt3.X, pt3.Y);

            Handle.Angle = angle;
            if (_prevLines == null){
                _prevLines = new List<Line>(_linesPerSide);

                for (int i = 0; i < _linesPerSide; i++){
                    _prevLines.Add(_lineTemplate.GenerateLine());
                }
            }
            else{
                for (int i = 0; i < _linesPerSide; i++){
                    _prevLines[i].Dispose();
                    _prevLines[i] = _lineTemplate.GenerateLine();
                }
            }

            if (_nextLines == null){
                _nextLines = new List<Line>(_linesPerSide);
                for (int i = 0; i < _linesPerSide; i++){
                    _nextLines.Add(_lineTemplate.GenerateLine());
                }
            }
            else{
                for (int i = 0; i < _linesPerSide; i++){
                    _nextLines[i].Dispose();
                    _nextLines[i] = _lineTemplate.GenerateLine();
                }
            }

            Update();
        }

        public void SetPrevCurve(BezierCurve val){
            _prevCurve = val;
            _prevCurve.NextCurveReference = this;
            Handle.PrevHandle = val.Handle;
            if (_prevLines == null){
                _prevLines = new List<Line>(_linesPerSide);

                for (int i = 0; i < _linesPerSide; i++){
                    _prevLines.Add(_lineTemplate.GenerateLine());
                }
            }
            else{
                for (int i = 0; i < _linesPerSide; i++){
                    _prevLines[i].Dispose();
                    _prevLines[i] = _lineTemplate.GenerateLine();
                }
            }
            Update();
        }

        public void SetNextCurve(BezierCurve val){
            _nextCurve = val;
            _nextCurve.PrevCurveReference = this;
            Handle.NextHandle = val.Handle;
            if (_nextLines == null){
                _nextLines = new List<Line>(_linesPerSide);
                for (int i = 0; i < _linesPerSide; i++){
                    _nextLines.Add(_lineTemplate.GenerateLine());
                }
            }
            else{
                for (int i = 0; i < _linesPerSide; i++){
                    _nextLines[i].Dispose();
                    _nextLines[i] = _lineTemplate.GenerateLine();
                }
            }
            Update();
        }

        #endregion

        #region curve information

        public Vector2 CenterHandlePos{
            get { return Handle.CentButtonPos; }
            set { Handle.CentButtonPos = value; }
        }

        public Vector2 PrevHandlePos{
            get { return Handle.PrevButtonPos; }
        }

        public Vector2 NextHandlePos{
            get { return Handle.NextButtonPos; }
        }

        public float PrevHandleLength{
            get { return Handle.PrevLength; }
        }

        public float NextHandleLength{
            get { return Handle.NextLength; }
        }

        public float GetNextArcLength(){
            float len = 0;
            if (_nextLines != null){
                len += _nextLines.Sum(line => line.Length);
            }
            return len;
        }

        public float GetPrevArcLength(){
            float len = 0;
            if (_prevLines != null){
                len += _prevLines.Sum(line => line.Length);
            }
            return len;
        }

        public Vector2 PrevContains(MouseState state, out float t){
            var mousePoint = new Vector2(state.X, state.Y);
            const int width = 5;

            if (_prevLines != null){
                for (int i = 0; i < _prevLines.Count; i++){
                    if (Vector2.Distance(_prevLines[i].DestPoint, mousePoint) < width){
                        t = ((float) i)/(_linesPerSide*2) + 0.5f;
                        return new Vector2(_prevLines[i].DestPoint.X, _prevLines[i].DestPoint.Y);
                    }
                }
                foreach (var line in _prevLines){
                }
            }
            //todo: fix these returns to not break on zero
            t = -1;
            return Vector2.Zero;
        }

        public Vector2 NextContains(MouseState state, out float t){
            var mousePoint = new Vector2(state.X, state.Y);
            const int width = 5;
            if (_nextLines != null){
                for (int i = 0; i < _nextLines.Count; i++){
                    if (Vector2.Distance(_nextLines[i].DestPoint, mousePoint) < width){
                        t = ((float) i)/(_linesPerSide*2);
                        return new Vector2(_nextLines[i].DestPoint.X, _nextLines[i].DestPoint.Y);
                    }
                }
            }
            t = -1;
            return Vector2.Zero;
        }

        #endregion

        #region ctor and disposal

        public BezierCurve(RenderTarget target, float offsetX, float offsetY, CurveInitalizeData initData){
            float initX = initData.HandlePosX + offsetX;
            float initY = initData.HandlePosY + offsetY;

            _nextLines = null;
            _nextCurve = null;
            _prevCurve = null;
            _prevLines = null;

            _lineTemplate = new LineGenerator();
            _lineTemplate.V1 = Vector2.Zero;
            _lineTemplate.V2 = Vector2.Zero;
            _lineTemplate.Color = Color.White;
            _lineTemplate.Depth = DepthLevel.Low;
            _lineTemplate.Target = target;

            Vector2 component1 = Common.GetComponentFromAngle(initData.Angle, initData.Length1);
            Vector2 component2 = Common.GetComponentFromAngle((float) (initData.Angle - Math.PI), initData.Length2); // minus math.pi to reverse direction

            #region stuff for generating ui elements

            var buttonTemplate = new ButtonGenerator("HullEditorHandle.json");

            var lineTemplate = new LineGenerator("HullEditorLine.json");
            lineTemplate.Target = target;
            Handle = new CurveHandle(buttonTemplate, lineTemplate, new Vector2(initX, initY), component1, component2);

            #endregion
        }

        public void Dispose(){
            Handle.Dispose();
            _lineTemplate = null;
            _nextCurve = null;
            _prevCurve = null;
            /*foreach (var line in _nextLines){
                line.Dispose();
            }
            foreach (var line in _prevLines) {
                line.Dispose();
            }
            _nextCurve.Dispose();
            _prevCurve.Dispose();*/
        }

        #endregion

        public void Update(){
            float t, dt;
            Vector2 firstPos, secondPos;
            if (_prevCurve != null){
                t = 0.5f;
                dt = 1/(float) (_linesPerSide*2);
                foreach (var line in _prevLines){
                    Bezier.GetBezierValue
                        (
                            out firstPos,
                            _prevCurve.CenterHandlePos,
                            _prevCurve.NextHandlePos,
                            PrevHandlePos,
                            CenterHandlePos,
                            t
                        );

                    Bezier.GetBezierValue
                        (
                            out secondPos,
                            _prevCurve.CenterHandlePos,
                            _prevCurve.NextHandlePos,
                            PrevHandlePos,
                            CenterHandlePos,
                            t + dt
                        );
                    line.OriginPoint = firstPos;
                    line.DestPoint = secondPos;
                    t += dt;
                }
            }
            if (_nextCurve != null){
                t = 0;
                dt = 1/(float) (_linesPerSide*2);
                foreach (var line in _nextLines){
                    Bezier.GetBezierValue
                        (
                            out firstPos,
                            CenterHandlePos,
                            NextHandlePos,
                            _nextCurve.PrevHandlePos,
                            _nextCurve.CenterHandlePos,
                            t
                        );

                    Bezier.GetBezierValue
                        (
                            out secondPos,
                            CenterHandlePos,
                            NextHandlePos,
                            _nextCurve.PrevHandlePos,
                            _nextCurve.CenterHandlePos,
                            t + dt
                        );
                    line.OriginPoint = firstPos;
                    line.DestPoint = secondPos;
                    t += dt;
                }
            }
        }

        /* //this might be useful someday
        private Vector2 GetPerpendicularBisector(Vector2 originPoint, Vector2 destPoint, Vector2 perpendicularPoint) {

            destPoint *= 10;

            var list = Common.Bresenham(originPoint, destPoint);
            var distances = new List<float>(list.Count);

            for (int i = 0; i < list.Count; i++) {
                distances.Add(Vector2.Distance(list[i], perpendicularPoint));
            }
            float lowestValue = 9999999999; //my condolences to players with screens larger than 9999999999x9999999999
            int lowestIndex = -1;

            for (int i = 0; i < distances.Count; i++) {
                if (distances[i] < lowestValue) {
                    lowestIndex = i;
                    lowestValue = distances[i];
                }
            }

            return list[lowestIndex];
        }
        */
    }
}