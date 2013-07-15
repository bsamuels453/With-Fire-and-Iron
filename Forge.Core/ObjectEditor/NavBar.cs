#region

using System;
using Forge.Framework.Control;
using Forge.Framework.Resources;
using Forge.Framework.UI;
using Forge.Framework.UI.Elements;
using MonoGameUtility;

#endregion

namespace Forge.Core.ObjectEditor{
    internal class NavBar : ElementGrid{
        const string _template = "UiTemplates/Specialized/NavBar.json";

        readonly ToolbarButton _downButton;
        readonly ToolbarButton _upButton;

        public NavBar(UIElementCollection parent, FrameStrata.Level depth, Point position) : base(parent, depth, position, _template){
            var jobj = Resource.LoadJObject(_template);
            var upTex = jobj["UpButtonTex"].ToObject<string>();
            var downTex = jobj["DownButtonTex"].ToObject<string>();

            _upButton = new ToolbarButton(this, FrameStrata.Level.Medium, new Point(), upTex);
            _downButton = new ToolbarButton(this, FrameStrata.Level.Medium, new Point(), downTex);

            base.AddGridElement(_upButton, 0, 0);
            base.AddGridElement(_downButton, 0, 1);
        }

        public Action<ForgeMouseState, float, UIElementCollection> OnUpPressed{
            set { _upButton.OnLeftClick += value; }
        }

        public Action<ForgeMouseState, float, UIElementCollection> OnDownPressed{
            set { _downButton.OnLeftClick += value; }
        }
    }
}