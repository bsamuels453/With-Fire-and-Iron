#region

using System.Collections.Generic;
using Forge.Framework.Control;
using Forge.Framework.Draw;
using MonoGameUtility;

#endregion

namespace Forge.Framework.UI.Elements{
    internal class ClickMask : IUIElement{
        const float _fadedInAlpha = 0.2f;
        readonly Sprite2D _sprite;
        float _alpha;
        Rectangle _boundingBox;

        public ClickMask(Rectangle boundingBox, FrameStrata parentStrata){
            FrameStrata = new FrameStrata(FrameStrata.Level.High, parentStrata, "ClickMask");
            _boundingBox = boundingBox;
            MouseController = new MouseController(this);

            _sprite = new Sprite2D
                (
                "Materials/SolidBlack",
                boundingBox.X,
                boundingBox.Y,
                boundingBox.Width,
                boundingBox.Height,
                FrameStrata
                );

            Alpha = 0;
        }

        #region IUIElement Members

        public FrameStrata FrameStrata { get; private set; }

        public int X{
            get { return _boundingBox.X; }
            set{
                _boundingBox.X = value;
                _sprite.X = value;
            }
        }

        public int Y{
            get { return _boundingBox.Y; }
            set{
                _boundingBox.Y = value;
                _sprite.Y = value;
            }
        }

        public int Width{
            get { return _boundingBox.Width; }
        }

        public int Height{
            get { return _boundingBox.Height; }
        }

        public float Alpha{
            get { return _alpha; }
            set{
                _alpha = value;
                _sprite.Alpha = value;
            }
        }

        public MouseController MouseController { get; private set; }

        public bool HitTest(int x, int y){
            return false;
        }

        public List<IUIElement> GetElementStackAtPoint(int x, int y){
            return new List<IUIElement>();
        }

        public void InitializeEvents(UIElementCollection parent){
            parent.OnMouseExit += OnMouseExit;
            parent.OnLeftDown += OnMouseLeftDown;
            parent.OnLeftRelease += OnMouseLeftUp;
        }

        #endregion

        void OnMouseExit(ForgeMouseState state, float timeDelta, UIElementCollection caller){
            Alpha = 0;
        }

        void OnMouseLeftDown(ForgeMouseState state, float timeDelta, UIElementCollection caller){
            if (caller.ContainsMouse){
                Alpha = _fadedInAlpha;
            }
        }

        void OnMouseLeftUp(ForgeMouseState state, float timeDelta, UIElementCollection caller){
            Alpha = 0;
        }
    }
}