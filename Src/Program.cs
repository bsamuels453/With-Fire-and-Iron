namespace Gondola{
#if WINDOWS || XBOX
    internal static class Program{
        //del /s /q $(SolutionDir)\bin\Raw
        //md $(SolutionDir)\bin\Raw
        static void Main(string[] args){
            using (Gondola game = new Gondola()){
                game.Run();
            }
        }
    }
#endif
}