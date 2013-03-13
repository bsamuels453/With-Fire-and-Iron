#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Forge.Framework.UI.Components;
using Microsoft.Xna.Framework;

#endregion

namespace Forge.Framework.UI{
    /// <summary>
    ///   This class serves as a container for UI elements. Its purpose is to update said elements, and to provide collection-wide modification methods for use by external classes.
    /// </summary>
    public class UIElementCollection : IUIElementBase{
        readonly List<IUIElement> _elements;
        readonly UISortedList _layerSortedIElements;
        public bool DisableEntryHandlers;
        readonly int _depthLevel;
        readonly float _collectionDepth;
        readonly Rectangle _boundingBox;

        #region ctor

        public UIElementCollection(){
            _elements = new List<IUIElement>();
            _layerSortedIElements = new UISortedList();
            DisableEntryHandlers = false;
            //_uiElementCollections.Add(this);
            _depthLevel = 0;
            _collectionDepth = 1;
            _boundingBox = new Rectangle(0, 0, Gbl.ScreenSize.X, Gbl.ScreenSize.Y);
            _parentCollection = this;
        }

        public UIElementCollection(UIElementCollection parent, DepthLevel depth){
            _elements = new List<IUIElement>();
            _layerSortedIElements = new UISortedList();
            DisableEntryHandlers = false;
            //_uiElementCollections.Add(this);
            _depthLevel = parent._depthLevel + 1;
            _collectionDepth = parent.GetRelDepth(depth);
            _boundingBox = new Rectangle(0, 0, Gbl.ScreenSize.X, Gbl.ScreenSize.Y);
            Debug.Assert(_depthLevel < 10);
            parent.AddElement(this);
        }

        public UIElementCollection(UIElementCollection parent, DepthLevel depth, Rectangle boundingBox){
            _elements = new List<IUIElement>();
            _layerSortedIElements = new UISortedList();
            DisableEntryHandlers = false;
            //_uiElementCollections.Add(this);
            _depthLevel = parent._depthLevel + 1;
            _collectionDepth = parent.GetRelDepth(depth);
            _boundingBox = boundingBox;
            Debug.Assert(_depthLevel < 10);
            parent.AddElement(this);
        }

        #endregion

        public float GetRelDepth(DepthLevel depth){
            float magnitude = (float)Math.Pow(10,(_depthLevel + 1));
            float d = ((float)depth)/magnitude;
            var ret = _collectionDepth - d;
            return ret;
        }

        public bool HitTest(int x, int y){
            if (_boundingBox.Contains(x, y)){
                foreach (var elem in _layerSortedIElements){
                    if (elem.HitTest(x, y)){
                        return true;
                    }
                }
            }
            return false;
        }

        public void AddNonInteractivElem(IUIElement element){
            _elements.Add(element);
        }

        public void AddElement(IUIElementBase element){
            Debug.Assert(_boundingBox.Contains(
                new Rectangle(
                    (int)element.X,
                    (int)element.Y,
                    (int)element.Width,
                    (int)element.Height
                    )
                    )
                    );
            _layerSortedIElements.Add(element.Depth, element);
            
        }

        #region IInputUpdates Members

        public void UpdateInput(ref InputState state){
            foreach (IUIElementBase t in _layerSortedIElements){
                t.UpdateInput(ref state);
            }
            if (HitTest(state.MousePos.X, state.MousePos.Y)){
                if (state.AllowLeftButtonInterpretation){
                    int f = 4;
                }
                //xxx will this work correctly?
                state.AllowLeftButtonInterpretation = false;
                state.AllowMouseMovementInterpretation = false;
            }
        }

        #endregion

        #region collection modification methods

        //unused
        public void EnableComponents<TComponent>(){
            foreach (var element in _elements){
                if (element.DoesComponentExist<TComponent>()){
                    ((IUIComponent) (element.GetComponent<TComponent>())).Enabled = true; //()()()()()((())())
                }
            }
        }

        //unused
        public void DisableComponents<TComponent>(){
            foreach (var element in _elements){
                if (element.DoesComponentExist<TComponent>()){
                    ((IUIComponent) (element.GetComponent<TComponent>())).Enabled = false;
                }
            }
        }

