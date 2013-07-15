#region

using System;
using Forge.Framework.Draw;
using Forge.Framework.UI;
using Microsoft.Xna.Framework;
using Rectangle = MonoGameUtility.Rectangle;
using Vector2 = MonoGameUtility.Vector2;

#endregion

namespace Forge.Core.HullEditor{
    public class CurveHandle{
        #region HandleMovementRestriction enum

        public enum HandleMovementRestriction{
            NoRotationOnX,
            NoRotationOnY,
            Vertical,
            Horizontal,
            Quadrant
        }

        #endregion

        const int _handleMinDist = 20;
        readonly HandleButton _centerButton;
        readonly HandleButton _nextButton;
        readonly Line2D _nextLine;
        readonly HandleButton _prevButton;
        readonly Line2D _prevLine;
        public CurveHandle NextHandle;
        public CurveHandle PrevHandle;
        public CurveHandle SymmetricHandle;
        public TranslateDragToExtern TranslateToExtern;
        ClampByNeighbors _clampByNeighbors;
        bool _dontTranslateHandles;
        bool _publicSymmetry;
        int _reflectionX;
        int _reflectionY;
        HandleMovementRestriction _rotRestriction;

        /// <summary>
        /// </summary>
        /// <param name="buttonTemplate"> </param>
        /// <param name="lineTemplate"> </param>
        /// <param name="pos"> </param>
        /// <param name="prevComponent"> </param>
        /// <param name="nextComponent"> </param>
        /// //removed ButtonGenerator buttonTemplate, LineGenerator lineTemplate
        public CurveHandle(Vector2 pos, Vector2 prevComponent, Vector2 nextComponent){
            //buttonTemplate.Identifier = (int) HandleType.Center;
            //buttonTemplate.X = pos.X;
            //buttonTemplate.Y = pos.Y;
            _centerButton = new HandleButton(UIElementCollection.BoundCollection, FrameStrata.Level.Medium, new Rectangle((int) pos.X, (int) pos.Y, 5, 5), "");
            //_centerButton.GetComponent<DraggableComponent>().DragMovementDispatcher += TranslateToLinks;
            //_centerButton.GetComponent<DraggableComponent>().DragMovementClamp += ClampHandleMovement;

            //buttonTemplate.Identifier = (int) HandleType.Prev;
            //buttonTemplate.X = prevComponent.X + pos.X;
            //buttonTemplate.Y = prevComponent.Y + pos.Y;
            _prevButton = new HandleButton
                (UIElementCollection.BoundCollection, FrameStrata.Level.Medium,
                    new Rectangle((int) (prevComponent.X + pos.X), (int) (prevComponent.Y + pos.Y), 5, 5), "");
            //_prevButton.GetComponent<DraggableComponent>().DragMovementDispatcher += TranslateToLinks;
            //_prevButton.GetComponent<DraggableComponent>().DragMovementClamp += ClampHandleMovement;

            //buttonTemplate.Identifier = (int) HandleType.Next;
            //buttonTemplate.X = nextComponent.X + pos.X;
            //buttonTemplate.Y = nextComponent.Y + pos.Y;
            _nextButton = new HandleButton
                (UIElementCollection.BoundCollection, FrameStrata.Level.Medium,
                    new Rectangle((int) (nextComponent.X + pos.X), (int) (nextComponent.Y + pos.Y), 5, 5), "");
            //_nextButton.GetComponent<DraggableComponent>().DragMovementDispatcher += TranslateToLinks;
            //_nextButton.GetComponent<DraggableComponent>().DragMovementClamp += ClampHandleMovement;

            //_prevLine = lineTemplate.GenerateLine();
            //_nextLine = lineTemplate.GenerateLine();
            _prevLine = new Line2D(UIElementCollection.BoundCollection.FrameStrata, FrameStrata.Level.Medium, Vector2.Zero, Vector2.Zero, Color.White);
            _nextLine = new Line2D(UIElementCollection.BoundCollection.FrameStrata, FrameStrata.Level.Medium, Vector2.Zero, Vector2.Zero, Color.White);

            _prevLine.OriginPoint = _centerButton.CentPosition;
            _prevLine.DestPoint = _prevButton.CentPosition;

            _nextLine.OriginPoint = _centerButton.CentPosition;
            _nextLine.DestPoint = _nextButton.CentPosition;
            /*
            _nextButton.GetComponent<FadeComponent>().ForceFadeout();
            _prevButton.GetComponent<FadeComponent>().ForceFadeout();
            _centerButton.GetComponent<FadeComponent>().ForceFadeout();

            _prevLine.GetComponent<FadeComponent>().ForceFadeout();
            _nextLine.GetComponent<FadeComponent>().ForceFadeout();

            _nextButton.GetComponent<FadeComponent>().ForceFadeout();
            _prevButton.GetComponent<FadeComponent>().ForceFadeout();
            _centerButton.GetComponent<FadeComponent>().ForceFadeout();

            _prevLine.GetComponent<FadeComponent>().ForceFadeout();
            _nextLine.GetComponent<FadeComponent>().ForceFadeout();
            
            InterlinkButtonEvents();
             */
        }

