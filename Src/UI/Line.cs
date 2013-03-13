using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gondola.Draw;
using Gondola.UI.Components;
using Gondola.Util;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Gondola.UI {
    internal class Line : IUIElement {
        #region fields and properties, and element modification methods

        public const int DefaultIdentifier = 1;
        readonly Line2D _lineSprite;
        public float Length;
        public Vector2 UnitVector;
        float _angle;
        Vector2 _point1;
        Vector2 _point2;

        public float Angle {
            get { return _angle; }
            set {
                _angle = value;
                UnitVector = Common.GetComponentFromAngle(value, 1);
                CalculateDestFromUnitVector();
            }
        }

        public Vector2 OriginPoint {
            get { return _point1; }
            set {
                _point1 = value;
                CalculateInfoFromPoints();
            }
        }

        public Vector2 DestPoint {
            get { return _point2; }
            set {
                _point2 = value;
                CalculateInfoFromPoints();
            }
        }

        public int LineWidth { get; set; }

        public IUIComponent[] Components { get; set; }

        public IDrawableSprite Sprite {
            get { return _lineSprite; }
        }

        public bool HitTest(int x, int y){
            throw new NotImplementedException();
        }

        public float Alpha { get; set; }
        public float Depth { get; set; }

        public String Texture {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public int Identifier { get; set; }

        public float X {
            get { return (int)_point1.X; }
            set { throw new NotImplementedException(); }
        }

        public float Y {
            get { return (int)_point1.Y; }
            set { throw new NotImplementedException(); }
        }

        public float Width {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public float Height {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public FloatingRectangle BoundingBox {
            get { throw new NotImplementedException(); }
        }

        public void TranslateOrigin(float dx, float dy) {
            _point1.X += dx;
            _point1.Y += dy;
            CalculateInfoFromPoints();
        }

        public void TranslateDestination(float dx, float dy) {
            _point2.X += dx;
            _point2.Y += dy;
            CalculateInfoFromPoints();
        }

        #endregion

        #region ctor

        public Line(RenderTarget target, Vector2 v1, Vector2 v2, Color color, DepthLevel depth, int identifier = DefaultIdentifier, IUIComponent[] components = null) {
            _lineSprite = new Line2D(this, color);
            _point1 = v1;
            _point2 = v2;
            Depth = (float)depth / 10;
            Alpha = 1;
            LineWidth = 1;
            Identifier = identifier;
            CalculateInfoFromPoints();
            UIElementCollection.AddElement(this);

            Components = components;
            if (Components != null) {
                foreach (IUIComponent component in Components) {
                    component.ComponentCtor(this, null);
                }
            }
        }

        #endregion

        #region private calculation functions

        /// <summary>
        ///   calculates the line's destination point from the line's unit vector and length
        /// </summary>
        void CalculateDestFromUnitVector() {
            _point2.X = UnitVector.X * Length + _point1.X;
            _point2.Y = UnitVector.Y * Length + _point1.Y;
        }

        /// <summary>
        ///   calculates the line's blit location, angle, length, and unit vector based on the origin point and destination point
        /// </summary>
        void CalculateInfoFromPoints() {
            _angle = (float)Math.Atan2(_point2.Y - _point1.Y, _point2.X - _point1.X);
            UnitVector = Common.GetComponentFromAngle(_angle, 1);
            Length = Vector2.Distance(_point1, _point2);
        }

        #endregion

        #region IUIElement Members
        public TComponent GetComponent<TComponent>(string identifier = null) {
            if (Components != null) {
                foreach (IUIComponent component in Components) {
                    if (component is TComponent) {
                        if (identifier != null) {
                            if (component.Identifier == identifier) {
                                return (TComponent)component;
                            }
                            continue;
                        }
                        return (TComponent)component;
                    }
                }
            }
            throw new Exception("Request made to a line object for a component that did not exist.");
        }

        public bool DoesComponentExist<TComponent>(string identifier = null) {
            if (Components != null) {
                foreach (var component in Components) {
                    if (component is TComponent) {
                        if (identifier != null) {
                            if (component.Identifier == identifier) {
                                return true;
                            }
                            continue;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public void UpdateLogic(double timeDelta) {
            if (Components != null) {
                foreach (IUIComponent component in Components) {
                    //component.Update();
                }
            }
        }

        #endregion

        public void Dispose() {
            Sprite.Dispose();
            _lineSprite.Dispose();
        }
    }

    internal class LineGenerator {
        public RenderTarget Target;
        public Color? Color;
        public Dictionary<string, JObject> Components;
        public DepthLevel? Depth;
        public int? Identifier;
        public Vector2? V1;
        public Vector2? V2;

        public LineGenerator() {
            Target = null;
            Components = null;
            Color = null;
            Depth = null;
            V1 = null;
            V2 = null;
            Identifier = null;
        }

        public LineGenerator(string template) {
            var sr = new StreamReader("Templates/" + template);
            string str = sr.ReadToEnd();

            JObject obj = JObject.Parse(str);
            var depthLevelSerializer = new JsonSerializer();
            //depthLevelSerializer.Converters.Add(new DepthLevelConverter());

            var jComponents = obj["Components"];
            if (jComponents != null)
                Components = jComponents.ToObject<Dictionary<string, JObject>>();
            else
                Components = null;

            Depth = obj["Depth"].ToObject<DepthLevel?>(depthLevelSerializer);
            V1 = obj["V1"].ToObject<Vector2>();
            V2 = obj["V2"].ToObject<Vector2>();

            Identifier = obj["Identifier"].Value<int>();
            Color = obj["Color"].ToObject<Color>();
        }

        public Line GenerateLine() {
            //make sure we have all the data required
            if (Target == null ||
                Depth == null ||
                V1 == null ||
                V2 == null ||
                Color == null ||
                Depth == null) {
                throw new Exception("Template did not contain all of the basic variables required to generate a button.");
            }
            //generate component list
            IUIComponent[] components = null;
            if (Components != null) {
                components = GenerateComponents(Components);
            }

            //now we handle optional parameters
            int identifier;
            if (Identifier != null)
                identifier = (int)Identifier;
            else
                identifier = Button.DefaultIdentifier;

            return new Line(
                Target,
                (Vector2)V1,
                (Vector2)V2,
                (Color)Color,
                (DepthLevel)Depth,
                identifier,
                components
                );
        }

        IUIComponent[] GenerateComponents(Dictionary<string, JObject> componentCtorData) {
            var components = new List<IUIComponent>();

            foreach (var data in componentCtorData) {
                string str = data.Key;
                //when there are multiple components, they are named "Componentname_n" where n is the number of the component
                //gotta remove that for the switch, if it exists
                string identifier = "";
                if (str.Contains('_')) {
                    identifier = str.Substring(str.IndexOf('_'), str.Count());
                    str = str.Substring(0, str.IndexOf('_'));
                }

                switch (str) {
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
