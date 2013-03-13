using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gondola.Logic;
using Gondola.UI.Components;

namespace Gondola.UI {
    /// <summary>
    ///   This class serves as a container for UI elements. Its purpose is to update said elements, and to provide collection-wide modification methods for use by external classes.
    /// </summary>
    internal class UIElementCollection : ILogicUpdates, IInputUpdates {
        readonly List<IUIElement> _elements;
        readonly UISortedList _layerSortedIElements;
        public bool DisableEntryHandlers;

        #region ctor

        public UIElementCollection(DepthLevel depth = DepthLevel.Medium) {
            _elements = new List<IUIElement>();
            _layerSortedIElements = new UISortedList();
            DisableEntryHandlers = false;
            Add(this);
        }

        #endregion

        #region ui element addition methods

        void AddElementToCollection(IUIElement elementToAdd) {
            _elements.Add(elementToAdd);
        }

        void AddElementToCollection(IUIInteractiveElement elementToAdd) {
            _layerSortedIElements.Add(elementToAdd.Depth, elementToAdd);
        }

        #endregion

        #region IInputUpdates Members

        public void UpdateInput(ref InputState state) {
            for (int i = 0; i < _layerSortedIElements.Count; i++) {
                _layerSortedIElements[i].UpdateInput(ref state);
            }
        }

        #endregion

        #region collection modification methods

        public void EnableComponents<TComponent>() {
            foreach (var element in _elements) {
                if (element.DoesComponentExist<TComponent>()) {
                    ((IUIComponent)(element.GetComponent<TComponent>())).Enabled = true; //()()()()()((())())
                }
            }
        }

        public void DisableComponents<TComponent>() {
            foreach (var element in _elements) {
                if (element.DoesComponentExist<TComponent>()) {
                    ((IUIComponent)(element.GetComponent<TComponent>())).Enabled = false;
                }
            }
        }

        public void FadeInAllElements() {
            foreach (var element in _elements) {
                if (element.DoesComponentExist<FadeComponent>()) {
                    element.GetComponent<FadeComponent>().ForceFadein();
                }
            }
        }

        public void FadeOutAllElements() {
            foreach (var element in _elements) {
                if (element.DoesComponentExist<FadeComponent>()) {
                    element.GetComponent<FadeComponent>().ForceFadeout();
                }
            }
        }

        public void AddFadeCallback(FadeStateChange deleg) {
            foreach (var element in _elements) {
                if (element.DoesComponentExist<FadeComponent>()) {
                    element.GetComponent<FadeComponent>().FadeStateChangeDispatcher += deleg;
                }
            }
        }

        public void AddDragCallback(OnComponentDrag deleg) {
            foreach (var element in _elements) {
                if (element.DoesComponentExist<DraggableComponent>()) {
                    element.GetComponent<DraggableComponent>().DragMovementDispatcher += deleg;
                }
            }
        }

        public void AddDragConstraintCallback(DraggableObjectClamp deleg) {
            foreach (var element in _elements) {
                if (element.DoesComponentExist<DraggableComponent>()) {
                    element.GetComponent<DraggableComponent>().DragMovementClamp += deleg;
                }
            }
        }

        #endregion

        #region static stuff

        static readonly List<UIElementCollection> _uiElementCollections;
        static UIElementCollection _curElementCollection;

        static UIElementCollection() {
            _uiElementCollections = new List<UIElementCollection>();
            _curElementCollection = null;
        }

        public static UIElementCollection Collection {
            get { return _curElementCollection; }
        }

        static void Add(UIElementCollection collection) {
            _uiElementCollections.Add(collection);
        }

        public static void Clear() {
            _uiElementCollections.Clear();
            _curElementCollection = null;
        }

        public static void AddElement(IUIElement elementToAdd) {
            if (_curElementCollection != null) {
                if (elementToAdd is IUIInteractiveElement) {
                    _curElementCollection.AddElementToCollection(elementToAdd as IUIInteractiveElement);
                }
                else {
                    _curElementCollection.AddElementToCollection(elementToAdd);
                }
            }
        }

        public static void BindCollection(UIElementCollection collection) {
            if (_curElementCollection != null) {
                throw new Exception("the previous bound collection needs to be cleared before a new one is set");
            }
            _curElementCollection = collection;
        }

        public static void UnbindCollection() {
            _curElementCollection = null;
        }

        #endregion

        #region ILogicUpdates Members

        public void UpdateLogic(double timeDelta) {
            foreach (IUIElement element in _elements) {
                element.UpdateLogic(timeDelta);
            }
            for (int i = 0; i < _layerSortedIElements.Count; i++) {
                _layerSortedIElements[i].UpdateLogic(timeDelta);
            }
        }

        #endregion
    }

    #region uisortedlist

    internal class UISortedList {
        readonly List<float> _depthList;
        readonly List<IUIInteractiveElement> _objList;

        public UISortedList() {
            _depthList = new List<float>();
            _objList = new List<IUIInteractiveElement>();
        }

        public int Count {
            get { return _depthList.Count; }
        }

        public IUIInteractiveElement this[int index] {
            get { return _objList[index]; }
        }

        public void Add(float depth, IUIInteractiveElement element) {
            _depthList.Add(depth);
            _objList.Add(element);

            for (int i = _depthList.Count - 1; i < 0; i--) {
                if (_depthList[i] < _depthList[i - 1]) {
                    _depthList.RemoveAt(i);
                    _objList.RemoveAt(i);
                    _depthList.Insert(i - 2, depth);
                    _objList.Insert(i - 2, element);
                }
                else {
                    break;
                }
            }
        }

        public void Clear() {
            _depthList.Clear();
            _objList.Clear();
        }

        public void RemoveAt(int index) {
            _depthList.RemoveAt(index);
            _objList.RemoveAt(index);
        }

        public void Remove(IUIElement element) {
            int i = 0;
            while (_objList[i] != element) {
                i++;
                if (i == _objList.Count) {
                    //return;
                }
            }
            RemoveAt(i);
        }
    }

    #endregion
}
