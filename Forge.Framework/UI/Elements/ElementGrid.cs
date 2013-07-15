#region

using System.Diagnostics;
using Forge.Framework.Resources;
using MonoGameUtility;

#endregion

namespace Forge.Framework.UI.Elements{
    public class ElementGrid : DraggableCollection{
        readonly IUIElement[,] _elements;
        readonly int _gridInsetX;
        readonly int _gridInsetY;
        readonly int _horizPadding;
        readonly int _itemHeight;
        readonly int _itemWidth;
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

            int width = horizItems*_itemWidth + (horizItems + 1)*_horizPadding + _gridInsetX*2;
            int height = vertItems*_itemHeight + (vertItems + 1)*_vertPadding + _gridInsetY*2;

            _elements = new IUIElement[horizItems,vertItems];

            base.BoundingBox = new Rectangle(position.X, position.Y, width, height);
        }

        public void AddGridElement(UIElementCollection element, int x, int y){
            element.X = base.BoundingBox.X + _gridInsetX + x*_itemWidth + (x + 1)*_horizPadding;
            element.Y = base.BoundingBox.Y + _gridInsetY + y*_itemHeight + (y + 1)*_vertPadding;

            Debug.Assert(element.Width == _itemWidth);
            Debug.Assert(element.Height == _itemHeight);

            _elements[x, y] = element;
        }

        public void AddGridElement(IUIElement element, int x, int y){
            element.X = base.BoundingBox.X + _gridInsetX + x*_itemWidth + (x + 1)*_horizPadding;
            element.Y = base.BoundingBox.Y + _gridInsetY + y*_itemHeight + (y + 1)*_vertPadding;

            Debug.Assert(element.Width == _itemWidth);
            Debug.Assert(element.Height == _itemHeight);

            this.AddElement(element);
            _elements[x, y] = element;
        }
    }
}