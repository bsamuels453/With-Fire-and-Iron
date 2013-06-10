namespace Forge.Core{
#if WINDOWS || XBOX
    public static class Program{
        //del /s /q $(SolutionDir)\bin\Raw
        //md $(SolutionDir)\bin\Raw
        static void Main(string[] args){
            using (Forge game = new Forge()){
                game.Run();
            }
        }
    }
#endif
}