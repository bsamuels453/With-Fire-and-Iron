﻿namespace Forge.Framework.UI.Widgets{
    public class Checkbox{
        readonly Button _boxSprite;
        readonly Button _checkSprite;

        public Checkbox(int x, int y, bool startChecked, int identifier = 0){
            var buttonGen = new ButtonGenerator("CheckboxBG.json");
            buttonGen.X = x;
            buttonGen.Y = y;
            _boxSprite = buttonGen.GenerateButton();

            buttonGen = new ButtonGenerator("CheckboxMark.json");
            buttonGen.X = x;
            buttonGen.Y = y;
            _checkSprite = buttonGen.GenerateButton();

            if (startChecked)
                Check();
            else
                UnCheck();

            _checkSprite.OnLeftClickDispatcher += _ => UnCheck();
            _boxSprite.OnLeftClickDispatcher += _ => Check();

            Identifier = identifier;
        }

        public int Identifier { get; private set; }

        public void Check(){
            _checkSprite.Alpha = 1;
            _checkSprite.Enabled = true;
        }

        public void UnCheck(){
            _checkSprite.Alpha = 0;
            _checkSprite.Enabled = false;
        }
    }
}