namespace Forge.Framework{
    public interface ILogicUpdates{
        void UpdateLogic(double timeDelta);
    }

    public interface IInputUpdates{
        void UpdateInput(ref InputState state);
    }
}