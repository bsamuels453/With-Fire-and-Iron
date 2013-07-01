#region

using System;
using System.IO;
using Forge.Framework.Control;
using Forge.Framework.Draw;
using MonoGameUtility;
using Newtonsoft.Json.Linq;

#endregion

namespace Forge.Framework.UI.Elements{
    public class Checkbox : UIElementCollection{
        readonly string _bgMaterial;
        readonly int _checkInsetPadding;
        readonly string _checkMaterial;
        readonly Sprite2D _checkSprite;
        readonly int _width;

        public Checkbox(UIElementCollection parent, FrameStrata.Level depth, Point position, string template = "UiTemplates/Checkbox.json") :
            base(parent, depth, new Rectangle(), "Checkbox"){
            #region load template

            var strmrdr = new StreamReader(template);
            var contents = strmrdr.ReadToEnd();
            strmrdr.Close();

            var jobj = JObject.Parse(contents);

            _width = jobj["Width"].ToObject<int>();
            _checkInsetPadding = jobj["CheckInsetPadding"].ToObject<int>();
            _checkMaterial = jobj["CheckMaterial"].ToObject<string>();
            _bgMaterial = jobj["BackgroundMaterial"].ToObject<string>();

            #endregion

            this.BoundingBox = new Rectangle(position.X, position.Y, _width, _width);

            var bg = new Sprite2D
                (
                _bgMaterial,
                position.X,
                position.Y,
                _width,
                _width,
                this.FrameStrata,
                FrameStrata.Level.Background
                );

            _checkSprite = new Sprite2D
                (
                _checkMaterial,
                position.X + _checkInsetPadding,
                position.Y + _checkInsetPadding,
                _width - _checkInsetPadding*2,
                _width - _checkInsetPadding*2,
                this.FrameStrata,
                FrameStrata.Level.Medium
                );

            var mouseoverMask = new MouseoverMask
                (
                this.BoundingBox,
                this
                );

            var clickMask = new ClickMask
                (
                this.BoundingBox,
                this
                );

            this.AddElement(bg);
            this.AddElement(_checkSprite);
            this.AddElement(mouseoverMask);
            this.AddElement(clickMask);

            this.OnLeftRelease += OnClick;

            Checked = true;
        }

        public bool Checked { get; private set; }

        void OnClick(ForgeMouseState state, float timeDelta, UIElementCollection caller){
            if (this.ContainsMouse){
                if (Checked){
                    SetUnchecked();
                }
                else{
                    SetChecked();
                }
            }
        }

        public void SetChecked(){
            Checked = true;
            _checkSprite.Enabled = true;
            if (OnChecked != null){
                OnChecked.Invoke();
            }
        }

        public void SetUnchecked(){
            Checked = false;
            _checkSprite.Enabled = false;
            if (OnUnchecked != null){
                OnUnchecked.Invoke();
            }
        }

        public event Action OnChecked;
        public event Action OnUnchecked;
    }
}