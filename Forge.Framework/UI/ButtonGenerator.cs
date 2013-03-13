#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Forge.Framework.UI.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Forge.Framework.UI{
    public class ButtonGenerator{
        public Dictionary<string, JObject> Components;
        public DepthLevel? Depth;
        public float? Height;
        public int? Identifier;
        public float? SpriteTexRepeatX;
        public float? SpriteTexRepeatY;
        public string TextureName;
        public float? Width;
        public float? X;
        public float? Y;

        public ButtonGenerator(){
            Components = null;
            Depth = null;
            Height = null;
            Width = null;
            Identifier = null;
            SpriteTexRepeatX = null;
            SpriteTexRepeatY = null;
            TextureName = null;
            X = null;
            Y = null;
        }

        public ButtonGenerator(string template){
            var sr = new StreamReader("Templates/" + template);
            string str = sr.ReadToEnd();

            JObject obj = JObject.Parse(str);
            var depthLevelSerializer = new JsonSerializer();


            //try{
            var jComponents = obj["Components"];
            if (jComponents != null)
                Components = jComponents.ToObject<Dictionary<string, JObject>>();
            else
                Components = null;
            //}
            //catch (InvalidCastException){
            //Components = null;
            //}

            Depth = obj["Depth"].ToObject<DepthLevel?>(depthLevelSerializer);

            Width = obj["Width"].Value<float>();
            Height = obj["Height"].Value<float>();
            Identifier = obj["Identifier"].Value<int>();
            SpriteTexRepeatX = obj["SpriteTexRepeatX"].Value<float>();
            SpriteTexRepeatY = obj["SpriteTexRepeatY"].Value<float>();
            TextureName = obj["TextureName"].Value<string>();
        }

        public Button GenerateButton(){
            //make sure we have all the data required
            if (X == null ||
                Y == null ||
                    Width == null ||
                        Height == null ||
                            Depth == null ||
                                TextureName == null){
                throw new Exception("Template did not contain all of the basic variables required to generate a button.");
            }
            //generate component list
            IUIComponent[] components = null;
            if (Components != null){
                components = GenerateComponents(Components);
            }

            //now we handle optional parameters
            float spriteTexRepeatX;
            float spriteTexRepeatY;
            int identifier;

            if (SpriteTexRepeatX != null)
                spriteTexRepeatX = (float) SpriteTexRepeatX;
            else
                spriteTexRepeatX = Button.DefaultTexRepeat;

            if (SpriteTexRepeatY != null)
                spriteTexRepeatY = (float) SpriteTexRepeatY;
            else
                spriteTexRepeatY = Button.DefaultTexRepeat;

            if (Identifier != null)
                identifier = (int) Identifier;
            else
                identifier = Button.DefaultIdentifier;

            return new Button(
                (float) X,
                (float) Y,
                (float) Width,
                (float) Height,
                (DepthLevel) Depth,
                TextureName,
                spriteTexRepeatX,
                spriteTexRepeatY,
                identifier,
                components
                );
        }

        IUIComponent[] GenerateComponents(Dictionary<string, JObject> componentCtorData){
            var components = new List<IUIComponent>();

            foreach (var data in componentCtorData){
                string str = data.Key;
                //when there are multiple components, they are named "Componentname_n" where n is the number of the component
                //gotta remove that for the switch, if it exists
                string identifier = "";
                if (str.Contains('_')){
                    identifier = str.Substring(str.IndexOf('_') + 1, str.Count() - str.IndexOf('_') - 1);
                    str = str.Substring(0, str.IndexOf('_'));
                }

                switch (str){
                    case "FadeComponent":
                        components.Add(FadeComponent.ConstructFromObject(data.Value, identifier));
                        break;
                    case "DraggableComponent":
                        components.Add(DraggableComponent.ConstructFromObject(data.Value));
                        break;
                    case "PanelComponent":
                        components.Add(PanelComponent.ConstructFromObject(data.Value));
                        break;
                    case "HighlightComponent":
                        components.Add(HighlightComponent.ConstructFromObject(data.Value, identifier));
                        break;
                }
            }

            return components.ToArray();
        }
    }
}