using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GABinaryClient
{

    public static class GABinaryClient
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

        public static void Nothing(Int32 n)
        {

        }

        /// <summary>
        /// Click on a specific coordinate in the process.
        /// (0, 0) is top left.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>True if successful, false if failure.</returns>
        public static bool ClickAt(ushort x, ushort y)
        {
            return true;
            var process = Process.GetCurrentProcess();

            var packedClick = (x << 16) | y;

            if (!PostMessage(process.MainWindowHandle, WM_LBUTTONDOWN, MK_LBUTTON, packedClick))
            {
                return false;
            }
            if (!PostMessage(process.MainWindowHandle, WM_LBUTTONUP, MK_LBUTTON, packedClick))
            {
                return false;
            }

            return true;
        }
    }
}
