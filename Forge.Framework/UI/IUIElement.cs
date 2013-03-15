namespace Forge.Framework.UI{
    public interface IUIElement : IUIElementBase{
        string Texture { get; set; }
        int Identifier { get; }

        TComponent GetComponent<TComponent>(string identifier = null);
        bool DoesComponentExist<TComponent>(string identifier = null);
    }
}