#region

using System;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework;
using Point = MonoGameUtility.Point;
using Rectangle = MonoGameUtility.Rectangle;

#endregion

namespace Forge.Framework.UI.Elements{
    public class DialogueBox : Panel{
        static Point _pos;

        static readonly int _width;
        static readonly int _height;

        static DialogueBox(){
            var jobj = Resource.LoadJObject("UiTemplates/Dialoguebox.json");
            _width = jobj["Width"].ToObject<int>();
            _height = jobj["Height"].ToObject<int>();

            int x, y;
            Resource.ScreenSize.GetScreenValue(0.5f, 0.5f, out x, out y);
            _pos = new Point(x - _width/2, y - _height/2);
        }

        public DialogueBox(UIElementCollection parent, string title, string content) :
            base(parent, FrameStrata.Level.High, new Rectangle(_pos.X, _pos.Y, _width, _height), "Dialoguebox"){
            var jobj = Resource.LoadJObject("UiTemplates/Dialoguebox.json");
            string titleFont = jobj["TitleFont"].ToObject<string>();
            string contentFont = jobj["ContentFont"].ToObject<string>();
            int tcSepDist = jobj["Title-ContentSeparationDist"].ToObject<int>();


            var cell = this.GeneratePanelCell();

            var _ = cell.CreateChild(0, 0, 1, 0, PanelCell.Border.Right | PanelCell.Border.Left);
            int width = _.Area.Width;

            var titletext = new TextBox
                (
                new Point(this.X + BorderPadding, this.Y + BorderPadding),
                this,
                FrameStrata.Level.High,
                Color.White,
                titleFont,
                width,
                1,
                TextBox.Justification.Center);
            titletext.SetText(title);

            var contentText = new TextBox
                (
                new Point(this.X + BorderPadding, titletext.Y + titletext.Height + tcSepDist),
                this,
                FrameStrata.Level.High,
                Color.White,
                contentFont,
                width
                );
            contentText.SetText(content);

            var button = new DialogueButton
                (
                this,
                FrameStrata.Level.Medium,
                new Point
                    (
                    this.X + BorderPadding,
                    0
                    ),
                "Ok"
                );
            button.X = this.X + this.Width / 2 - button.Width / 2;
            button.Y = this.Y + this.Height - BorderPadding - button.Height;

            this.AddElement(titletext);
            this.AddElement(contentText);
            base.GenerateBackgroundSprite();

            button.OnLeftRelease +=
                (state, f, arg3) => {
                    if (arg3.ContainsMouse){
                        if (OnBoxDismissed != null){
                            OnBoxDismissed.Invoke(this);
                        }
                        this.Dispose();
                    }
                };
        }

        public event Action<DialogueBox> OnBoxDismissed;
    }
}