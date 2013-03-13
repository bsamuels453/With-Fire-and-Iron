#region

using System;
using System.Collections.Generic;
using Gondola.Draw;
using Gondola.Logic;
using Gondola.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#endregion

namespace Gondola.UI{
    internal class Button : IUIInteractiveElement{
        public const int DefaultTexRepeat = 1;
        public const int DefaultIdentifier = 1;

        readonly FloatingRectangle _boundingBox; //bounding box that represents the bounds of the button
        readonly ButtonEventDispatcher _iEventDispatcher;
        readonly int _identifier; //non-function based identifier that can be used to differentiate buttons
        readonly Sprite2D _sprite; //the button's sprite
        Vector2 _centPosition; //represents the approximate center of the button
        bool _enabled;
        string _texture;

        public Vector2 CentPosition{
            get { return _centPosition; }
            set{
                _centPosition = value;
                _boundingBox.X = _centPosition.X - _boundingBox.Width/2;
                _boundingBox.Y = _centPosition.Y - _boundingBox.Height/2;
            }
        }

        public IUIComponent[] Components { get; set; }

        public String Texture{
            get { return _texture; }
            set{
                _texture = value;
                _sprite.SetTextureFromString(value);
            }
        }

        public int Identifier{
            get { return _identifier; }
        }

        public float X{
            get { return _boundingBox.X; }
            set{
                _boundingBox.X = value;
                _centPosition.X = _boundingBox.X + _boundingBox.Width/2;
                _sprite.X = (int) value;
            }
        }

        public float Y{
            get { return _boundingBox.Y; }
            set{
                _boundingBox.Y = value;
                _centPosition.Y = _boundingBox.Y + _boundingBox.Height/2;
                _sprite.Y = (int) value;
            }
        }

        public float Width{
            get { return _boundingBox.Width; }
            set{
                throw new NotImplementedException();
                //requires center point fixing
                _boundingBox.Width = value;
                _sprite.Width = (int) value;
            }
        }

        public float Height{
            get { return _boundingBox.Height; }
            set{
                throw new NotImplementedException();
                //requires center point fixing
                _boundingBox.Height = value;
                _sprite.Height = (int) value;
            }
        }

        public event OnBasicMouseEvent OnLeftClickDispatcher;
        public event OnBasicMouseEvent OnLeftPressDispatcher;
        public event OnBasicMouseEvent OnLeftReleaseDispatcher;

        public bool Enabled{
            get { return _enabled; }
            set{
                _enabled = value;
                _sprite.Enabled = value;
                foreach (var component in Components){
                    component.Enabled = value;
                }
            }
        }

        public bool ContainsMouse { get; private set; }

        public float Alpha{
            get { return _sprite.Alpha; }
            set { _sprite.Alpha = value; }
        }

        public float Depth{
            get { return _sprite.Depth; }
            set { _sprite.Depth = value; }
        }

        public bool HitTest(int x, int y){
            return _boundingBox.Contains(x, y);
        }

        #region ctor

        //xx why are these position coordinates all floats?
        public Button(float x, float y, float width, float height, DepthLevel depth, string textureName, float spriteTexRepeatX = DefaultTexRepeat, float spriteTexRepeatY = DefaultTexRepeat, int identifier = DefaultIdentifier, IUIComponent[] components = null){
            _identifier = identifier;
            _enabled = true;
            _iEventDispatcher = new ButtonEventDispatcher();

            _centPosition = new Vector2();
            _boundingBox = new FloatingRectangle(x, y, width, height);
            _sprite = new Sprite2D(textureName, (int) x, (int) y, (int) width, (int) height, (float) depth/10, 1, spriteTexRepeatX, spriteTexRepeatY);

            _centPosition.X = _boundingBox.X + _boundingBox.Width/2;
            _centPosition.Y = _boundingBox.Y + _boundingBox.Height/2;

            Components = components;
            Alpha = 1;
            if (Components != null){
                foreach (IUIComponent component in Components){
                    component.ComponentCtor(this, _iEventDispatcher);
                }
            }
            UIElementCollection.AddElement(this);
        }

        #endregion

        #region other IUIElement derived methods

        public TComponent GetComponent<TComponent>(string identifier = null){
            if (Components != null){
                foreach (IUIComponent component in Components){
                    if (component is TComponent){
                        if (identifier != null){
                            if (component.Identifier == identifier){
                                return (TComponent) component;
                            }
                            continue;
                        }
                        return (TComponent) component;
                    }
                }
            }
            throw new Exception("Request made to a Button object for a component that did not exist.");
        }