        public void Dispose(){
            /*
            _centerButton.Dispose();
            _nextButton.Dispose();
            _nextLine.Dispose();
            _prevButton.Dispose();
            _prevLine.Dispose();
             */

            NextHandle = null;
            PrevHandle = null;
            SymmetricHandle = null;
            TranslateToExtern = null;
        }

        public void SetReflectionType(PanelAlias panelType, HandleMovementRestriction restrictionType, bool haspublicSymmetry = false){
            _publicSymmetry = haspublicSymmetry;
            switch (panelType){
                case PanelAlias.Side:
                    _rotRestriction = restrictionType; //vert
                    _reflectionX = 0;
                    _reflectionY = 1;
                    _dontTranslateHandles = true;
                    //_clampByNeighbors = SideNeighborClamp;
                    break;
                case PanelAlias.Top:
                    _rotRestriction = restrictionType; //vert
                    _reflectionX = 1;
                    _reflectionY = -1;
                    _dontTranslateHandles = false;
                    //_clampByNeighbors = TopNeighborClamp;
                    if (haspublicSymmetry){
                        _reflectionX = 0;
                    }
                    break;
                case PanelAlias.Back:
                    _rotRestriction = restrictionType; //vert
                    _reflectionX = -1;
                    _reflectionY = 1;
                    _dontTranslateHandles = false;
                    //_clampByNeighbors = BackNeighborClamp;
                    if (haspublicSymmetry){
                        _reflectionY = 0;
                    }
                    break;
            }
        }

        public void TranslatePosition(float dx, float dy){
            BalancedCenterTranslate((int) dx, (int) dy);
        }

        public void ClampPositionFromExternal(float dx, float dy){
        }

