namespace Forge.Core.Airship.Data{
    /// <summary>
    ///   Defines the different kinds of maneuvers the autopilot can perform.
    /// </summary>
    public enum ManeuverTypeEnum{
        None,
        Orbit,
        Approach,
        Follow
    }

    /// <summary>
    ///   Defines what kind of input device will control the airship.
    /// </summary>
    public enum AirshipControllerType{
        Player,
        AI,
        None
    }
}