using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gondola.Logic {
    internal interface ILogicUpdates {
        void UpdateLogic(double timeDelta);
    }

    internal interface IInputUpdates {
        void UpdateInput(ref InputState state);
    }
}