        /*
        void ClampHandleMovement(IUIInteractiveElement owner, ref int x, ref int y, int oldX, int oldY){
            var button = (Button) owner;
            float dx = x - oldX;
            float dy = y - oldY;
            publicMovementClamp(ref dx, ref dy, button);
            x = (int) dx + oldX;
            y = (int) dy + oldY;
        }
         */
        /*
        void publicMovementClamp(ref float dx, ref float dy, Button button){
            #region region clamp

            if (_rotRestriction == HandleMovementRestriction.Vertical || _rotRestriction == HandleMovementRestriction.Quadrant){
                if ((HandleType) button.Identifier != HandleType.Center){
                    Button handle = (HandleType) button.Identifier == HandleType.Prev ? _prevButton : _nextButton;
                    bool isHandleOnLeftSide = handle.CentPosition.X < _centerButton.CentPosition.X;
                    if (isHandleOnLeftSide){
                        if (handle.CentPosition.X + dx >= _centerButton.CentPosition.X){
                            dx = _centerButton.X - handle.X - 1;
                        }
                    }
                    else{
                        if (handle.CentPosition.X + dx <= _centerButton.CentPosition.X){
                            dx = _centerButton.X - handle.X + 1;
                        }
                    }
                }
            }
            if (_rotRestriction == HandleMovementRestriction.Horizontal || _rotRestriction == HandleMovementRestriction.Quadrant){
                if ((HandleType) button.Identifier != HandleType.Center){
                    Button handle = (HandleType) button.Identifier == HandleType.Prev ? _prevButton : _nextButton;
                    bool isHandleOnLeftSide = handle.CentPosition.Y < _centerButton.CentPosition.Y;
                    if (isHandleOnLeftSide){
                        if (handle.CentPosition.Y + dy >= _centerButton.CentPosition.Y){
                            dy = _centerButton.Y - handle.Y - 1;
                        }
                    }
                    else{
                        if (handle.CentPosition.Y + dy <= _centerButton.CentPosition.Y){
                            dy = _centerButton.Y - handle.Y + 1;
                        }
                    }
                }
            }
            if (_rotRestriction == HandleMovementRestriction.NoRotationOnX){
                if (button == _centerButton){
                    dy = 0;
                }
                else{
                    dx = 0;
                    //this next part prevents handles from "crossing over" the center to the other side
                    if (button == _prevButton){
                        if (button.Y + dy >= _centerButton.Y - 9){
                            dy = _centerButton.X - button.Y - 10;
                        }
                    }
                    else{
                        if (button.Y + dy <= _centerButton.Y + 9){
                            dy = _centerButton.Y - button.Y + 10;
                        }
                    }
                }
            }
            if (_rotRestriction == HandleMovementRestriction.NoRotationOnY){
                if (button == _centerButton){
                    dx = 0;
                }
                else{
                    dy = 0;
                    //this next part prevents handles from "crossing over" the center to the other side
                    if (button == _prevButton){
                        if (button.X + dx >= _centerButton.X - 9){
                            dx = _centerButton.X - button.X - 10;
                        }
                    }
                    else{
                        if (button.X + dx <= _centerButton.X + 9){
                            dx = _centerButton.X - button.X + 10;
                        }
                    }
                }
            }

            #endregion

            #region distance clamp

            if (button == _prevButton){
                if (Common.GetDist((button.CentPosition.X + dx), (button.CentPosition.Y + dy), _centerButton.CentPosition.X, _centerButton.CentPosition.Y) <
                    _handleMinDist){
                    Vector2 tempDest = _prevLine.DestPoint - _prevLine.OriginPoint;
                    tempDest.X += dx;
                    tempDest.Y += dy;
                    tempDest.Normalize();
                    tempDest *= _handleMinDist;
                    tempDest += _prevLine.OriginPoint;

                    dx = tempDest.X - _prevLine.DestPoint.X;
                    dy = tempDest.Y - _prevLine.DestPoint.Y;
                }
            }
            if (button == _nextButton){
                if (Common.GetDist((button.CentPosition.X + dx), (button.CentPosition.Y + dy), _centerButton.CentPosition.X, _centerButton.CentPosition.Y) <
                    _handleMinDist){
                    Vector2 tempDest = _nextLine.DestPoint - _nextLine.OriginPoint;
                    tempDest.X += dx;
                    tempDest.Y += dy;
                    tempDest.Normalize();
                    tempDest *= _handleMinDist;
                    tempDest += _nextLine.OriginPoint;

                    dx = tempDest.X - _nextLine.DestPoint.X;
                    dy = tempDest.Y - _nextLine.DestPoint.Y;
                }
            }

            #endregion

            _clampByNeighbors(ref dx, ref dy, button);
        }
         */

