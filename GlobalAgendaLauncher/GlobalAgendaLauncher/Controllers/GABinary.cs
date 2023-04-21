using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

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
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out Rectangle rect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetClientRect(IntPtr hWnd, out Rectangle rect);

        [DllImport("user32.dll")]
        internal static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        internal static extern IntPtr SetCapture(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern uint SendInput(uint cInputs, INPUT[] pInputs, int cbSize);

        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_LBUTTONDOWN = 0x0201;
        private const uint WM_LBUTTONUP = 0x0202;
        private const uint WM_SETCURSOR = 0x0020;
        private const uint WM_CAPTURECHANGED = 0x0215;

        private const int MK_LBUTTON = 0x0001;
        private const int HTCLIENT = 1;

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            internal InputType type;
            internal InputUnion U;
            internal static int Size
            {
                get { return Marshal.SizeOf(typeof(INPUT)); }
            }
        }

        public enum InputType : uint
        {
            INPUT_MOUSE,
            INPUT_KEYBOARD,
            INPUT_HARDWARE
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public long dx;
            public long dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public ulong dwExtraInfo;
        }

        public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            ushort wVk;
            ushort wScan;
            uint dwFlags;
            uint time;
            ulong dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            uint uMsg;
            ushort wParamL;
            ushort wParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)]
            internal MOUSEINPUT mi;
            [FieldOffset(0)]
            internal KEYBDINPUT ki;
            [FieldOffset(0)]
            internal HARDWAREINPUT hi;
        }


        /// <summary>
        /// Location where GA could be installed in a standard computer on Steam.
        /// </summary>
        public const string GLOBAL_AGENDA_BINARY_STEAM_PATH = "\"C:\\Users\\conta\\AppData\\Local\\Microsoft\\WindowsApps\\pbrush.exe\"";//"\"C:\\Program Files (x86)\\Steam\\steamapps\\common\\Global Agenda Live\\Binaries\\GlobalAgenda.exe\"";

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
                //SendMessage(process.Id, WM_KEYDOWN, (IntPtr)c, (IntPtr)0);
                PostMessage(process.MainWindowHandle, WM_KEYDOWN, c, 0);
            }
        }

        public void ClickOnLoginUIElement(LoginUIElement item)
        {
            if (process is null)
            {
                return;
            }

            double clickX = 0;
            double clickY = 0;
            if (item == LoginUIElement.Username)
            {
                // 54, 768
                clickX = (double)183 / (double)1920;
                clickY = (double)759 / (double)1080;
            }

            ClickAt(clickX, clickY);
        }

        /// <summary>
        /// Send a click event to the GA window. Coordinates are from [0, 1] where 0 is the left / top and 1 is the right / bottom.
        /// </summary>
        /// <param name="x">Where to click [0,1] left to right within the window</param>
        /// <param name="y">Where to click [0, 1] top to bottom within the window</param>
        public void ClickAt(double x, double y)
        {
            if (process is null)
            {
                return;
            }

            // Get window size
            //ShowWindow(this.process.MainWindowHandle, 5);
            //SetForegroundWindow(this.process.MainWindowHandle);

            Rectangle wSize = new Rectangle();
            GetClientRect(this.process.MainWindowHandle, out wSize);

            var wX = wSize.Right;
            var wY = wSize.Bottom;

            Debug.Print(String.Format("wX={0}, wY={1}", wX, wY));

            ushort clickX = (ushort)Math.Round(x * wX);
            ushort clickY = (ushort)Math.Round(y * wY);

            var packedClick = (clickY << 16) | clickX;

            Debug.Print(String.Format("x={2}, y={3}, clickX={0}, clickY={1}", clickX, clickY, x, y));

            MOUSEINPUT mouseInput = new MOUSEINPUT();
            mouseInput.dx = clickX;
            mouseInput.dy = clickY;
            mouseInput.mouseData = 0;
            mouseInput.dwFlags = MOUSEEVENTF_LEFTDOWN;
            mouseInput.time = 0;
            mouseInput.dwExtraInfo = 0;

            InputUnion inputUnion = new InputUnion();
            inputUnion.mi = mouseInput;

            INPUT[] inputs = new INPUT[1];
            inputs[0].type = InputType.INPUT_MOUSE;
            inputs[0].U = inputUnion;

            //SendInput(1, inputs, INPUT.Size);

            //SetCapture(process.MainWindowHandle);


            var hwnd = ((MauiWinUIWindow)App.Current.Windows[0].Handler.PlatformView).WindowHandle;

            // var hwnd = ((MauiWinUIWindow)App.Current.Windows[0].Handler.NativeView).WindowHandle;


            PostMessage(hwnd, WM_SETCURSOR, (int)process.MainWindowHandle, ((ushort)WM_LBUTTONDOWN << 16) | (ushort)HTCLIENT);
            PostMessage(hwnd, WM_CAPTURECHANGED, 0, (int)process.MainWindowHandle);
            //var proccessedDown = PostMessage(process.MainWindowHandle, WM_LBUTTONDOWN, MK_LBUTTON, packedClick);
            //var proccessedUp = PostMessage(process.MainWindowHandle, WM_LBUTTONUP, MK_LBUTTON, packedClick);

            //Debug.Print(String.Format("down={0}, up={1}", proccessedDown, proccessedUp));
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
