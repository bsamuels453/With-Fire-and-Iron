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
        readonly int _trackWidth;
        readonly int _verticalTrackPadding;

        #endregion

        readonly DraggableSprite _handle;
        readonly int _maxTrackPixel;
        readonly float _maxVal;
        readonly int _minTrackPixel;
        readonly float _minVal;
        readonly int[] _stepOffsetsPixel;
        readonly float[] _stepOffsetsValue;

        readonly float _stepValueLength;

        /// <summary>
        /// multiplying by this value convers from value scale to pixel scale
        /// </summary>
        readonly float _valueMultiplier;

        readonly int _yPosition;

        float _handleValue;

        public Slider(
            UIElementCollection parent,
            FrameStrata.Level depth,
            Point position,
            float minVal,
            float maxVal,
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
            Debug.Assert(minVal < maxVal);

            var handleTex = Resource.LoadContent<Texture2D>(_handleMaterial);
            var trackTex = Resource.LoadContent<Texture2D>(_trackMaterial);
            var endTex = Resource.LoadContent<Texture2D>(_trackEndpointMaterial);

            #region logic

            float sliderRange = _trackWidth - _sliderTrackPadding*2;
            float valueRange = maxVal - minVal;
            _valueMultiplier = sliderRange/valueRange;
            _minVal = minVal;
            _maxVal = maxVal;
            _stepValueLength = step;
            _minTrackPixel = position.X + _horizontalTrackPadding + _sliderTrackPadding;
            _maxTrackPixel = _minTrackPixel + _trackWidth - _sliderTrackPadding*2;
            _yPosition = position.Y - _handleVerticalOff + _verticalTrackPadding;

            _stepOffsetsPixel = new int[(int) ((maxVal - minVal)/step) + 1];
            _stepOffsetsValue = new float[_stepOffsetsPixel.Length];
            for (int i = 0; i < _stepOffsetsPixel.Length; i++){
                _stepOffsetsPixel[i] = (int) (_valueMultiplier*step*i);

                _stepOffsetsValue[i] = step*i + _minVal;
            }

            #endregion

            float defaultHandleValue = (_minVal + _maxVal)/2f;

            this.BoundingBox = new Rectangle
                (
                position.X,
                position.Y,
                _trackWidth + _horizontalTrackPadding*2,
                _trackHeight + _verticalTrackPadding*2
                );

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

            _handle = new DraggableSprite
                (
                this,
                FrameStrata.Level.Medium,
                new Rectangle
                    (
                    0,
                    _yPosition,
                    _handleWidth,
                    _handleHeight
                    ),
                handleTex
                );


            this.AddElement(bgSprite);
            this.AddElement(_handle);

            #endregion

            _handle.ConstrainDrag =
                (collection, newpoint, oldpoint) =>{
                    newpoint.Y = _yPosition;
                    newpoint.X = SetHandlePosUsingPix(newpoint.X);
                    return newpoint;
                };
            HandleValue = defaultHandleValue;
        }

        public float HandleValue{
            get { return _handleValue; }
            set{
                Debug.Assert(value <= _maxVal && value >= _minVal);
                SetHandlePosUsingValue(value);
            }
        }

        int SetHandlePosUsingPix(int pixel){
            //convert pixel to value
            pixel -= _minTrackPixel;
            float value = pixel*(1/_valueMultiplier);
            value += _minVal;
            return SetHandlePosUsingValue(value);
        }

        int SetHandlePosUsingValue(float value){
            //clamp to step
            if (value > _maxVal){
                value = _maxVal;
            }
            if (value < _minVal){
                value = _minVal;
            }

            int clampedValueIdx = -1;
            for (int i = 0; i < _stepOffsetsValue.Length - 1; i++){
                if (value >= _stepOffsetsValue[i] && value <= _stepOffsetsValue[i + 1]){
                    float d1 = Math.Abs(value - _stepOffsetsValue[i]);
                    float d2 = Math.Abs(value - _stepOffsetsValue[i + 1]);
                    clampedValueIdx = d1 < d2 ? i : i + 1;
                    break;
                }
            }
            // ReSharper disable CompareOfFloatsByEqualityOperator
            Debug.Assert(clampedValueIdx != -1);
            // ReSharper restore CompareOfFloatsByEqualityOperator
            int pixPos = _stepOffsetsPixel[clampedValueIdx];
            pixPos -= _handleWidth/2; //center handle on value

            pixPos += _minTrackPixel;

            _handleValue = value;
            _handle.X = pixPos;

            if (OnSliderValueChange != null){
                OnSliderValueChange.Invoke(value);
            }

            return pixPos;
        }

        /// <summary>
        /// args: (float newValue)
        /// </summary>
        public event Action<float> OnSliderValueChange;
    }
}