        /*
        void BackNeighborClamp(ref float dx, ref float dy, Button button){
            //prevent symmetric buttons from crossing each other
            if (PrevHandle != null && NextHandle == null){
                if (button.CentPosition.X + dx < PrevHandle.CentButtonCenter.X){
                    dx = PrevHandle.CentButtonCenter.X - button.CentPosition.X;
                }
            }
            if (PrevHandle == null && NextHandle != null){
                if (button.CentPosition.X + dx > NextHandle.CentButtonCenter.X){
                    dx = NextHandle.CentButtonCenter.X - button.CentPosition.X;
                }
            }

            //prevent buttons from crossing the middle handle's center position
            //also prevents center handle from crossing the two bounding handle's satellite buttons
            CurveHandle handleToUse = NextHandle ?? PrevHandle;
            switch ((HandleType) button.Identifier){
                case HandleType.Prev:
                    if (NextHandle == null || PrevHandle == null){ //assume  this is a bounding handle
                        if (button.CentPosition.Y + dy > handleToUse.PrevButtonCenter.Y){
                            dy = handleToUse.PrevButtonCenter.Y - _prevButton.CentPosition.Y;
                        }
                    }

                    break;
                case HandleType.Center:
                    if (NextHandle != null && PrevHandle != null){ //assume this is the center curve handle
                        if (button.CentPosition.Y + dy < NextHandle.PrevButtonCenter.Y){
                            dy = NextHandle.PrevButtonCenter.Y - _centerButton.CentPosition.Y;
                        }
                    }
                    break;
                case HandleType.Next:
                    if (NextHandle == null || PrevHandle == null){ //assume  this is a bounding handle
                        if (button.CentPosition.Y + dy > handleToUse.PrevButtonCenter.Y){
                            dy = handleToUse.PrevButtonCenter.Y - _nextButton.CentPosition.Y;
                        }
                    }
                    break;
            }

            //prevents bounding handles from crossing center handle's satellite buttons (x+y direction)
            switch ((HandleType) button.Identifier){
                case HandleType.Center:
                    if (NextHandle == null && PrevHandle != null){ //assume this is a next bounding handle
                        if (button.CentPosition.X + dx < PrevHandle.NextButtonCenter.X){
                            dx = PrevHandle.NextButtonCenter.X - button.CentPosition.X;
                        }
                        if (_prevButton.CentPosition.Y + dy > PrevHandle.NextButtonCenter.Y){
                            dy = PrevHandle.NextButtonCenter.Y - _prevButton.CentPosition.Y;
                        }
                    }
                    if (NextHandle != null && PrevHandle == null){ //assume this is a prev bounding handle
                        if (button.CentPosition.X + dx > NextHandle.PrevButtonCenter.X){
                            dx = NextHandle.PrevButtonCenter.X - button.CentPosition.X;
                        }
                        if (_nextButton.CentPosition.Y + dy > NextHandle.NextButtonCenter.Y){
                            dy = NextHandle.NextButtonCenter.Y - _nextButton.CentPosition.Y;
                        }
                    }
                    break;
            }

            //prevents the center button's satellite buttons from crossing the bounding handle centers
            if (NextHandle != null && PrevHandle != null){
                if ((HandleType) button.Identifier == HandleType.Prev){
                    if (button.CentPosition.X + dx < PrevHandle.CentButtonCenter.X){
                        dx = PrevHandle.CentButtonCenter.X - button.CentPosition.X;
                    }
                }
                if ((HandleType) button.Identifier == HandleType.Next){
                    if (button.CentPosition.X + dx > NextHandle.CentButtonCenter.X){
                        dx = NextHandle.CentButtonCenter.X - button.CentPosition.X;
                    }
                }
            }
        }

        void TopNeighborClamp(ref float dx, ref float dy, Button button){
        }
         */