        //unused
        public void FadeInAllElements(){
            foreach (var element in _elements){
                if (element.DoesComponentExist<FadeComponent>()){
                    element.GetComponent<FadeComponent>().ForceFadein();
                }
            }
        }

        //unused
        public void FadeOutAllElements(){
            foreach (var element in _elements){
                if (element.DoesComponentExist<FadeComponent>()){
                    element.GetComponent<FadeComponent>().ForceFadeout();
                }
            }
        }

        //unused
        public void AddFadeCallback(FadeStateChange deleg){
            foreach (var element in _elements){
                if (element.DoesComponentExist<FadeComponent>()){
                    element.GetComponent<FadeComponent>().FadeStateChangeDispatcher += deleg;
                }
            }
        }

        //unused
        public void AddDragCallback(OnComponentDrag deleg){
            foreach (var element in _elements){
                if (element.DoesComponentExist<DraggableComponent>()){
                    element.GetComponent<DraggableComponent>().DragMovementDispatcher += deleg;
                }
            }
        }

        //used
        public void AddDragConstraintCallback(DraggableObjectClamp deleg){
            foreach (var element in _elements){
                if (element.DoesComponentExist<DraggableComponent>()){
                    element.GetComponent<DraggableComponent>().DragMovementClamp += deleg;
                }
            }
        }

        #endregion

        public void Bind(){
            Debug.Assert(!_bindOrder.Contains(this));
            _bindOrder.Insert(0, this);
        }

        public void Unbind(){
            Debug.Assert(_bindOrder[0] == this);
            _bindOrder.RemoveAt(0);
        }

        public float X{
            get { return _boundingBox.X; }
            set { throw new NotImplementedException(); }
        }

        public float Y{
            get { return _boundingBox.Y; }
            set { throw new NotImplementedException(); }
        }

        public float Width{
            get { return _boundingBox.Width; }
            set { throw new NotImplementedException(); }
        }

        public float Height{
            get { return _boundingBox.Height; }
            set { throw new NotImplementedException(); }
        }

        public float Alpha { 
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public float Depth{
            get { return _collectionDepth; }
            set { throw new NotImplementedException(); }
        }

        public List<IUIElementBase> GetElementStack(int x, int y) {
            var ret = new List<IUIElementBase>();
            foreach (var element in _layerSortedIElements){
                ret.AddRange(element.GetElementStack(x, y));
            }
            return ret;
        }

        #region static stuff

        //static readonly List<UIElementCollection> _uiElementCollections;
        static UIElementCollection _parentCollection;
        static readonly List<UIElementCollection> _bindOrder;

        static UIElementCollection(){
            //_uiElementCollections = new List<UIElementCollection>();
            _bindOrder = new List<UIElementCollection>();
        }

        public static UIElementCollection BoundCollection{
            get { return _bindOrder[0]; }
        }

        public static List<IUIElementBase> GetGlobalElementStack(int x, int y){
            return _parentCollection.GetElementStack(x, y);
        }

        #endregion

        #region ILogicUpdates Members

        public void UpdateLogic(double timeDelta){
            foreach (IUIElement element in _elements){
                element.UpdateLogic(timeDelta);
            }
            foreach (IUIElementBase t in _layerSortedIElements){
                t.UpdateLogic(timeDelta);
            }
        }

        #endregion
    }

    #region uisortedlist

    internal class UISortedList : IEnumerable<IUIElementBase>{
        List<IUIElementBase> _objList;

        public UISortedList(){
            _objList = new List<IUIElementBase>();
        }

        public int Count{
            get { return _objList.Count; }
        }

        public IUIElementBase this[int index]{
            get { return _objList[index]; }
        }

        public void Add(float depth, IUIElementBase element){
            _objList.Add(element);
            _objList = _objList.OrderBy(o => o.Depth).ToList();
        }

        public void Clear(){
            _objList.Clear();
        }

        public void RemoveAt(int index){
            _objList.RemoveAt(index);
        }

        public void Remove(IUIElement element){
            _objList.Remove(element);
        }

        public IEnumerator<IUIElementBase> GetEnumerator(){
            return _objList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator(){
            return GetEnumerator();
        }
    }

    #endregion
}