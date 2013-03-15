using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Forge.Framework.UI.Widgets {
    public class DropdownMenu : IUIElementBase{
        public event Action<string> OnOptionSelected;

        //readonly Button _dropdownBG;
        readonly Button _dropdownButton;
        readonly Button _dropdownTopElem;
        readonly TextBox _selectedText;

        readonly List<Button> _dropdownHighlights;
        readonly List<TextBox> _dropdownText;
        Rectangle _strayMouseBounds;

        readonly string[] _options;

        bool _submenuVisible;

        public DropdownMenu(int x, int y, string startingOption, string[] options){
            const int width = 100;
            const int baseHeight = 16;
            const int menuWidth = width - baseHeight;
            const string dropdownButtonTex = "Icons/DownArrow";
            const string dropdownParentButTex = "Materials/DarkTextBoxMid";
            const string dropdownBGTex = "Materials/TextBoxMid";
            _options = options;
            _strayMouseBounds = new Rectangle(
                x - 25,
                y - 25,
                width + 60,
                baseHeight * options.Length + baseHeight + 50
                );

            #region generate top

            var buttonGen = new ButtonGenerator();
            buttonGen.Depth = DepthLevel.Medium;

            buttonGen.TextureName = dropdownBGTex;
            buttonGen.X = x;
            buttonGen.Y = y + baseHeight;
            buttonGen.Width = menuWidth;
            buttonGen.Height = baseHeight*options.Length;
            //_dropdownBG = buttonGen.GenerateButton();
            //_dropdownBG.Alpha = 0.5f;

            buttonGen.X = x;
            buttonGen.Y = y;
            buttonGen.Height = baseHeight;
            buttonGen.TextureName = dropdownParentButTex;
            _dropdownTopElem = buttonGen.GenerateButton();

            buttonGen.X = x + menuWidth;
            buttonGen.TextureName = dropdownButtonTex;
            buttonGen.Width = baseHeight;
            _dropdownButton = buttonGen.GenerateButton();

            _dropdownTopElem.OnLeftPressDispatcher += identifier => ShowSubmenu();
            _dropdownButton.OnLeftPressDispatcher += identifier => ShowSubmenu();

            buttonGen = new ButtonGenerator("ToolbarButton45.json");

            buttonGen.Height = baseHeight;
            buttonGen.Depth = DepthLevel.High;
            buttonGen.Width = menuWidth;
            buttonGen.TextureName = dropdownBGTex;
            buttonGen.X = x;

            _selectedText = new TextBox(
                x + 1,
                y + 1,
                DepthLevel.Highlight,
                Color.Black
                );
            _selectedText.SetText(startingOption);

            #endregion

            #region generate dropdown

            int yPos = y + baseHeight;
            _dropdownText = new List<TextBox>(options.Length);
            _dropdownHighlights = new List<Button>(options.Length);
            int id = 0;
            foreach (var option in options){
                _dropdownText.Add(new TextBox(
                    x + 1,
                    yPos + 1,
                    DepthLevel.Highlight,
                    Color.Black
                    ));
                _dropdownText.Last().SetText(option);

                buttonGen.Identifier = id;
                buttonGen.Y = yPos;
                buttonGen.Identifier = id;
                _dropdownHighlights.Add(
                    buttonGen.GenerateButton());

                _dropdownHighlights.Last().OnLeftClickDispatcher += OnSelectionMade;
                _dropdownHighlights.Last().Enabled = false;
                _dropdownText.Last().Enabled = false;
                yPos += baseHeight;
                id++;
            }

            #endregion

            UIElementCollection.BoundCollection.AddElement(this);
        }

        void OnSelectionMade(int identifier){
            HideSubmenu();
            _selectedText.SetText(_options[identifier]);

            if (OnOptionSelected != null){
                OnOptionSelected.Invoke(_options[identifier]);
            }
        }

        void ShowSubmenu(){
            foreach (var button in _dropdownHighlights){
                button.Enabled = true;
            }
            foreach (var textBox in _dropdownText){
                textBox.Enabled = true;
            }
            _submenuVisible = true;
        }

        void HideSubmenu(){
            foreach (var button in _dropdownHighlights) {
                button.Enabled = false;
            }
            foreach (var textBox in _dropdownText) {
                textBox.Enabled = false;
            }
            _submenuVisible = false;
        }

        public void UpdateLogic(double timeDelta){
            //throw new NotImplementedException();
        }

        public void UpdateInput(ref InputState state){
            if (_submenuVisible){
                if (!_strayMouseBounds.Contains(state.MousePos.X, state.MousePos.Y)){
                    HideSubmenu();
                }
            }
        }

        public float X{
            get { return _dropdownTopElem.X; }
            set { throw new NotImplementedException(); }
        }

        public float Y{
            get { return _dropdownTopElem.Y; }
            set { throw new NotImplementedException(); }
        }

        public float Width{
            get { return _dropdownTopElem.Width; }
            set { throw new NotImplementedException(); }
        }

        public float Height{
            get { return _dropdownTopElem.Height; }
            set { throw new NotImplementedException(); }
        }

        public float Alpha{
            get { return 1; }
            set { throw new NotImplementedException(); }
        }

        public float Depth{
            get { return _dropdownTopElem.Depth; }
            set { throw new NotImplementedException(); }
        }

        public bool HitTest(int x, int y){
            return false;
        }

        public List<IUIElementBase> GetElementStack(int x, int y){
            return new List<IUIElementBase>();
        }
    }
}