using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NXPorts.Attributes;

namespace GABinaryClient
{

    public unsafe static class GABinaryClient
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

        [DllImport("user32.dll")]
        static extern bool SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        static extern uint SendInput(uint cInputs, INPUT[] pInputs, int cbSize);

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

        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public SendInputEventType type;
            public MouseKeybdhardwareInputUnion mkhi;
        }
        [StructLayout(LayoutKind.Explicit)]
        struct MouseKeybdhardwareInputUnion
        {
            [FieldOffset(0)]
            public MouseInputData mi;

            [FieldOffset(0)]
            public KEYBDINPUT ki;

            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }
        struct MouseInputData
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public MouseEventFlags dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        [Flags]
        enum MouseEventFlags : uint
        {
            MOUSEEVENTF_MOVE = 0x0001,
            MOUSEEVENTF_LEFTDOWN = 0x0002,
            MOUSEEVENTF_LEFTUP = 0x0004,
            MOUSEEVENTF_RIGHTDOWN = 0x0008,
            MOUSEEVENTF_RIGHTUP = 0x0010,
            MOUSEEVENTF_MIDDLEDOWN = 0x0020,
            MOUSEEVENTF_MIDDLEUP = 0x0040,
            MOUSEEVENTF_XDOWN = 0x0080,
            MOUSEEVENTF_XUP = 0x0100,
            MOUSEEVENTF_WHEEL = 0x0800,
            MOUSEEVENTF_VIRTUALDESK = 0x4000,
            MOUSEEVENTF_ABSOLUTE = 0x8000
        }
        enum SendInputEventType : int
        {
            InputMouse,
            InputKeyboard,
            InputHardware
        }

        public static void ClickLeftMouseButton(ushort x, ushort y)
        {
            INPUT mouseDownInput = new INPUT();
            mouseDownInput.type = SendInputEventType.InputMouse;
            mouseDownInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTDOWN;
            mouseDownInput.mkhi.mi.dx = x;
            mouseDownInput.mkhi.mi.dy = y;

            INPUT[] mouseDownInputs = { mouseDownInput };
            SendInput(1, mouseDownInputs, Marshal.SizeOf(new INPUT()));

            INPUT mouseUpInput = new INPUT();
            mouseUpInput.type = SendInputEventType.InputMouse;
            mouseUpInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTUP;
            mouseUpInput.mkhi.mi.dx = x;
            mouseUpInput.mkhi.mi.dy = y;

            INPUT[] mouseUpInputs = { mouseUpInput };

            SendInput(1, mouseUpInputs, Marshal.SizeOf(new INPUT()));
        }

        public struct CoordinatesArg
        {
            public ushort x;
            public ushort y;
        }

        /// <summary>
        /// Click on a specific coordinate in the process.
        /// (0, 0) is top left.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>True if successful, false if failure.</returns>
        [DllExport()]
        public static bool ClickAt(CoordinatesArg* args)
        {
            var process = Process.GetCurrentProcess();

            var packedClick = (args->y << 16) | args->x;

            /*
            MOUSEINPUT mouseInput = new MOUSEINPUT();
            mouseInput.dx = args->x;
            mouseInput.dy = args->y;
            mouseInput.mouseData = 0;
            mouseInput.dwFlags = MOUSEEVENTF_LEFTDOWN;
            mouseInput.time = 0;
            mouseInput.dwExtraInfo = 0;

            InputUnion inputUnion = new InputUnion();
            inputUnion.mi = mouseInput;

            INPUT[] inputs = new INPUT[1];
            inputs[0].type = InputType.INPUT_MOUSE;
            inputs[0].U = inputUnion;

            SendInput(1, inputs, INPUT.Size);
            */
            //ClickLeftMouseButton(args->x, args->y);


            SendMessage(process.MainWindowHandle, WM_LBUTTONDOWN, MK_LBUTTON, packedClick);
            SendMessage(process.MainWindowHandle, WM_LBUTTONUP, MK_LBUTTON, packedClick);

            return true;
        }
    }
}
