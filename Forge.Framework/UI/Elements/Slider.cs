#region

using System;
using System.Diagnostics;
using Forge.Framework.Draw;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Point = MonoGameUtility.Point;
using Rectangle = MonoGameUtility.Rectangle;

#endregion

namespace Forge.Framework.UI.Elements{
    public class Slider : UIElementCollection{
        #region template defined

        readonly int _handleHeight;
        readonly string _handleMaterial;
        readonly int _handleVerticalOff;
        readonly int _handleWidth;
        readonly int _horizontalTrackPadding;
        readonly int _sliderTrackPadding;
        readonly string _trackEndpointMaterial;
        readonly int _trackHeight;
        readonly string _trackMaterial;
        readonly int _trackStart;
        readonly int _trackWidth;

        #endregion

        readonly float _maxVal;
        readonly float _minVal;
        readonly Sprite2D _sliderSprite;
        readonly int[] _stepOffsetsPixel;
        readonly float[] _stepOffsetsValue;

        /// <summary>
        /// multiplying by this value convers from value scale to pixel scale
        /// </summary>
        readonly float _valueMultiplier;

        readonly int _verticalTrackPadding;

        float _handleValue;

        public Slider(
            UIElementCollection parent,
            FrameStrata.Level depth,
            Point position,
            float maxVal,
            float minVal,
            float step,
            string template = "UiTemplates/Slider.json") :
                base(parent, depth, new Rectangle(), "Slider"){
            #region load template

            var jobj = Resource.LoadJObject(template);

            _handleMaterial = jobj["HandleMaterial"].ToObject<string>();
            _trackMaterial = jobj["TrackMaterial"].ToObject<string>();
            _trackEndpointMaterial = jobj["TrackEndpointMaterial"].ToObject<string>();
            _trackWidth = jobj["TrackWidth"].ToObject<int>();
            _horizontalTrackPadding = jobj["HorizontalTrackPadding"].ToObject<int>();
            _verticalTrackPadding = jobj["VerticalTrackPadding"].ToObject<int>();
            _sliderTrackPadding = jobj["SliderTrackPadding"].ToObject<int>();
            _handleVerticalOff = jobj["HandleVerticalOffset"].ToObject<int>();
            _trackHeight = jobj["TrackHeight"].ToObject<int>();
            _handleWidth = jobj["HandleWidth"].ToObject<int>();
            _handleHeight = jobj["HandleHeight"].ToObject<int>();

            #endregion

            Debug.Assert(Math.Abs((maxVal - minVal)%step - 0) < 0.00001f);

            var handleTex = Resource.LoadContent<Texture2D>(_handleMaterial);
            var trackTex = Resource.LoadContent<Texture2D>(_trackMaterial);
            var endTex = Resource.LoadContent<Texture2D>(_trackEndpointMaterial);

            #region logic

            float sliderRange = _trackWidth - _sliderTrackPadding*2;
            float valueRange = maxVal - minVal;
            _valueMultiplier = sliderRange/valueRange;
            _minVal = minVal;
            _maxVal = maxVal;
            _trackStart = position.X + _horizontalTrackPadding + _sliderTrackPadding;

            _stepOffsetsPixel = new int[(int) ((maxVal - minVal)/step)];
            _stepOffsetsValue = new float[_stepOffsetsPixel.Length];
            for (int i = 0; i < _stepOffsetsPixel.Length; i++){
                _stepOffsetsPixel[i] = (int) (_valueMultiplier*step*i);

                _stepOffsetsValue[i] = ConvertToValue(_stepOffsetsPixel[i]);
            }

            #endregion

            float defaultHandleValue = (_minVal + _maxVal)/2f;

            #region sprites

            var sb = new SpriteBatch(Resource.Device);
            var target = new RenderTarget2D(Resource.Device, _trackWidth, trackTex.Height);
            Resource.Device.SetRenderTarget(target);
            Resource.Device.Clear(Color.Transparent);

            sb.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
            sb.Draw
                (
                    trackTex,
                    new Rectangle(endTex.Width, 0, _trackWidth - endTex.Width*2, trackTex.Height),
                    null,
                    Color.White);
            sb.Draw
                (
                    endTex,
                    Vector2.Zero,
                    null,
                    Color.White);
            sb.Draw
                (
                    endTex,
                    new Rectangle(_trackWidth - endTex.Width, 0, endTex.Width, endTex.Height),
                    null,
                    Color.White,
                    0,
                    Vector2.Zero,
                    SpriteEffects.FlipHorizontally, 0);
            sb.End();

            //apply trackheight scaling
            var finalBG = new RenderTarget2D(Resource.Device, target.Width, _trackHeight);
            Resource.Device.SetRenderTarget(finalBG);
            Resource.Device.Clear(Color.Transparent);

            sb.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
            sb.Draw(target, new Rectangle(0, 0, finalBG.Width, finalBG.Height), null, Color.White);
            sb.End();

            Resource.Device.SetRenderTarget(null);

            var bgSprite = new Sprite2D
                (
                finalBG,
                position.X + _horizontalTrackPadding,
                position.Y + _verticalTrackPadding,
                finalBG.Width,
                finalBG.Height,
                this.FrameStrata,
                FrameStrata.Level.Background
                );

            _sliderSprite = new Sprite2D
                (
                handleTex,
                _trackStart + ConvertToPixelOffset(defaultHandleValue) - _handleWidth/2,
                position.Y - _handleVerticalOff + _verticalTrackPadding,
                _handleWidth,
                _handleHeight,
                this.FrameStrata,
                FrameStrata.Level.Medium
                );

            this.AddElement(bgSprite);
            this.AddElement(_sliderSprite);

            #endregion

            HandleValue = defaultHandleValue;
        }

        public float HandleValue{
            get { return _handleValue; }
            set{
                Debug.Assert(value <= _maxVal && value >= _minVal);
                int offset = ConvertToPixelOffset(value);
                _sliderSprite.X = offset + _trackStart;
                _handleValue = value;
            }
        }

        float ConvertToValue(int stepIndex){
            return stepIndex*(1/_valueMultiplier) + _minVal;
        }

        int ConvertToPixelOffset(float value){
            int ret = (int) ((value - _minVal)*_valueMultiplier);
            return ret;
        }
    }
}