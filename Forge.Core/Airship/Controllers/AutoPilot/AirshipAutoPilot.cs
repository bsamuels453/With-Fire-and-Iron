namespace Forge.Core.Airship.Controllers.AutoPilot{
    internal abstract class AirshipAutoPilot{
        public abstract Pathfinder.RetAttributes CalculateNextPosition(double timeDelta);
    }
}