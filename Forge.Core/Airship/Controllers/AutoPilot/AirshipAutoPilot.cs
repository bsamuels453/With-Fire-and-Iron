namespace Forge.Core.Airship.Controllers.AutoPilot{
    internal abstract class AirshipAutoPilot{
        protected readonly AirshipController Controller;
        protected readonly AirshipIndexer Indexer;

        protected AirshipAutoPilot(AirshipController controller, AirshipIndexer airships){
            Controller = controller;
            Indexer = airships;
        }

        public void Update(double timeDelta){
            UpdateChild(timeDelta);
        }

        protected abstract void UpdateChild(double timeDelta);
    }
}