        /*
        void SideNeighborClamp(ref float dx, ref float dy, Button button){
            //prevent symmetric buttons from crossing each other
            if (PrevHandle != null && NextHandle == null){
                if (button.CentPosition.X + dx < PrevHandle.CentButtonCenter.X){
                    dx = PrevHandle.CentButtonCenter.X - button.CentPosition.X;
                }
            }
            if (PrevHandle == null && NextHandle != null){
                if (button.CentPosition.X + dx > NextHandle.CentButtonCenter.X){
                    dx = NextHandle.CentButtonCenter.X - button.CentPosition.X;
                }
            }

            //prevent buttons from crossing the middle handle's center position
            //also prevents center handle from crossing the two bounding handle's satellite buttons
            CurveHandle handleToUse = NextHandle ?? PrevHandle;
            switch ((HandleType) button.Identifier){
                case HandleType.Prev:
                    if (NextHandle == null || PrevHandle == null){ //assume  this is a bounding handle
                        if (button.CentPosition.Y + dy > handleToUse.PrevButtonCenter.Y){
                            dy = handleToUse.PrevButtonCenter.Y - _prevButton.CentPosition.Y;
                        }
                    }

                    break;
                case HandleType.Center:
                    if (NextHandle != null && PrevHandle != null){ //assume this is the center curve handle
                        if (button.CentPosition.Y + dy < NextHandle.PrevButtonCenter.Y){
                            dy = NextHandle.PrevButtonCenter.Y - _centerButton.CentPosition.Y;
                        }
                    }
                    break;
                case HandleType.Next:
                    if (NextHandle == null || PrevHandle == null){ //assume  this is a bounding handle
                        if (button.CentPosition.Y + dy > handleToUse.PrevButtonCenter.Y){
                            dy = handleToUse.PrevButtonCenter.Y - _nextButton.CentPosition.Y;
                        }
                    }
                    break;
            }

            //prevents bounding handles from crossing center handle's satellite buttons (x+y direction)
            switch ((HandleType) button.Identifier){
                case HandleType.Center:
                    if (NextHandle == null && PrevHandle != null){ //assume this is a next bounding handle
                        if (button.CentPosition.X + dx < PrevHandle.NextButtonCenter.X){
                            dx = PrevHandle.NextButtonCenter.X - button.CentPosition.X;
                        }
                        if (_prevButton.CentPosition.Y + dy > PrevHandle.NextButtonCenter.Y){
                            dy = PrevHandle.NextButtonCenter.Y - _prevButton.CentPosition.Y;
                        }
                    }
                    if (NextHandle != null && PrevHandle == null){ //assume this is a prev bounding handle
                        if (button.CentPosition.X + dx > NextHandle.PrevButtonCenter.X){
                            dx = NextHandle.PrevButtonCenter.X - button.CentPosition.X;
                        }
                        if (_nextButton.CentPosition.Y + dy > NextHandle.NextButtonCenter.Y){
                            dy = NextHandle.NextButtonCenter.Y - _nextButton.CentPosition.Y;
                        }
                    }
                    break;
            }

            //prevents the center button's satellite buttons from crossing the bounding handle centers
            if (NextHandle != null && PrevHandle != null){
                if ((HandleType) button.Identifier == HandleType.Prev){
                    if (button.CentPosition.X + dx < PrevHandle.CentButtonCenter.X){
                        dx = PrevHandle.CentButtonCenter.X - button.CentPosition.X;
                    }
                }
                if ((HandleType) button.Identifier == HandleType.Next){
                    if (button.CentPosition.X + dx > NextHandle.CentButtonCenter.X){
                        dx = NextHandle.CentButtonCenter.X - button.CentPosition.X;
                    }
                }
            }
        }
         */

        void BalancedCenterTranslate(int dx, int dy){
            _centerButton.X += dx;
            _centerButton.Y += dy;
            _prevButton.X += dx;
            _prevButton.Y += dy;

            _nextButton.X += dx;
            _nextButton.Y += dy;

            _prevLine.TranslateDestination(dx, dy);
            _prevLine.TranslateOrigin(dx, dy);
            _nextLine.TranslateDestination(dx, dy);
            _nextLine.TranslateOrigin(dx, dy);
        }

        void RawPrevTranslate(int dx, int dy){
            _prevButton.X += dx;
            _prevButton.Y += dy;
            _prevLine.TranslateDestination(dx, dy);
        }

        void BalancedPrevTranslate(int dx, int dy){
            RawPrevTranslate(dx, dy);
            _nextLine.Angle = (float) (_prevLine.Angle + Math.PI);

            _nextButton.X = (int) (_nextLine.DestPoint.X - _nextButton.Width/2f);
            _nextButton.Y = (int) (_nextLine.DestPoint.Y - _nextButton.Height/2f);
        }

        void RawNextTranslate(int dx, int dy){
            _nextButton.X += dx;
            _nextButton.Y += dy;
            _nextLine.TranslateDestination(dx, dy);
        }

        void BalancedNextTranslate(int dx, int dy){
            RawNextTranslate(dx, dy);
            _prevLine.Angle = (float) (_nextLine.Angle + Math.PI);

            _prevButton.X = (int) (_prevLine.DestPoint.X - _prevButton.Width/2f);
            _prevButton.Y = (int) (_prevLine.DestPoint.Y - _prevButton.Height/2f);
        }

        #region properties

        public Vector2 CentButtonCenter{
            get { return _centerButton.CentPosition; }
        }

        public Vector2 PrevButtonCenter{
            get { return _prevButton.CentPosition; }
        }

        public Vector2 NextButtonCenter{
            get { return _nextButton.CentPosition; }
        }

