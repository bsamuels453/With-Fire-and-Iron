namespace Forge.Framework.UI{
    public interface IUIElement : ILogicUpdates{
        float X { get; set; }
        float Y { get; set; }
        float Width { get; set; }
        float Height { get; set; }
        float Alpha { get; set; }
        float Depth { get; set; }
        string Texture { get; set; }
        int Identifier { get; }
        bool HitTest(int x, int y);

        TComponent GetComponent<TComponent>(string identifier = null);
        bool DoesComponentExist<TComponent>(string identifier = null);
    }
}