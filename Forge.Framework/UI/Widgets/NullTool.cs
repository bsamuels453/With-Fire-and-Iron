#region

using MonoGameUtility;

#endregion

namespace Forge.Framework.UI.Widgets{
    internal class NullTool : IToolbarTool{
        #region IToolbarTool Members

        public void UpdateInput(ref InputState state){
            //throw new NotImplementedException();
        }

        public void UpdateLogic(double timeDelta){
            //throw new NotImplementedException();
        }

        public bool Enabled{
            get { return false; }
            set { }
        }

        public void Dispose(){
        }

        #endregion

        public void Draw(Matrix viewMatrix){
        }
    }
}