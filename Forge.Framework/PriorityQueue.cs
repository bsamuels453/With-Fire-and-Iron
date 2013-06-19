#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Forge.Framework{
    /// <summary>
    /// Standard priorityqueue class that orders items based on a provided priority, from lowest to highest.
    /// The enumerator will respect the priority as well.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class PriorityQueue<T> : IEnumerable<T>{
        //used to store elements+priorities
        //used for the enumerator
        List<T> _elementObjs;
        List<Element> _elements;

        public PriorityQueue(int defaultSize){
            _elements = new List<Element>(defaultSize);
        }

        public PriorityQueue(){
            _elements = new List<Element>();
        }

        public int Count{
            get { return _elements.Count; }
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator(){
            return _elementObjs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator(){
            return GetEnumerator();
        }

        #endregion

        public void Add(T element, float priority){
            _elements.Add(new Element(element, priority));
            UpdateElementOrder();
        }

        public void Clear(){
            _elements.Clear();
            UpdateElementOrder();
        }

        public void RemoveAt(int index){
            _elements.RemoveAt(index);
            UpdateElementOrder();
        }

        public void Remove(T element){
            _elements = (
                from elem in _elements
                where !elem.Object.Equals(element)
                select elem).ToList();
            UpdateElementOrder();
        }

        void UpdateElementOrder(){
            _elements.Sort();
            _elementObjs = (
                from elem in _elements
                select elem.Object).ToList();
        }

        #region Nested type: Element

        struct Element : IComparable<Element>{
            public readonly T Object;
            readonly float _priortiy;

            public Element(T o, float priortiy)
                : this(){
                Object = o;
                _priortiy = priortiy;
            }

            #region IComparable<PriorityQueue<T>.Element> Members

            public int CompareTo(Element other){
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (other._priortiy == _priortiy){
                    return 0;
                }
                if (other._priortiy > _priortiy){
                    return -1;
                }
                return 1;
                // ReSharper restore CompareOfFloatsByEqualityOperator
            }

            #endregion
        }

        #endregion
    }
}