        public bool DoesComponentExist<TComponent>(string identifier = null){
            if (Components != null){
                foreach (var component in Components){
                    if (component is TComponent){
                        if (identifier != null){
                            if (component.Identifier == identifier){
                                return true;
                            }
                            continue;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public void UpdateLogic(double timeDelta){
            if (Enabled){
                if (Components != null){
                    foreach (IUIComponent component in Components){
                        component.Update();
                    }
                }
            }
        }

        public void UpdateInput(ref InputState state){
            if (Enabled){
                bool containedMousePrev = ContainsMouse;
                ContainsMouse = HitTest(state.MousePos.X, state.MousePos.Y);

                if (state.AllowLeftButtonInterpretation){
                    if (state.LeftButtonClick){
                        foreach (var @event in _iEventDispatcher.OnGlobalLeftClick){
                            @event.OnLeftButtonClick(ref state.AllowLeftButtonInterpretation, state.MousePos, state.PrevState.MousePos);
                            if (!state.AllowLeftButtonInterpretation){
                                break;
                            }
                        }
                    }
                }
                if (state.AllowLeftButtonInterpretation){
                    if (state.LeftButtonState == ButtonState.Pressed){
                        foreach (var @event in _iEventDispatcher.OnGlobalLeftPress){
                            @event.OnLeftButtonPress(ref state.AllowLeftButtonInterpretation, state.MousePos, state.PrevState.MousePos);
                            if (!state.AllowLeftButtonInterpretation){
                                break;
                            }
                        }
                    }
                }
                if (state.AllowLeftButtonInterpretation){
                    if (state.LeftButtonState == ButtonState.Released){
                        foreach (var @event in _iEventDispatcher.OnGlobalLeftRelease){
                            @event.OnLeftButtonRelease(ref state.AllowLeftButtonInterpretation, state.MousePos, state.PrevState.MousePos);
                            if (!state.AllowLeftButtonInterpretation){
                                break;
                            }
                        }
                    }
                }
                if (state.AllowMouseMovementInterpretation){
                    foreach (var @event in _iEventDispatcher.OnMouseMovement){
                        @event.OnMouseMovement(ref state.AllowMouseMovementInterpretation, state.MousePos, state.PrevState.MousePos);
                        if (!state.AllowMouseMovementInterpretation){
                            break;
                        }
                    }
                }
                if (state.AllowMouseMovementInterpretation){
                    if(ContainsMouse && !containedMousePrev){
                        foreach (var @event in _iEventDispatcher.OnMouseEntry){
                            @event.OnMouseEntry(ref state.AllowMouseMovementInterpretation, state.MousePos, state.PrevState.MousePos);
                            if (!state.AllowMouseMovementInterpretation){
                                break;
                            }
                        }
                    }
                }
                if (state.AllowMouseMovementInterpretation){
                    if(!ContainsMouse && containedMousePrev){
                        foreach (var @event in _iEventDispatcher.OnMouseExit){
                            @event.OnMouseExit(ref state.AllowMouseMovementInterpretation, state.MousePos, state.PrevState.MousePos);
                            if (!state.AllowMouseMovementInterpretation){
                                break;
                            }
                        }
                    }
                }

                //now dispatch the external delegates
                if (state.AllowLeftButtonInterpretation){
                    if (_boundingBox.Contains(state.MousePos.X, state.MousePos.Y)){
                        state.AllowLeftButtonInterpretation = false;
                        if (state.LeftButtonClick){
                            if (OnLeftClickDispatcher != null){
                                OnLeftClickDispatcher.Invoke(Identifier);
                            }
                        }
                        if (state.LeftButtonState == ButtonState.Released){
                            if (OnLeftReleaseDispatcher != null){
                                OnLeftReleaseDispatcher.Invoke(Identifier);
                            }
                        }
                        if (state.LeftButtonState == ButtonState.Pressed){
                            if (OnLeftPressDispatcher != null){
                                OnLeftPressDispatcher.Invoke(Identifier);
                            }
                        }
                    }
                }
            }
        }

        public void Dispose(){
            throw new NotImplementedException();
        }

        #endregion
    }

    internal class ButtonEventDispatcher{
        public ButtonEventDispatcher(){
            OnGlobalLeftClick = new List<IAcceptLeftButtonClickEvent>();
            OnGlobalLeftPress = new List<IAcceptLeftButtonPressEvent>();
            OnGlobalLeftRelease = new List<IAcceptLeftButtonReleaseEvent>();
            OnMouseEntry = new List<IAcceptMouseEntryEvent>();
            OnMouseExit = new List<IAcceptMouseExitEvent>();
            OnMouseMovement = new List<IAcceptMouseMovementEvent>();
            OnMouseScroll = new List<IAcceptMouseScrollEvent>();
        }

        public List<IAcceptLeftButtonClickEvent> OnGlobalLeftClick { get; set; }
        public List<IAcceptLeftButtonPressEvent> OnGlobalLeftPress { get; set; }
        public List<IAcceptLeftButtonReleaseEvent> OnGlobalLeftRelease { get; set; }
        public List<IAcceptMouseEntryEvent> OnMouseEntry { get; set; }
        public List<IAcceptMouseExitEvent> OnMouseExit { get; set; }
        public List<IAcceptMouseMovementEvent> OnMouseMovement { get; set; }
        public List<IAcceptMouseScrollEvent> OnMouseScroll { get; set; }
    }
}