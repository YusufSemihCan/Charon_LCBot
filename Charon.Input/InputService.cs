using System.Runtime.InteropServices; // Required for 'Marshal' and 'DllImport'
using System.Windows.Input;           // Required for 'Key' and 'KeyInterop'

namespace Charon.Input
{
    public class InputService : IInputService
    {
        private readonly Random _random = new Random();

        public void Click(int x, int y)
        {
            // 1. Get Screen Resolution dynamically
            // 0 = SM_CXSCREEN (Width), 1 = SM_CYSCREEN (Height)
            int screenWidth = NativeMethods.GetSystemMetrics(0);
            int screenHeight = NativeMethods.GetSystemMetrics(1);

            // 2. Convert pixels to "Absolute" coordinates (0 to 65535)
            int absX = (x * 65535) / screenWidth;
            int absY = (y * 65535) / screenHeight;

            // 3. Define the inputs: Move, Down, Up
            NativeMethods.INPUT[] inputs = new NativeMethods.INPUT[3];

            // Move
            inputs[0].type = NativeMethods.INPUT_MOUSE;
            inputs[0].U.mi.dx = absX;
            inputs[0].U.mi.dy = absY;
            inputs[0].U.mi.dwFlags = NativeMethods.MOUSEEVENTF_MOVE | NativeMethods.MOUSEEVENTF_ABSOLUTE;

            // Click Down
            inputs[1].type = NativeMethods.INPUT_MOUSE;
            inputs[1].U.mi.dwFlags = NativeMethods.MOUSEEVENTF_LEFTDOWN;

            // Click Up
            inputs[2].type = NativeMethods.INPUT_MOUSE;
            inputs[2].U.mi.dwFlags = NativeMethods.MOUSEEVENTF_LEFTUP;

            // 4. Send the command
            NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));

            // Add a tiny random delay to feel human
            Thread.Sleep(_random.Next(20, 50));
        }

        public void PressKey(Key key)
        {
            // Convert WPF Key to Virtual Key Code
            ushort virtualKey = (ushort)KeyInterop.VirtualKeyFromKey(key);

            NativeMethods.INPUT[] inputs = new NativeMethods.INPUT[2];

            // Key Down
            inputs[0].type = NativeMethods.INPUT_KEYBOARD;
            inputs[0].U.ki.wVk = virtualKey;

            // Key Up
            inputs[1].type = NativeMethods.INPUT_KEYBOARD;
            inputs[1].U.ki.wVk = virtualKey;
            inputs[1].U.ki.dwFlags = NativeMethods.KEYEVENTF_KEYUP;

            NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));

            Thread.Sleep(_random.Next(30, 60));
        }
    }
}