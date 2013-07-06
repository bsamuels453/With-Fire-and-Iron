#region

using System.Collections.Generic;
using Forge.Framework.Control;
using Forge.Framework.Draw;
using Microsoft.Xna.Framework.Input;
using MonoGameUtility;

#endregion

namespace Forge.Framework.UI.Elements{
    internal class MouseoverMask : IUIElement{
        const float _fadedInAlpha = 0.1f;
        readonly MaskingSprite _mask;
        float _alpha;
        Rectangle _boundingBox;

        public MouseoverMask(Rectangle boundingBox, UIElementCollection parent){
            FrameStrata = new FrameStrata(FrameStrata.Level.Highlight, parent.FrameStrata, "MouseoverMaskLayer");
            _boundingBox = boundingBox;
            MouseController = new MouseController(this);
            parent.OnMouseEntry += OnMouseEntry;
            parent.OnMouseExit += OnMouseExit;
            parent.OnLeftDown += OnMouseLeftDown;
            parent.OnLeftRelease += OnMouseLeftUp;

            _mask = new MaskingSprite
                (
                "Materials/SolidYellow",
                boundingBox
                );

            Alpha = 0;
        }

        #region IUIElement Members

        public FrameStrata FrameStrata { get; private set; }

        public int X{
            get { return _boundingBox.X; }
            set{
                _boundingBox.X = value;
                _mask.X = value;
            }
        }

        public int Y{
            get { return _boundingBox.Y; }
            set{
                _boundingBox.Y = value;
                _mask.Y = value;
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
                _mask.Alpha = value;
            }
        }

        public MouseController MouseController { get; private set; }

        public bool HitTest(int x, int y){
            return false;
        }

        public List<IUIElement> GetElementStackAtPoint(int x, int y){
            return new List<IUIElement>();
        }

        public void Update(float timeDelta){
        }

        public void Dispose(){
            _mask.Dispose();
        }

        #endregion

        void OnMouseEntry(ForgeMouseState state, float timeDelta, UIElementCollection caller){
            if (state.LeftButtonState == ButtonState.Released){
                Alpha = _fadedInAlpha;
            }
        }

        void OnMouseExit(ForgeMouseState state, float timeDelta, UIElementCollection caller){
            Alpha = 0;
        }

        void OnMouseLeftDown(ForgeMouseState state, float timeDelta, UIElementCollection caller){
            Alpha = 0;
        }

        void OnMouseLeftUp(ForgeMouseState state, float timeDelta, UIElementCollection caller){
            if (caller.ContainsMouse){
                Alpha = _fadedInAlpha;
            }
        }
    }
}