#region

using System;
using System.Diagnostics;
using Forge.Framework.Draw;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;

#endregion

namespace Forge.Framework.UI.Elements{
    public class Slider : UIElementCollection{
        #region template defined

        readonly int _endpointWidth;
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
            _endpointWidth = jobj["EndpointWidth"].ToObject<int>();
            _handleWidth = jobj["HandleWidth"].ToObject<int>();
            _handleHeight = jobj["HandleHeight"].ToObject<int>();

            #endregion

            Debug.Assert(Math.Abs((maxVal - minVal)%step - 0) < 0.00001f);

            #region logic

            float sliderRange = _trackWidth - _sliderTrackPadding*2;
            float valueRange = maxVal - minVal;
            _valueMultiplier = sliderRange/valueRange;
            _minVal = minVal;
            _maxVal = maxVal;
            _trackStart = position.X + _horizontalTrackPadding + _sliderTrackPadding + _endpointWidth;

            _stepOffsetsPixel = new int[(int) ((maxVal - minVal)/step)];
            _stepOffsetsValue = new float[_stepOffsetsPixel.Length];
            for (int i = 0; i < _stepOffsetsPixel.Length; i++){
                _stepOffsetsPixel[i] = (int) (_valueMultiplier*step*i);

                _stepOffsetsValue[i] = ConvertToValue(_stepOffsetsPixel[i]);
            }

            #endregion

            #region sprites

            var handleTex = Resource.LoadContent<Texture2D>(_handleMaterial);
            var trackTex = Resource.LoadContent<Texture2D>(_trackMaterial);
            var endTex = Resource.LoadContent<Texture2D>(_trackEndpointMaterial);

            var trackSprite = new Sprite2D
                (
                trackTex,
                position.X + _horizontalTrackPadding + _endpointWidth,
                position.Y + _verticalTrackPadding,
                _trackWidth,
                _trackHeight,
                this.FrameStrata,
                FrameStrata.Level.Low
                );

            var endSprite1 = new Sprite2D
                (
                endTex,
                position.X + _horizontalTrackPadding,
                position.Y + _verticalTrackPadding,
                _endpointWidth,
                _trackHeight,
                this.FrameStrata,
                FrameStrata.Level.Low
                );

            var endSprite2 = new Sprite2D
                (
                endTex,
                position.X + _horizontalTrackPadding + _endpointWidth + _trackWidth,
                position.Y + _verticalTrackPadding,
                _endpointWidth,
                _trackHeight,
                this.FrameStrata,
                FrameStrata.Level.Low
                );
            endSprite2.SpriteEffect = SpriteEffects.FlipHorizontally;

            _sliderSprite = new Sprite2D
                (
                handleTex,
                _trackStart + ConvertToPixelOffset((_minVal + _maxVal)/2f),
                position.Y - _handleVerticalOff + _verticalTrackPadding,
                _handleWidth,
                _handleHeight,
                this.FrameStrata,
                FrameStrata.Level.Medium
                );

            this.AddElement(trackSprite);
            this.AddElement(endSprite1);
            this.AddElement(endSprite2);
            this.AddElement(_sliderSprite);

            #endregion
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