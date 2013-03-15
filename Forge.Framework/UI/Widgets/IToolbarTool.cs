namespace Forge.Framework.UI.Widgets{
    public interface IToolbarTool : IInputUpdates, ILogicUpdates{
        bool Enabled { get; set; }
    }
}