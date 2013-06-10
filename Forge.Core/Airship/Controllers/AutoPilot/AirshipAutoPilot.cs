namespace Forge.Core.Airship.Controllers.AutoPilot{
    public abstract class AirshipAutoPilot{
        public abstract Pathfinder.RetAttributes CalculateNextPosition(double timeDelta);
    }
}