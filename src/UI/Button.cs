#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gondola.Common;
using Gondola.Draw;
using Gondola.Logic;
using Gondola.UI.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;

#endregion

namespace Gondola.UI{
    internal class Button{
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

        public void Draw(){
            _sprite.Draw();
            if (Components != null){
                foreach (var component in Components){
                    component.Draw();
                }
            }
        }

        //xxx fix loops
        public void Update(ref InputState state, double timeDelta){
            if (IsEnabled){
                if (ContainsMouse && !ContainedMousePrevisly)
                    ContainedMousePrevisly = true;

                if (!ContainsMouse && ContainedMousePrevisly)
                    ContainedMousePrevisly = false;

                if (BoundingBox.Contains(state.MousePos.X, state.MousePos.Y) && !ContainsMouse){
                    ContainsMouse = true;
                    ContainedMousePrevisly = false;
                }

                if (!BoundingBox.Contains(state.MousePos.X, state.MousePos.Y) && ContainsMouse){
                    ContainsMouse = false;
                    ContainedMousePrevisly = true;
                }


                if (Components != null){
                    foreach (IUIComponent component in Components){
                        component.Update(state, timeDelta);
                    }
                }
                /*
                if (state.AllowLeftButtonInterpretation){
                    if (state.LeftButtonChange){
                        foreach (var @event in _iEventDispatcher.OnLeftButtonClick){
                            @event.OnLeftButtonClick(ref state.AllowLeftButtonInterpretation, state.MousePos, state.PrevState.MousePos);
                            if (!state.AllowLeftButtonInterpretation)
                                break;
                        }
                    }
                }
                if (state.AllowLeftButtonInterpretation){
                    if (state.LeftButtonState == ButtonState.Pressed){
                        foreach (var @event in _iEventDispatcher.OnLeftButtonPress){
                            @event.OnLeftButtonPress(ref state.AllowLeftButtonInterpretation, state.MousePos, state.PrevState.MousePos);
                            if (!state.AllowLeftButtonInterpretation)
                                break;
                        }
                    }
                }
                if (state.AllowLeftButtonInterpretation){
                    if (state.LeftButtonState == ButtonState.Released){
                        foreach (var @event in _iEventDispatcher.OnLeftButtonRelease){
                            @event.OnLeftButtonRelease(ref state.AllowLeftButtonInterpretation, state.MousePos, state.PrevState.MousePos);
                            if (!state.AllowLeftButtonInterpretation)
                                break;
                        }
                    }
                }
                if (state.AllowMouseMovementInterpretation){
                    foreach (var @event in _iEventDispatcher.OnMouseMovement){
                        @event.OnMouseMovement(ref state.AllowMouseMovementInterpretation, state.MousePos, state.PrevState.MousePos);
                        if (!state.AllowMouseMovementInterpretation)
                            break;
                    }
                }
                if (state.AllowMouseMovementInterpretation){
                    if (BoundingBox.Contains(state.MousePos.X, state.MousePos.Y) && !ContainsMouse){
                        ContainsMouse = true;
                        foreach (var @event in _iEventDispatcher.OnMouseEntry){
                            @event.OnMouseEntry(ref state.AllowMouseMovementInterpretation, state.MousePos, state.PrevState.MousePos);
                            if (!state.AllowMouseMovementInterpretation)
                                break;
                        }
                    }
                }
                if (state.AllowMouseMovementInterpretation){
                    if (!BoundingBox.Contains(state.MousePos.X, state.MousePos.Y) && ContainsMouse){
                        ContainsMouse = false;
                        foreach (var @event in _iEventDispatcher.OnMouseExit){
                            @event.OnMouseExit(ref state.AllowMouseMovementInterpretation, state.MousePos, state.PrevState.MousePos);
                            if (!state.AllowMouseMovementInterpretation)
                                break;
                        }
                    }
                }
                if (state.AllowKeyboardInterpretation){
                    foreach (var @event in _iEventDispatcher.OnKeyboardEvent){
                        @event.OnKeyboardEvent(ref state.AllowKeyboardInterpretation, state.KeyboardState);
                        if (!state.AllowKeyboardInterpretation)
                            break;
                    }
                }
                */
                //now dispatch the external delegates
                if (state.AllowLeftButtonInterpretation){
                    if (BoundingBox.Contains(state.MousePos.X, state.MousePos.Y)){
                        state.AllowLeftButtonInterpretation = false;
                        if (state.LeftButtonChange){
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

        //unfortunately we have to implement this because microsoft is too stupid to
        //allow Rectangle to accept point into its contains method
        public bool Contains(Point point){
            if (point.X > BoundingBox.X &&
                point.X < BoundingBox.X + BoundingBox.Width &&
                point.Y > BoundingBox.Y &&
                point.Y < BoundingBox.Y + BoundingBox.Height){
                return true;
            }
            return false;
        }

        #region Nested type: OnBasicMouseEvent

        internal delegate void OnBasicMouseEvent(int identifier);

        #endregion

        #region properties and fields

        public const int DefaultTexRepeat = 1;
        public const int DefaultIdentifier = 1;

        readonly FloatingRectangle _boundingBox; //bounding box that represents the bounds of the button
        readonly int _identifier; //non-function based identifier that can be used to differentiate buttons
        readonly Sprite2D _sprite; //the button's sprite
        Vector2 _centPosition; //represents the approximate center of the button
        bool _isEnabled;
        string _texture;

        public RenderTarget RenderTarget { get; private set; }

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

        //xxxx setting these position variables after construction wont update highlight masks
        public int X{
            get { return (int) _boundingBox.X; }
            set{
                _boundingBox.X = value;
                _centPosition.X = _boundingBox.X + _boundingBox.Width/2;
                _sprite.X = value;
            }
        }

        public int Y{
            get { return (int) _boundingBox.Y; }
            set{
                _boundingBox.Y = value;
                _centPosition.Y = _boundingBox.Y + _boundingBox.Height/2;
                _sprite.Y = value;
            }
        }

        public int Width{
            get { return (int) _boundingBox.Width; }
            set{
                _boundingBox.Width = value;
                _sprite.Width = value;
            }
        }

        public int Height{
            get { return (int) _boundingBox.Height; }
            set{
                _boundingBox.Height = value;
                _sprite.Height = value;
            }
        }

        public FloatingRectangle BoundingBox{
            get { return _boundingBox; }
        }

        public bool IsEnabled{
            get { return _isEnabled; }
            set{
                _isEnabled = value;
                _sprite.IsEnabled = value;
                foreach (var component in Components){
                    component.IsEnabled = value;
                }
            }
        }

        public bool ContainsMouse { get; set; }

        public bool ContainedMousePrevisly { get; private set; }

        public float Opacity{
            get { return _sprite.Opacity; }
            set { _sprite.Opacity = value; }
        }

        public event OnBasicMouseEvent OnLeftClickDispatcher;
        public event OnBasicMouseEvent OnLeftPressDispatcher;
        public event OnBasicMouseEvent OnLeftReleaseDispatcher;

        #endregion

        #region ctor

        //xxx why are these position coordinates all floats?
        public Button(RenderTarget target, int x, int y, int width, int height, string textureName, float spriteTexRepeatX = DefaultTexRepeat, float spriteTexRepeatY = DefaultTexRepeat, int identifier = DefaultIdentifier, IUIComponent[] components = null){
            RenderTarget = target;
            _identifier = identifier;
            _isEnabled = true;

            _centPosition = new Vector2();
            _boundingBox = new FloatingRectangle(x, y, width, height);
            _sprite = new Sprite2D(target, textureName, x, y, width, height, 1, spriteTexRepeatX, spriteTexRepeatY);

            _centPosition.X = _boundingBox.X + _boundingBox.Width/2;
            _centPosition.Y = _boundingBox.Y + _boundingBox.Height/2;

            Components = components;
            Opacity = 1;
            if (Components != null){
                foreach (IUIComponent component in Components){
                    component.ComponentCtor(this);
                }
            }
            ////UIElementCollection.AddElement(this);
        }

        #endregion
    }

    internal class ButtonGenerator{
        public Dictionary<string, JObject> Components;
        public float? Height;
        public int? Identifier;
        public RenderTarget RenderTarget;
        public float? SpriteTexRepeatX;
        public float? SpriteTexRepeatY;
        public string TextureName;
        public float? Width;
        public float? X;
        public float? Y;

        public ButtonGenerator(){
            Components = null;
            RenderTarget = null;
            Height = null;
            Width = null;
            Identifier = null;
            SpriteTexRepeatX = null;
            SpriteTexRepeatY = null;
            TextureName = null;
            X = null;
            Y = null;
        }

        public ButtonGenerator(string template){
            string str;
            using (var sr = new StreamReader("Raw/Templates/" + template)){
                str = sr.ReadToEnd();
            }
            JObject obj = JObject.Parse(str);

            //try{
            var jComponents = obj["Components"];
            if (jComponents != null)
                Components = jComponents.ToObject<Dictionary<string, JObject>>();
            else
                Components = null;
            //}
            //catch (InvalidCastException){
            //Components = null;
            //}

            Width = obj["Width"].Value<float>();
            Height = obj["Height"].Value<float>();
            Identifier = obj["Identifier"].Value<int>();
            SpriteTexRepeatX = obj["SpriteTexRepeatX"].Value<float>();
            SpriteTexRepeatY = obj["SpriteTexRepeatY"].Value<float>();
            TextureName = obj["TextureName"].Value<string>();
        }

        public Button GenerateButton(){
            //make sure we have all the data required
            if (X == null ||
                Y == null ||
                Width == null ||
                Height == null ||
                TextureName == null ||
                RenderTarget == null){
                throw new Exception("Template did not contain all of the basic variables required to generate a button.");
            }
            //generate component list
            IUIComponent[] components = null;
            if (Components != null){
                components = GenerateComponents(Components);
            }

            //now we handle optional parameters
            float spriteTexRepeatX;
            float spriteTexRepeatY;
            int identifier;

            if (SpriteTexRepeatX != null)
                spriteTexRepeatX = (float) SpriteTexRepeatX;
            else
                spriteTexRepeatX = Button.DefaultTexRepeat;

            if (SpriteTexRepeatY != null)
                spriteTexRepeatY = (float) SpriteTexRepeatY;
            else
                spriteTexRepeatY = Button.DefaultTexRepeat;

            if (Identifier != null)
                identifier = (int) Identifier;
            else
                identifier = Button.DefaultIdentifier;

            return new Button(
                RenderTarget,
                (int) X,
                (int) Y,
                (int) Width,
                (int) Height,
                TextureName,
                spriteTexRepeatX,
                spriteTexRepeatY,
                identifier,
                components
                );
        }

        IUIComponent[] GenerateComponents(Dictionary<string, JObject> componentCtorData){
            var components = new List<IUIComponent>();

            foreach (var data in componentCtorData){
                string str = data.Key;
                //when there are multiple components, they are named "Componentname_n" where n is the number of the component
                //gotta remove that for the switch, if it exists
                string identifier = "";
                if (str.Contains('_')){
                    identifier = str.Substring(str.IndexOf('_') + 1, str.Count() - str.IndexOf('_') - 1);
                    str = str.Substring(0, str.IndexOf('_'));
                }

                switch (str){
                    case "FadeComponent":
                        components.Add(FadeComponent.ConstructFromObject(data.Value, identifier));
                        break;
                    case "DraggableComponent":
                        components.Add(DraggableComponent.ConstructFromObject(data.Value));
                        break;
                    case "PanelComponent":
                        components.Add(PanelComponent.ConstructFromObject(data.Value));
                        break;
                    case "HighlightComponent":
                        components.Add(HighlightComponent.ConstructFromObject(data.Value, identifier));
                        break;
                }
            }

            return components.ToArray();
        }
    }
}