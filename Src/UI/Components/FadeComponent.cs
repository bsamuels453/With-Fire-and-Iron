#region

using System;
using Gondola.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;

#endregion

namespace Gondola.UI.Components{
    internal delegate void FadeStateChange(FadeComponent.FadeState state);

    /// <summary>
    ///   allows a UI element to be faded in and out. Required element to be IUIInteractiveComponent for certain settings.
    /// </summary>
    internal class FadeComponent : IUIComponent, IAcceptMouseEntryEvent, IAcceptMouseExitEvent {
        #region FadeState enum

        public enum FadeState {
            InvalidState,
            Visible,
            Faded
        }

        #endregion

        #region FadeTrigger enum

        public enum FadeTrigger {
            InvalidState,
            EntryExit,
            None
        }

        #endregion

        public const FadeTrigger DefaultTrigger = FadeTrigger.None;
        public const float DefaultFadeoutOpacity = 0.10f;
        public const float DefaultFadeDuration = 250;
        readonly FadeState _defaultState;
        readonly FadeTrigger _fadeTrigger;
        readonly float _fadeoutOpacity;
        float _fadeDuration;
        bool _Enabled;
        bool _isFadingOut;
        bool _isInTransition;
        IUIElement _owner;
        ButtonEventDispatcher _ownerEventDispatcher;
        long _prevUpdateTimeIndex;

        #region properties

        public float FadeDuration {
            set {
                if (_isInTransition) {
                    throw new Exception("cannot set duration while a fade is in progress");
                }
                _fadeDuration = value;
            }
        }


        public void ComponentCtor(IUIElement owner, ButtonEventDispatcher ownerEventDispatcher) {
            _owner = owner;
            _ownerEventDispatcher = ownerEventDispatcher;
            if (_defaultState == FadeState.Faded) {
                _owner.Opacity = _fadeoutOpacity;
            }
            switch (_fadeTrigger) {
                case FadeTrigger.EntryExit:


                    if (!(_owner is IUIInteractiveElement)) {
                        throw new Exception("Invalid fade trigger: Unable to set an interactive trigger to a non-interactive element.");
                    }

                    ownerEventDispatcher.OnMouseEntry.Add(this);
                    ownerEventDispatcher.OnMouseExit.Add(this);
                    //((IUIInteractiveElement) _owner).OnLeftButtonRelease.Add(ConfirmFadeoutProc);what the fuck was this for
                    break;

                case FadeTrigger.None:
                    break;
            }
        }

