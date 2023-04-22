using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using CSharpFunctionalExtensions;
using GlobalAgendaLauncher.Util;
using Reloaded.Injector;

namespace GlobalAgendaLauncher.Controllers
{
    public enum LoginUIElement
    {
        Username,
        Password,
        Login,
    }

    public class GABinary
    {
        /// <summary>
        /// Send an message to a Windows process.
        /// </summary>
        /// <param name="hWnd">Windows process handle</param>
        /// <param name="Msg">Message type code</param>
        /// <param name="wParam">Argument 1 (depends on message type)</param>
        /// <param name="lParam">Argument 2 (depends on message type)</param>
        /// <returns>True if message sent successfully</returns>
        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        /// <summary>
        /// Get the size of a Windows window. Specifically the client area.
        /// </summary>
        /// <param name="hWnd">Windows process handle</param>
        /// <param name="rect">Variable in which size rectangle will be emitted</param>
        /// <returns>True if successfully got the result, false otherwise</returns>

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetClientRect(IntPtr hWnd, out Rectangle rect);

        /// <summary>
        /// Keyboard key down message event code.
        /// </summary>
        private const uint WM_KEYDOWN = 0x0100;
        /// <summary>
        /// Left mouse button down message event code.
        /// </summary>
        private const uint WM_LBUTTONDOWN = 0x0201;
        /// <summary>
        /// Left mouse button up message event code.
        /// </summary>
        private const uint WM_LBUTTONUP = 0x0202;

        /// <summary>
        /// Argument to the mouse button down message type indicating that only the left button is clicked.
        /// </summary>
        private const int MK_LBUTTON = 0x0001;


        /// <summary>
        /// Location where GA could be installed in a standard computer on Steam.
        /// </summary>
        public const string GLOBAL_AGENDA_BINARY_STEAM_PATH = "\"C:\\Users\\conta\\AppData\\Local\\Microsoft\\WindowsApps\\pbrush.exe\"";//"\"C:\\Program Files (x86)\\Steam\\steamapps\\common\\Global Agenda Live\\Binaries\\GlobalAgenda.exe\"";

        /// <summary>
        /// Commonly used options passed to the GA binary which allow it to launch.
        /// </summary>
        public const string DEFAULT_LAUNCH_OPTIONS = "-host=107.150.130.77 -hostdns=inapatl.globalagendagame.com -seekfreeloading -tcp=300 -log=";

        private const string CLIENT_DLL = "C:\\Users\\conta\\Documents\\Code\\games\\global-agenda-launcher\\GlobalAgendaLauncher\\GABinaryClient\\bin\\x86\\Debug\\net7.0\\GABinaryClient.dll";

        /// <summary>
        /// The location of the game binary.
        /// </summary>
        private string path;

        /// <summary>
        /// Game binary launch options.
        /// </summary>
        private string opts;

        /// <summary>
        /// System object for running process.
        /// </summary>
        private Process process;

        /// <summary>
        /// Client used to inject DLLs and call functions.
        /// </summary>
        private Injector? injector;

        /// <summary>
        /// Internal tracker of if process is running. Set to true and false based on process events.
        /// </summary>
        private bool processRunning = false;

        private struct CoordinatesArg
        {
            public ushort x;
            public ushort y;
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="path"></param>
        public GABinary(string path, string opts)
        {
            this.path = path;
            this.opts = opts;
        }

        /// <summary>
        /// Start the process.
        /// </summary>
        public Result<Empty> Launch()
        {
            // Start game binary
            process = Process.Start(path, opts);
            processRunning = true;
            process.Exited += OnProcessExited;

            return Result.Success<Empty>(Empty.AnEmpty);
        }

        private Result<Empty> Inject()
        {
            injector = new Injector(process);
            var injectRes = injector.Inject(CLIENT_DLL);

            if (injectRes == 0)
            {
                return Result.Failure<Empty>("Failed to inject client into game");
            }

            return Result.Success<Empty>(Empty.AnEmpty);
        }

        public void Stop()
        {
            process.Kill();
            processRunning = false;
        }

