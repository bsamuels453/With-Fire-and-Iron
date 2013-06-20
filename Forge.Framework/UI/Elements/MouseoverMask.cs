using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forge.Framework.Control;
using MonoGameUtility;

namespace Forge.Framework.UI.Elements {
    class MouseoverMask : IUIElement{
        public MouseoverMask(Rectangle boundingBox){
            throw new NotImplementedException();
        }

        public FrameStrata FrameStrata{
            get { throw new NotImplementedException(); }
        }

        public int X{
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public int Y{
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public int Width{
            get { throw new NotImplementedException(); }
        }

        public int Height{
            get { throw new NotImplementedException(); }
        }

        public float Alpha{
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public MouseController MouseController{
            get { throw new NotImplementedException(); }
        }

        public bool HitTest(int x, int y){
            throw new NotImplementedException();
        }

        public List<IUIElement> GetElementStackAtPoint(int x, int y){
            throw new NotImplementedException();
        }
    }
}
