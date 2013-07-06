#region

using System.Diagnostics;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework;
using Point = MonoGameUtility.Point;
using Rectangle = MonoGameUtility.Rectangle;

#endregion

namespace Forge.Framework.UI.Elements{
    public class DialogueButton : Panel{
        public DialogueButton(
            UIElementCollection parent,
            FrameStrata.Level depth,
            Point position,
            string text
            ) : base(
                parent,
                depth,
                new Rectangle(),
                "DialogueButton",
                Resource.LoadJObject("UiTemplates/DialogueButton.json")["BackgroundDefinition"].ToObject<string>()){
            //my god that base ctor should be illegal
            var jobj = Resource.LoadJObject("UiTemplates/DialogueButton.json");
            string textFont = jobj["TextFont"].ToObject<string>();
            int horizontalTextPadding = jobj["HorizontalTextPadding"].ToObject<int>();
            int height = jobj["Height"].ToObject<int>();
            int textVOffset = jobj["TextVerticalOffset"].ToObject<int>();

            var dims = TextBox.MeasureString(textFont, text);

            Debug.Assert(dims.Y < height);

            this.BoundingBox = new Rectangle
                (
                position.X,
                position.Y,
                (int) (dims.X + horizontalTextPadding*2),
                height
                );


            var clickMask = new ClickMask(BoundingBox, this);
            var mouseoverMask = new MouseoverMask(BoundingBox, this);

            var textObject = new TextBox
                (
                position + new Point(horizontalTextPadding, textVOffset),
                this,
                FrameStrata.Level.High,
                Color.White,
                textFont,
                (int) dims.X,
                1,
                TextBox.Justification.Center
                );
            textObject.SetText(text);


            AddElement(clickMask);
            AddElement(mouseoverMask);
            AddElement(textObject);
            base.GenerateBackgroundSprite();
            base.Draggable = false;
        }
    }
}