        /// <summary>
        /// Complete the login process with a running game.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public Result<Empty> Login(string username, string password)
        {
            if (!IsRunning())
            {
                return Result.Failure<Empty>("Process is not running");
            }

            var injectRes = Inject();
            if (injectRes.IsFailure)
            {
                return Result.Failure<Empty>(injectRes.Error);
            }

            return ClickOnLoginUIElement(LoginUIElement.Username);
        }

        /// <summary>
        /// Event handler for when process exits.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProcessExited(object sender, EventArgs e)
        {
            processRunning = false;
            injector = null;
        }

        /// <summary>
        /// Determines if the GA binary is running.
        /// </summary>
        /// <returns></returns>
        public bool IsRunning()
        {
            if (process is null)
            {
                return false;
            }

            return processRunning;
        }

        /// <summary>
        /// Determines if the game binary client is injected and ready for use.
        /// </summary>
        /// <returns>True if injected, false if not.</returns>
        private bool IsClientInjected()
        {
            return injector is not null;
        }

        /// <summary>
        /// Send text to the game binary as keyboard inputs.
        /// </summary>
        /// <param name="text">The text to send</param>
        private Result<Empty> SendText(string text)
        {
            if (!IsRunning())
            {
                return Result.Failure<Empty>("Process isn't running");
            }

            foreach (char c in text)
            {
                if (!PostMessage(process.MainWindowHandle, WM_KEYDOWN, c, 0))
                {
                    return Result.Failure<Empty>("Failed to send character code");
                }
            }

            return Result.Success<Empty>(Empty.AnEmpty);
        }

        /// <summary>
        /// Click on a specific element on the game login screen.
        /// </summary>
        /// <param name="item">The element to click on</param>
        /// <returns>If successful or failed</returns>
        private Result<Empty> ClickOnLoginUIElement(LoginUIElement item)
        {
            if (!IsRunning())
            {
                return Result.Failure<Empty>("Process is not running");
            }

            double clickX = 0;
            double clickY = 0;
            if (item == LoginUIElement.Username)
            {
                // 54, 768
                clickX = (double)183 / (double)1920;
                clickY = (double)759 / (double)1080;
            }

            return ClickAt(clickX, clickY);
        }

        /// <summary>
        /// Send a click event to the GA window. Coordinates are from [0, 1] where 0 is the left / top and 1 is the right / bottom.
        /// </summary>
        /// <param name="x">Where to click [0,1] left to right within the window</param>
        /// <param name="y">Where to click [0, 1] top to bottom within the window</param>
        /// <returns>If successful or failed</returns>
        private Result<Empty> ClickAt(double x, double y)
        {
            if (!IsRunning())
            {
                return Result.Failure<Empty>("Process is not running");
            }

            if (!IsClientInjected())
            {
                return Result.Failure<Empty>("Client is not injected");
            }

            // Get window size
            Rectangle wSize = new Rectangle();
            GetClientRect(this.process.MainWindowHandle, out wSize);

            var wX = wSize.Right;
            var wY = wSize.Bottom;

            Debug.Print(String.Format("wX={0}, wY={1}", wX, wY));

            // Given window size calcualte the x, y coordinates on which to click
            ushort clickX = (ushort)Math.Round(x * wX);
            ushort clickY = (ushort)Math.Round(y * wY);

            Debug.Print(String.Format("x={2}, y={3}, clickX={0}, clickY={1}", clickX, clickY, x, y));

            // Click
            var res = injector.CallFunction(CLIENT_DLL, "GABinaryClient.GABinaryClient.ClickAt", new CoordinatesArg
            {
                x = clickX,
                y = clickY,
            });
            Debug.Print(String.Format("Call ClickAt={0}", res));

            return Result.Success<Empty>(Empty.AnEmpty);
        }

        /// <summary>
        /// Try to find the GA binary based on already known locations.
        /// </summary>
        /// <returns>
        /// Path of GA binary if the guessed path exists, null if the path guesses didn't find the binary.
        /// </returns>
        public static string? GuessPath()
        {
            // Check if the GA binary is in any known locations
            if (File.Exists(GLOBAL_AGENDA_BINARY_STEAM_PATH))
            {
                return GLOBAL_AGENDA_BINARY_STEAM_PATH;
            }

            return null;
        }
    }
}
