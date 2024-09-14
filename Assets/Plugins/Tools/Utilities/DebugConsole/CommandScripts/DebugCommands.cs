//Created by: Ryan King

/// <summary>
/// All methods in this class will be accessible as commands. They should return a string value to be output to the console.
/// The parameters will be received from the what is typed into the input field. They are converted in the DebugConsole class but not 
/// sure if this works with non-primitive types.
/// General commands that can be used in any app can go here. Game specific commands should be in a separate partial class.
/// </summary>
namespace Skritty.Tools.Utilities.Commands
{
    public static partial class DebugCommands
    {
        private static bool logToggle;

        /// <summary>
        /// Example: Tests printing to debug console.
        /// </summary>
        /// <returns>Response to hello world</returns>
        public static string HelloWorld()
        {
            ConsoleLog.Log("Hello World!!!", true);
            return ".... Why hello there!";
        }

        /// <summary>
        /// Will toggle whether logs from ConsoleLog will also be displayed in the debug console.
        /// </summary>
        /// <param name="overrideConsole">Whether to override toConsole parameter option in ConsoleLog.</param>
        /// <returns></returns>
        public static string ToggleLog(bool overrideConsole = false)
        {
            logToggle = !logToggle;
            DebugConsole.Instance.consoleLogOutputting = logToggle && overrideConsole;

            return (logToggle ? "Logging in console and " : "Not logging in console and ") + 
                   (overrideConsole ? "overriding." : "not overriding.");
        }
    }
}