        public Vector2 CentButtonPos{
            get { return _centerButton.CentPosition; }
            set { _centerButton.CentPosition = value; }
        }

        public Vector2 NextButtonPos{
            get { return _nextButton.CentPosition; }
        }

        public Vector2 PrevButtonPos{
            get { return _prevButton.CentPosition; }
        }

        public float PrevLength{
            get { return _prevLine.Length; }
        }

        public float NextLength{
            get { return _nextLine.Length; }
        }

        public float Angle{
            set{
                _prevLine.Angle = value;
                _nextLine.Angle = (float) (value + Math.PI);
                _prevButton.CentPosition = _prevLine.DestPoint;
                _nextButton.CentPosition = _nextLine.DestPoint;
            }
            get { return _prevLine.Angle; }
        }

        #endregion

        /*
        void TranslateToLinks(object caller, int dx, int dy){
            var obj = (Button) caller;
            switch ((HandleType) obj.Identifier){
                case HandleType.Center:
                    _prevButton.X += dx;
                    _prevButton.Y += dy;

                    _nextButton.X += dx;
                    _nextButton.Y += dy;

                    _prevLine.TranslateDestination(dx, dy);
                    _prevLine.TranslateOrigin(dx, dy);
                    _nextLine.TranslateDestination(dx, dy);
                    _nextLine.TranslateOrigin(dx, dy);
                    if (SymmetricHandle != null){
                        SymmetricHandle.BalancedCenterTranslate(_reflectionX*dx, _reflectionY*dy);
                    }
                    if (TranslateToExtern != null){
                        float dxf = dx;
                        float dyf = dy;

                        TranslateToExtern(this, ref dxf, ref dyf, false);
                    }
                    break;
                case HandleType.Prev:
                    _prevLine.TranslateDestination(dx, dy);
                    if (_publicSymmetry){
                        RawNextTranslate(dx*_reflectionX, dy*_reflectionY);
                    }
                    else{
                        _nextLine.Angle = (float) (_prevLine.Angle + Math.PI);

                        _nextButton.X = _nextLine.DestPoint.X - _nextButton.Width/2;
                        _nextButton.Y = _nextLine.DestPoint.Y - _nextButton.Height/2;
                    }

                    if (SymmetricHandle != null && !_dontTranslateHandles){
                        SymmetricHandle.BalancedNextTranslate(_reflectionX*dx, _reflectionY*dy);
                    }

                    break;
                case HandleType.Next:
                    _nextLine.TranslateDestination(dx, dy);
                    if (_publicSymmetry){
                        RawPrevTranslate(dx*_reflectionX, dy*_reflectionY);
                    }
                    else{
                        _prevLine.Angle = (float) (_nextLine.Angle - Math.PI);

                        _prevButton.X = _prevLine.DestPoint.X - _prevButton.Width/2;
                        _prevButton.Y = _prevLine.DestPoint.Y - _prevButton.Height/2;
                    }

                    if (SymmetricHandle != null && !_dontTranslateHandles){
                        SymmetricHandle.BalancedPrevTranslate(_reflectionX*dx, _reflectionY*dy);
                    }

                    break;
            }
        }
         */
        /*
        void InterlinkButtonEvents(){
            FadeComponent.LinkFadeComponentTriggers(_prevButton, _nextButton, FadeComponent.FadeTrigger.EntryExit);
            FadeComponent.LinkFadeComponentTriggers(_prevButton, _centerButton, FadeComponent.FadeTrigger.EntryExit);
            FadeComponent.LinkFadeComponentTriggers(_nextButton, _centerButton, FadeComponent.FadeTrigger.EntryExit);


            FadeComponent.LinkOnewayFadeComponentTriggers
                (
                    eventProcElements: new IUIElement[]{
                        _prevButton,
                        _nextButton,
                        _centerButton
                    },
                    eventRecieveElements: new IUIElement[]{
                        _prevLine,
                        _nextLine
                    },
                    state: FadeComponent.FadeTrigger.EntryExit
                );
        }
         */

        #region Nested type: ClampByNeighbors

        delegate void ClampByNeighbors(ref float dx, ref float dy, HandleButton button);

        #endregion

        #region Nested type: HandleType

        enum HandleType{
            Center,
            Prev,
            Next
        }

        #endregion
    }
}