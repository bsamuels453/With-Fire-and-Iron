using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gondola.Logic;
using Microsoft.Xna.Framework;

namespace Gondola.UI.Widgets {
    internal class NullTool : IToolbarTool {
        #region IToolbarTool Members

        public void UpdateInput(ref InputState state) {
            //throw new NotImplementedException();
        }

        public void UpdateLogic(double timeDelta) {
            //throw new NotImplementedException();
        }


        #endregion

        public bool Enabled {
            get { return false; }
            set { }
        }

        public void Draw(Matrix viewMatrix) {

        }
    }
}
