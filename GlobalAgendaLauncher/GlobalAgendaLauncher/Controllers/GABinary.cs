using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GlobalAgendaLauncher.Controllers
{
    public class GABinary
    {
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint WM_KEYDOWN = 0x100;

        /// <summary>
        /// Location where GA could be installed in a standard computer on Steam.
        /// </summary>
        public const string GLOBAL_AGENDA_BINARY_STEAM_PATH = "\"C:\\Program Files (x86)\\Steam\\steamapps\\common\\Global Agenda Live\\Binaries\\GlobalAgenda.exe\"";

        public const string DEFAULT_LAUNCH_OPTIONS = "-host=107.150.130.77 -hostdns=inapatl.globalagendagame.com -seekfreeloading -tcp=300 -log=";

        /// <summary>
        /// The location of the game binary.
        /// </summary>
        private string path;

        private string opts;

        /// <summary>
        /// System object for running process.
        /// </summary>
        private System.Diagnostics.Process process;

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="path"></param>
        public GABinary(string path, string opts)
        {
            this.path = path;
            this.opts = opts;
        }

        public void Launch()
        {
            this.process = System.Diagnostics.Process.Start(path, opts);
        }

        public void SendText(string text)
        {
            if (process is null)
            {
                return;
            }

            foreach (char c in text)
            {
                SendMessage(process.Id, WM_KEYDOWN, (IntPtr)c, (IntPtr)0);
            }
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