        public bool Enabled {
            get { return _Enabled; }
            set {
                _Enabled = value;
                var state = Mouse.GetState();
                if (_Enabled) { //reset the timer
                    _prevUpdateTimeIndex = DateTime.Now.Ticks;
                    //because the mouse may have left the bounding box while this component was disabled
                    if (!_owner.BoundingBox.Contains(state.X, state.Y)) {
                        ForceFadeout();
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// </summary>
        /// <param name="defaultState"> default state of the parent object, faded or visible </param>
        /// <param name="trigger"> </param>
        /// <param name="fadeoutOpacity"> opacity level to fade out to. range 0-1f </param>
        /// <param name="fadeDuration"> the time it takes for sprite to fade out in milliseconds </param>
        /// <param name="identifier"> </param>
        public FadeComponent(FadeState defaultState, FadeTrigger trigger = DefaultTrigger, float fadeoutOpacity = DefaultFadeoutOpacity, float fadeDuration = DefaultFadeDuration, string identifier = "") {
            _fadeoutOpacity = fadeoutOpacity;
            _fadeDuration = fadeDuration * 10000; //10k ticks in a millisecond
            _isInTransition = false;
            _isFadingOut = false;
            _prevUpdateTimeIndex = DateTime.Now.Ticks;
            _defaultState = defaultState;
            _fadeTrigger = trigger;
            _Enabled = true;
            Identifier = identifier;
        }

        #region IAcceptMouseEntryEvent Members

        public void OnMouseEntry(ref bool allowInterpretation, Point mousePos, Point prevMousePos) {
            UIElementCollection.Collection.DisableEntryHandlers = true;
            if (Enabled) {
                _isInTransition = true;
                _isFadingOut = false;
                if (FadeStateChangeDispatcher != null) {
                    FadeStateChangeDispatcher(FadeState.Visible);
                }
            }
        }

        #endregion

        #region IAcceptMouseExitEvent Members

        public void OnMouseExit(ref bool allowInterpretation, Point mousePos, Point prevMousePos) {
            UIElementCollection.Collection.DisableEntryHandlers = false;
            if (Enabled) {
                _isInTransition = true;
                _isFadingOut = true;
                if (FadeStateChangeDispatcher != null) {
                    FadeStateChangeDispatcher(FadeState.Faded);
                }
            }
        }

        #endregion

        #region IUIComponent Members

        public void Update() {
            if (Enabled) {
                if (_isInTransition) {
                    long timeSinceLastUpdate = DateTime.Now.Ticks - _prevUpdateTimeIndex;
                    float step = timeSinceLastUpdate / _fadeDuration;
                    if (_isFadingOut) {
                        _owner.Opacity -= step;
                        if (_owner.Opacity < _fadeoutOpacity) {
                            _owner.Opacity = _fadeoutOpacity;
                            _isInTransition = false;
                        }
                    }
                    else {
                        _owner.Opacity += step;
                        if (_owner.Opacity > 1) {
                            _owner.Opacity = 1;
                            _isInTransition = false;
                        }
                    }
                }
                _prevUpdateTimeIndex = DateTime.Now.Ticks;
            }
        }

        public string Identifier { get; private set; }

        #endregion

        #region modification methods

        public void ForceFadeout() {
            UIElementCollection.Collection.DisableEntryHandlers = false;
            if (Enabled) {
                _isInTransition = true;
                _isFadingOut = true;
                if (FadeStateChangeDispatcher != null) {
                    FadeStateChangeDispatcher(FadeState.Faded);
                }
            }
        }

        public void ForceFadein() {
            UIElementCollection.Collection.DisableEntryHandlers = true;
            if (Enabled) {
                _isInTransition = true;
                _isFadingOut = false;
                if (FadeStateChangeDispatcher != null) {
                    FadeStateChangeDispatcher(FadeState.Visible);
                }
            }
        }

        #endregion

        #region static methods

        /// <summary>
        ///   This method links two elements together so that each element can proc the other's fade as defined by the FadeTrigger.
        /// </summary>
        /// <param name="element1"> </param>
        /// <param name="element2"> </param>
        /// <param name="state"> </param>
        public static void LinkFadeComponentTriggers(IUIElement element1, IUIElement element2, FadeTrigger state) {
            switch (state) {
                case FadeTrigger.EntryExit:
                    //first we check both elements to make sure they are both interactive. This check is specific for triggers that are interactive
                    if (!(element1 is IUIInteractiveElement) || !(element2 is IUIInteractiveElement)) {
                        throw new Exception("Unable to link interactive element fade triggers; one of the elements is not interactive");
                    }
                    //cast to interactive
                    var e1 = (IUIInteractiveElement)element1;
                    var e2 = (IUIInteractiveElement)element2;

                    e1.GetComponent<FadeComponent>().AddRecievingFadeComponent(
                        e2.GetComponent<FadeComponent>()
                        );

                    e2.GetComponent<FadeComponent>().AddRecievingFadeComponent(
                        e1.GetComponent<FadeComponent>()
                        );

                    break;
            }
        }

        public void AddRecievingFadeComponent(FadeComponent component) {
            _ownerEventDispatcher.OnMouseEntry.Add(component);
            _ownerEventDispatcher.OnMouseExit.Add(component);
        }

        /// <summary>
        ///   This method allows an element's fade to trigger when another element undergoes a certain event as defined by the FadeTrigger.
        /// </summary>
        /// <param name="eventProcElement"> The element whose events will proc the recieving element's fade. </param>
        /// <param name="eventRecieveElement"> The recieving element. </param>
        /// <param name="state"> </param>
        public static void LinkOnewayFadeComponentTriggers(IUIElement eventProcElement, IUIElement eventRecieveElement, FadeTrigger state) {
            switch (state) {
                case FadeTrigger.EntryExit:
                    if (!(eventProcElement is IUIInteractiveElement)) {
                        throw new Exception("Unable to link interactive element fade triggers; the event proc element is not interactive.");
                    }
                    //cast to interactive
                    var e1 = (IUIInteractiveElement)eventProcElement;


                    e1.GetComponent<FadeComponent>().AddRecievingFadeComponent(
                        eventRecieveElement.GetComponent<FadeComponent>()
                        );

                    break;
            }
        }

        /// <summary>
        ///   This method allows an element's fade to trigger when another element undergoes a certain event as defined by the FadeTrigger.
        /// </summary>
        /// <param name="eventProcElements"> The list of elements whose events will proc the recieving element's fade. </param>
        /// <param name="eventRecieveElements"> The recieving elements. </param>
        /// <param name="state"> </param>
        public static void LinkOnewayFadeComponentTriggers(IUIElement[] eventProcElements, IUIElement[] eventRecieveElements, FadeTrigger state) {
            switch (state) {
                case FadeTrigger.EntryExit:
                    foreach (var pElement in eventProcElements) {
                        if (!(pElement is IUIInteractiveElement)) {
                            throw new Exception("Unable to link interactive element fade triggers; the event proc element is not interactive.");
                        }
                        foreach (var eElement in eventRecieveElements) {
                            var procElement = (IUIInteractiveElement)pElement;

                            procElement.GetComponent<FadeComponent>().AddRecievingFadeComponent(
                                eElement.GetComponent<FadeComponent>()
                                );
                        }
                    }

                    break;
            }
        }

        #endregion

        public event FadeStateChange FadeStateChangeDispatcher;

        public static FadeComponent ConstructFromObject(JObject obj, string identifier) {
            var ctorData = obj.ToObject<FadeComponentCtorData>();

            if (ctorData.DefaultState == FadeState.InvalidState) //trivial: why no default state for this?
                throw new Exception("not enough data to create a FadeComponent from template");

            var defaultState = ctorData.DefaultState;
            FadeTrigger fadeTrigger;
            float fadeOpacity;
            float fadeDuration;

            if (ctorData.FadeTrigger != FadeTrigger.InvalidState)
                fadeTrigger = ctorData.FadeTrigger;
            else
                fadeTrigger = DefaultTrigger;


            if (ctorData.FadedOpacity != null)
                fadeOpacity = (float)ctorData.FadedOpacity;
            else
                fadeOpacity = DefaultFadeoutOpacity;


            if (ctorData.FadeDuration != null)
                fadeDuration = (float)ctorData.FadeDuration;
            else
                fadeDuration = DefaultFadeDuration;

            return new FadeComponent(defaultState, fadeTrigger, fadeOpacity, fadeDuration, identifier);
        }

        #region Nested type: FadeComponentCtorData

        struct FadeComponentCtorData {
#pragma warning disable 649
            public FadeState DefaultState;
            public FadeTrigger FadeTrigger;
            public float? FadedOpacity;
            public float? FadeDuration;
#pragma warning restore 649
        }

        #endregion
    }
}