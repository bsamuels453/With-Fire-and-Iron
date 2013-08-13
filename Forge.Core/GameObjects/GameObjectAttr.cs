namespace Forge.Core.GameObjects{
    //enums marked with req are required fields for editor game objects
    public enum GameObjectAttr{
        Dimensions,
        ModelName,
        Icon,
        Name,

        IsInteractable, //req
        IsMultifloorInteractable, //req
        InteractionArea,
        InteractionOrientation,

        SideEffect, //req
        HasMultifloorAABB, //req
        CeilingCutArea,

        ProjectileEmitterOffset,
        FiringForce,
        Mass,
        Radius,
    }
}