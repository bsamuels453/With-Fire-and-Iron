#region

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAGameConsole;

#endregion

namespace Forge.Framework{
    public static class DebugConsole{
        static SpriteBatch _target;
        static GameConsole _console;

        public static void InitalizeConsole(Game game){
            _target = new SpriteBatch(Gbl.Device);

            var options = new GameConsoleOptions();
            options.OpenOnWrite = true;
            _console = new GameConsole(game, _target, new GameConsoleOptions{
                Font = Gbl.ContentManager.Load<SpriteFont>("Fonts/SpriteFont"),
                FontColor = Color.LawnGreen,
                Prompt = ">",
                PromptColor = new Color(0, 0, 0, 0),
                CursorColor = new Color(0, 0, 0, 0),
                BackgroundColor = new Color(0, 0, 0, 70), //Color.BLACK with transparency
                PastCommandOutputColor = Color.White,
                BufferColor = new Color(0, 0, 0, 0)
            });
            _console.Options.Padding = 1;
            _console.Options.Margin = 200;
            _console.Options.ToggleKey = Keys.OemTilde;
            _console.Options.Height = 80;
        }

        public static void WriteLine(string s){
            _console.WriteLine(s);
        }
    }
}