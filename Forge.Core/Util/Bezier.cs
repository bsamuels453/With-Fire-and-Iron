using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Forge.Core.Util {
    internal static class Bezier {
        #region generation methods

        static void Lerp(ref Vector2 dest, Vector2 a, Vector2 b, float t) {
            dest.X = a.X + (b.X - a.X) * t;
            dest.Y = a.Y + (b.Y - a.Y) * t;
        }

        static void DLerp(ref DVector2 dest, DVector2 a, DVector2 b, double t) {
            dest.X = a.X + (b.X - a.X) * t;
            dest.Y = a.Y + (b.Y - a.Y) * t;
        }

        /// <summary>
        ///   b and c are controllers. a and d are statics. AB is origin and CD is dest
        /// </summary>
        /// <param name="dest"> </param>
        /// <param name="ptA"> </param>
        /// <param name="ptB"> dest </param>
        /// <param name="ptC"> </param>
        /// <param name="ptD"> </param>
        /// <param name="t"> </param>
        public static void DGetBezierValue(out DVector2 dest, DVector2 ptA, DVector2 ptB, DVector2 ptC, DVector2 ptD, double t) {
            var ab = new DVector2();
            var bc = new DVector2();
            var cd = new DVector2();
            var abbc = new DVector2();
            var bccd = new DVector2();

            dest = new DVector2();

            DLerp(ref ab, ptA, ptB, t);
            DLerp(ref bc, ptB, ptC, t);
            DLerp(ref cd, ptC, ptD, t);
            DLerp(ref abbc, ab, bc, t);
            DLerp(ref bccd, bc, cd, t);
            DLerp(ref dest, abbc, bccd, t);
        }

        /// <summary>
        ///   partial double version for the inner pedantic
        /// </summary>
        /// <param name="dest"> </param>
        /// <param name="ptA"> </param>
        /// <param name="ptB"> dest </param>
        /// <param name="ptC"> </param>
        /// <param name="ptD"> </param>
        /// <param name="t"> </param>
        public static void GetBezierValue(out Vector2 dest, Vector2 ptA, Vector2 ptB, Vector2 ptC, Vector2 ptD, float t) {
            var ab = new Vector2();
            var bc = new Vector2();
            var cd = new Vector2();
            var abbc = new Vector2();
            var bccd = new Vector2();

            var ddest = new Vector2();
            var dptA = new Vector2(ptA.X, ptA.Y);
            var dptB = new Vector2(ptB.X, ptB.Y);
            var dptC = new Vector2(ptC.X, ptC.Y);
            var dptD = new Vector2(ptD.X, ptD.Y);

            Lerp(ref ab, dptA, dptB, t);
            Lerp(ref bc, dptB, dptC, t);
            Lerp(ref cd, dptC, dptD, t);
            Lerp(ref abbc, ab, bc, t);
            Lerp(ref bccd, bc, cd, t);
            Lerp(ref ddest, abbc, bccd, t);

            dest = new Vector2(ddest.X, ddest.Y);
        }
        #endregion

        internal struct DVector2 {
            public double X;
            public double Y;

            public DVector2(double x, double y) {
                X = x;
                Y = y;
            }

            public static explicit operator Vector2?(DVector2 v) {
                return new Vector2((float)v.X, (float)v.Y);
            }

            public static explicit operator DVector2?(Vector2 v) {
                return new DVector2(v.X, v.Y);
            }
        }
    }

    /// <summary>
    ///   generates bezier intersection point from an independent value do note that x is treated as an independent v variable, and y is dependent
    /// </summary>
    internal class BezierDependentGenerator {
        readonly List<BoundCache> _boundCache;
        readonly List<BezierInfo> _curveinfo;
        readonly float _largestX;
        readonly int _resolution; //represents the number of halfing operations required to get a result with an accuracy of less than one pixel in the worst case

        public BezierDependentGenerator(List<BezierInfo> curveinfo) {
            _curveinfo = curveinfo;
            float dist = 0;
            for (int i = 0; i < _curveinfo.Count - 1; i++) {
                dist += Vector2.Distance(_curveinfo[i].Pos, _curveinfo[i + 1].Pos);
            }

            float estimatedArcLen = dist * 2;

            int powResult = 1;
            int pow = 1;
            while (powResult < estimatedArcLen) {
                pow++;
                powResult = (int)Math.Pow(2, pow);
            }
            _resolution = pow * 2;

            _boundCache = new List<BoundCache>(curveinfo.Count - 1);
            for (int i = 0; i < curveinfo.Count - 1; i++) {
                _boundCache.Add(new BoundCache(0, 1, curveinfo[i].Pos, curveinfo[i + 1].Pos, 0));
            }
            _largestX = 0;
            foreach (var cache in _boundCache) {
                if (cache.LeftVal.X > _largestX) {
                    _largestX = cache.LeftVal.X;
                }
                if (cache.RightVal.X > _largestX) {
                    _largestX = cache.RightVal.X;
                }
            }
        }

        public Vector2 GetValueFromIndependent(float x) {
            //we run on the assumption that t=0 is on the left, and t=1 is on the right
            //we also run on the assumption that the given controllers form a curve that passes the vertical line test

            //using the assumption of vertical line test and sorted controllers left->right, we can figure out which segment to check for intersection

            if (x <= 0) {
                x = 0.000001f;
            }

            if (x >= _largestX) {
                x = _largestX;
            }
            if (_largestX == 0) {
                //return new Vector2(x,0);
            }


            int curvesToUse = -1;
            for (int i = 0; i < _boundCache.Count; i++) {
                if (_boundCache[i].Contains(x)) {
                    curvesToUse = i;
                }
            }
            if (curvesToUse == -1) {
                throw new Exception("Supplied X value is not contained within the bezier curve collection");
                //return Vector2.Zero;
            }
            //now we traverse the cache
            BoundCache curCache = _boundCache[curvesToUse];
            while (true) {
                if (curCache.LeftChild != null) {
                    if (curCache.LeftChild.Contains(x)) {
                        curCache = curCache.LeftChild;
                        continue;
                    }
                }
                if (curCache.RightChild != null) {
                    if (curCache.RightChild.Contains(x)) {
                        curCache = curCache.RightChild;
                        continue;
                    }
                }
                break;
            }

            //now continue until we get to dest
            int numRuns = curCache.Depth;
            while (numRuns++ <= _resolution) {
                Vector2 v = GenerateBoundValue((curCache.RightT + curCache.LeftT) / 2, curvesToUse);

                if (x > v.X) { //create a new rightchild
                    //leftbound.Val1 += (rightbound.Val1 - leftbound.Val1)/2;
                    float newLeftT = curCache.LeftT + (curCache.RightT - curCache.LeftT) / 2;

                    curCache.RightChild = new BoundCache(
                        newLeftT,
                        curCache.RightT,
                        GenerateBoundValue(newLeftT, curvesToUse),
                        curCache.RightVal,
                        curCache.Depth + 1
                        );
                    curCache = curCache.RightChild;
                }
                else { //create a new leftchild
                    //rightbound.Val1 -= (rightbound.Val1 - leftbound.Val1)/2;
                    float newRightT = curCache.RightT - (curCache.RightT - curCache.LeftT) / 2;

                    curCache.LeftChild = new BoundCache(
                        curCache.LeftT,
                        newRightT,
                        curCache.LeftVal,
                        GenerateBoundValue(newRightT, curvesToUse),
                        curCache.Depth + 1
                        );
                    curCache = curCache.LeftChild;
                }
            }

            return (curCache.LeftVal + curCache.RightVal) / 2;
        }

        /// <summary>
        ///   just a function to get the bezier value from a parameterized point
        /// </summary>
        Vector2 GenerateBoundValue(float t, int curvesToUse) {
            Vector2 v;
            Bezier.GetBezierValue(
                out v,
                _curveinfo[curvesToUse].Pos,
                _curveinfo[curvesToUse].NextControl,
                _curveinfo[curvesToUse + 1].PrevControl,
                _curveinfo[curvesToUse + 1].Pos,
                t
                );
            return v;
        }

        #region Nested type: BoundCache

        class BoundCache {
            public readonly int Depth;
            public readonly float LeftT;

            public readonly Vector2 LeftVal;
            public readonly float RightT;
            public readonly Vector2 RightVal;

            public BoundCache LeftChild;
            public BoundCache RightChild;

            public BoundCache(float leftT, float rightT, Vector2 leftVal, Vector2 rightVal, int depth) {
                Depth = depth;
                LeftT = leftT;
                RightT = rightT;
                LeftVal = leftVal;
                RightVal = rightVal;
            }

            public bool Contains(float x) {
                if (x >= LeftVal.X && x <= RightVal.X) {
                    return true;
                }
                return false;
            }
        }

        #endregion
    }

    /// <summary>
    ///   generates points from a set of bezier curves via brute force do note that x is treated as an independent v variable, and y is dependent
    /// </summary>
    internal class BruteBezierGenerator {
        readonly List<int> _curveStartIndexes; //contains a list of indexes that specify where a curve starts in the pointCache 
        readonly List<Vector2> _pointCache;
        List<float> _curveSegmentLengths;
        float _totalArcLen;

        public BruteBezierGenerator(List<BezierInfo> curveinfo) {
            _pointCache = new List<Vector2>();
            _curveStartIndexes = new List<int>(curveinfo.Count);
            _curveStartIndexes.Add(0);
            for (int curve = 0; curve < curveinfo.Count - 1; curve++) {
                float estArcLen = Vector2.Distance(curveinfo[curve].Pos, curveinfo[curve + 1].Pos);

                if (estArcLen == 0) { //fix for one of the special cases
                    _pointCache.Add(curveinfo[0].Pos);
                    return;
                }

                int numPoints = (int)estArcLen * 200; //eyeballing it to the max, this class's speed isnt important so feel free to increase this value
                if (numPoints == 0) {
                    numPoints = 1;
                }
                //right now it causes a ~half-centimeter resolution in worst case scenario
                _curveStartIndexes.Add(numPoints + _curveStartIndexes[_curveStartIndexes.Count - 1]);

                for (int point = 0; point <= numPoints; point++) {
                    float t = point / (float)numPoints;
                    Vector2 vec;

                    Bezier.GetBezierValue(
                        out vec,
                        curveinfo[curve].Pos,
                        curveinfo[curve].NextControl,
                        curveinfo[curve + 1].PrevControl,
                        curveinfo[curve + 1].Pos,
                        t
                        );

                    _pointCache.Add(vec);
                }
            }
        }

        /// <summary>
        ///   this method really doesn't belong in a class used to getting intersects, but it's still nifty
        /// </summary>
        /// <returns> </returns>
        public float GetLargestDependentValue() {
            float largest = 0;
            foreach (var vector2 in _pointCache) {
                if (vector2.Y > largest) {
                    largest = vector2.Y;
                }
            }
            return largest;
        }

        /// <summary>
        ///   this method takes an origin point which is assumed to be on the bezier curve, then travels down the curve until the distance between the origin point and current iterated point is equal to distance.
        /// </summary>
        public Vector2 GetPointFromPointDist(Vector2 originPoint, float distance) {
            var distList = new float[_pointCache.Count];

            //first we find where on our curve the origin point exists
            for (int i = 0; i < _pointCache.Count; i++) {
                distList[i] = Math.Abs(_pointCache[i].X - originPoint.X);
            }

            var xMatches = FindLowestValue(distList);
            if (xMatches.Count > 1) {
                throw new Exception("More than one independent variable detected for the given origin point X value. The curves this class was initalized with do not pass the vertical line test.");
            }

            int originIndex = xMatches[0];

            //now we build a list of distances between the origin point and other points on the curve
            var distToPointList = new float[_pointCache.Count - originIndex];
            for (int i = 0; i < _pointCache.Count - originIndex; i++) {
                distToPointList[i] = Vector2.Distance(originPoint, _pointCache[i + originIndex]);
            }

            var destPointIndex = FindClosestValue(distToPointList, distance);
            return _pointCache[originIndex + destPointIndex];
        }

        /// <summary>
        ///   there can be multiple independent values for each supplied dependent value, so it returns a list.
        /// </summary>
        public List<Vector2> GetValuesFromDependent(float dependent) {
            var distList = new float[_pointCache.Count];

            //y value is treated as the dependent value 
            for (int i = 0; i < _pointCache.Count; i++) {
                distList[i] = Math.Abs(_pointCache[i].Y - dependent);
            }

            var dependentIndexList = FindLowestValue(distList);

            var retList = new List<Vector2>(dependentIndexList.Count);

            foreach (var index in dependentIndexList) {
                retList.Add(_pointCache[index]);
            }
            if (retList.Count == 0) {
                retList.Add(_pointCache[_pointCache.Count - 1]);
                //retList.Add(new Vector2(0, 0));
            }

            return retList;
        }

        public Vector2 GetValueFromIndependent(float independent) {
            var distList = new float[_pointCache.Count];

            //y value is treated as the dependent value 
            for (int i = 0; i < _pointCache.Count; i++) {
                distList[i] = Math.Abs(_pointCache[i].X - independent);
            }

            var dependentIndexList = FindLowestValue(distList);

            var retList = new List<Vector2>(dependentIndexList.Count);

            foreach (var index in dependentIndexList) {
                retList.Add(_pointCache[index]);
                //retList.Add(new Vector2(0, 0));
            }


            return retList[0];
        }

        public Vector2 GetParameterizedPoint(double t) {
            double pointArcLen = GetArcLength() * t;
            double tempLen = pointArcLen;

            //figure out which curve is going to contain point t
            int segmentIndex;
            for (segmentIndex = 0; segmentIndex < _curveSegmentLengths.Count; segmentIndex++) {
                tempLen -= _curveSegmentLengths[segmentIndex];
                if (tempLen < 0) {
                    tempLen += _curveSegmentLengths[segmentIndex];
                    tempLen /= _curveSegmentLengths[segmentIndex]; //this turns tempLen into a t(0-1)
                    break;
                }
            }

            if (segmentIndex == _curveSegmentLengths.Count) { //clamp it 
                segmentIndex--;
                tempLen = 1;
            }

            int offset = _curveStartIndexes[segmentIndex + 1] - _curveStartIndexes[segmentIndex] + 1; //where did this +1 come from? nobody knows.
            offset = (int)(tempLen * offset);
            return _pointCache[_curveStartIndexes[segmentIndex] + offset];
        }

        public float GetArcLength() {
            if (_curveSegmentLengths != null) {
                return _totalArcLen;
            }
            _curveSegmentLengths = new List<float>();
            float total = 0;

            for (int i = 0; i < _curveStartIndexes.Count - 1; i++) {
                float len = 0;
                for (int p = _curveStartIndexes[i]; p < _curveStartIndexes[i + 1] - 1; p++) {
                    len += Vector2.Distance(_pointCache[p], _pointCache[p + 1]);
                }
                _curveSegmentLengths.Add(len);
                total += len;
            }
            _totalArcLen = total;
            return total;
        }

        List<int> FindLowestValue(float[] distList) {
            var retList = new List<int>();
            //intercepts are located based on whether the distlist is increasing or decreasing
            bool isDecreasing;

            //special case for first point
            if (distList.GetLength(0) < 2) {
                retList.Add(0);
                return retList;
            }
            if (distList[1] - distList[0] > 0) {
                //in this case, the endpoint matches up with the dependent 
                isDecreasing = false;
                retList.Add(0);
            }
            else {
                isDecreasing = true;
            }
            //end of special case

            for (int i = 1; i < _pointCache.Count - 1; i++) {
                if (isDecreasing) {
                    if (distList[i + 1] - distList[i] > 0) {
                        isDecreasing = false;
                        retList.Add(i);
                    }
                }
                else {
                    if (distList[i + 1] - distList[i] < 0) {
                        isDecreasing = true;
                    }
                }
            }

            //special case for last point
            if (distList[_pointCache.Count - 1] - distList[_pointCache.Count - 2] < 0) {
                retList.Add(distList.GetLength(0) - 1);
            }

            return retList;
        }

        /// <summary>
        /// </summary>
        /// <returns> index of the valuelist that is closest to the value </returns>
        int FindClosestValue(float[] valueList, float value) {
            for (int i = 0; i < valueList.GetLength(0) - 1; i++) {
                if (valueList[i] <= value && valueList[i + 1] > value) {
                    float d1 = Math.Abs(valueList[i] - value);
                    float d2 = Math.Abs(valueList[i + 1] - value);
                    if (d1 < d2) {
                        return i;
                    }
                    return i + 1;
                }
            }
            return valueList.GetLength(0) - 1;
        }
    }

    public struct BezierInfo {
        public Vector2 NextControl;
        public Vector2 Pos;
        public Vector2 PrevControl;

        public BezierInfo(Vector2 pos, Vector2 prev, Vector2 next) {
            Pos = pos;
            PrevControl = prev;
            NextControl = next;
        }

        public BezierInfo CreateScaledCopy(float scaleX, float scaleY) {
            var scaledController = new BezierInfo();
            scaledController.Pos.X = Pos.X * scaleX;
            scaledController.Pos.Y = Pos.Y * scaleY;
            scaledController.NextControl.X = NextControl.X * scaleX;
            scaledController.NextControl.Y = NextControl.Y * scaleY;
            scaledController.PrevControl.X = PrevControl.X * scaleX;
            scaledController.PrevControl.Y = PrevControl.Y * scaleY;
            return scaledController;
        }
    }
}
