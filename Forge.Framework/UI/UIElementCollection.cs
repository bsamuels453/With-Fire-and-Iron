﻿#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
            _uiElementCollections.Add(this);
            _depthLevel = 0;
            _collectionDepth = 0;
            _boundingBox = new Rectangle(0, 0, Gbl.ScreenSize.X, Gbl.ScreenSize.Y);
        }

        public UIElementCollection(UIElementCollection parent, DepthLevel depth){
            _elements = new List<IUIElement>();
            _layerSortedIElements = new UISortedList();
            DisableEntryHandlers = false;
            _uiElementCollections.Add(this);
            _depthLevel = parent._depthLevel + 1;
            _collectionDepth = parent.GetRelDepth(depth);
            _boundingBox = new Rectangle(0, 0, Gbl.ScreenSize.X, Gbl.ScreenSize.Y);
        }

        public UIElementCollection(UIElementCollection parent, DepthLevel depth, Rectangle boundingBox){
            _elements = new List<IUIElement>();
            _layerSortedIElements = new UISortedList();
            DisableEntryHandlers = false;
            _uiElementCollections.Add(this);
            _depthLevel = parent._depthLevel + 1;
            _collectionDepth = parent.GetRelDepth(depth);
            _boundingBox = boundingBox;
        }

        #endregion

        public float GetRelDepth(DepthLevel depth){
            float magnitude = 10*(_depthLevel + 1);
            float d = (float) depth/magnitude;
            return _collectionDepth + d;
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
            _layerSortedIElements.Add(element.Depth, element);
        }

        #region IInputUpdates Members

        public void UpdateInput(ref InputState state){
            foreach (IUIElementBase t in _layerSortedIElements){
                t.UpdateInput(ref state);
            }
            if (HitTest(state.MousePos.X, state.MousePos.Y)){
                //xxx will this work correctly?
                state.AllowLeftButtonInterpretation = false;
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

        public float Alpha { get; set; }

        public float Depth{
            get { return _collectionDepth; }
            set { throw new NotImplementedException(); }
        }

        #region static stuff

        static readonly List<UIElementCollection> _uiElementCollections;
        static readonly List<UIElementCollection> _bindOrder;

        static UIElementCollection(){
            _uiElementCollections = new List<UIElementCollection>();
            _bindOrder = new List<UIElementCollection>();
        }

        public static UIElementCollection BoundCollection{
            get { return _bindOrder[0]; }
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
        readonly List<float> _depthList;
        readonly List<IUIElementBase> _objList;

        public UISortedList(){
            _depthList = new List<float>();
            _objList = new List<IUIElementBase>();
        }

        public int Count{
            get { return _depthList.Count; }
        }

        public IUIElementBase this[int index]{
            get { return _objList[index]; }
        }

        public void Add(float depth, IUIElementBase element){
            _depthList.Add(depth);
            _objList.Add(element);

            for (int i = _depthList.Count - 1; i < 0; i--){
                if (_depthList[i] < _depthList[i - 1]){
                    _depthList.RemoveAt(i);
                    _objList.RemoveAt(i);
                    _depthList.Insert(i - 2, depth);
                    _objList.Insert(i - 2, element);
                }
                else{
                    break;
                }
            }
        }

        public void Clear(){
            _depthList.Clear();
            _objList.Clear();
        }

        public void RemoveAt(int index){
            _depthList.RemoveAt(index);
            _objList.RemoveAt(index);
        }

        public void Remove(IUIElement element){
            int i = 0;
            while (_objList[i] != element){
                i++;
                if (i == _objList.Count){
                    throw new Exception("element doesnt exist");
                }
            }
            RemoveAt(i);
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