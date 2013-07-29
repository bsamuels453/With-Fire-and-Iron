#region

using System.Diagnostics;
using Forge.Framework.Control;
using Forge.Framework.Draw;
using Forge.Framework.Resources;
using MonoGameUtility;

#endregion

namespace Forge.Framework.UI.Elements{
    public class ElementGrid : DraggableCollection{
        const string _selectionMaskTex = "Effects/SolidBlack";
        const float _selectionMaskAlpha = 0.5f;
        protected readonly IUIElement[,] Elements;
        readonly int _gridInsetX;
        readonly int _gridInsetY;
        readonly int _horizPadding;
        readonly int _itemHeight;
        readonly int _itemWidth;
        readonly Sprite2D _selectionMask;
        readonly bool _useSelectionMask;
        readonly int _vertPadding;

        public ElementGrid(
            UIElementCollection parent,
            FrameStrata.Level depth,
            Point position,
            string template)
            : base(parent, depth, new Rectangle(), "ElementGrid"){
            var jobj = Resource.LoadJObject(template);

            var horizItems = jobj["NumHorizontalItems"].ToObject<int>();
            var vertItems = jobj["NumVerticalItems"].ToObject<int>();
            _horizPadding = jobj["HorizontalPadding"].ToObject<int>();
            _vertPadding = jobj["VerticalPadding"].ToObject<int>();
            _itemWidth = jobj["ItemWidth"].ToObject<int>();
            _itemHeight = jobj["ItemHeight"].ToObject<int>();
            _gridInsetX = jobj["GridInsetX"].ToObject<int>();
            _gridInsetY = jobj["GridInsetY"].ToObject<int>();
            _useSelectionMask = jobj["SelectionMask"].ToObject<bool>();

            int width = horizItems*_itemWidth + (horizItems + 1)*_horizPadding + _gridInsetX*2;
            int height = vertItems*_itemHeight + (vertItems + 1)*_vertPadding + _gridInsetY*2;

            Elements = new IUIElement[horizItems,vertItems];

            base.BoundingBox = new Rectangle(position.X, position.Y, width, height);

            if (_useSelectionMask){
                _selectionMask = new Sprite2D(_selectionMaskTex, base.X, base.Y, _itemWidth, _itemHeight, FrameStrata, FrameStrata.Level.Highlight, true);
                _selectionMask.Alpha = _selectionMaskAlpha;
                _selectionMask.Enabled = false;
                this.AddElement(_selectionMask);
                this.OnLeftDown += ProcSelectionMask;
            }
        }

        void ProcSelectionMask(ForgeMouseState state, float delta, UIElementCollection elem){
            if (elem.ContainsPoint(state.X, state.Y)){
                foreach (var element in Elements){
                    if (element.HitTest(state.X, state.Y)){
                        _selectionMask.X = element.X;
                        _selectionMask.Y = element.Y;
                        _selectionMask.Enabled = true;
                        break;
                    }
                }
            }
        }

        protected void AddGridElement(UIElementCollection element, int x, int y){
            var pos = CalculateElementPosition(x, y);
            element.X = pos.X;
            element.Y = pos.Y;

            Debug.Assert(element.Width == _itemWidth);
            Debug.Assert(element.Height == _itemHeight);

            Elements[x, y] = element;
        }

        protected void AddGridElement(IUIElement element, int x, int y){
            var pos = CalculateElementPosition(x, y);
            element.X = pos.X;
            element.Y = pos.Y;

            Debug.Assert(element.Width == _itemWidth);
            Debug.Assert(element.Height == _itemHeight);

            this.AddElement(element);
            Elements[x, y] = element;
        }

        protected Point CalculateElementPosition(int cellX, int cellY){
            int x = base.BoundingBox.X + _gridInsetX + cellX*_itemWidth + (cellX + 1)*_horizPadding;
            int y = base.BoundingBox.Y + _gridInsetY + cellY*_itemHeight + (cellY + 1)*_vertPadding;
            return new Point(x, y);
        }
    }
}