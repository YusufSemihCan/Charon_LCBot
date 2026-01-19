using System;
using System.Drawing;
using System.Threading;
using System.Runtime.InteropServices;
using static Charon.Input.NativeMethods;

namespace Charon.Input
{
    public class InputService : IInputService
    {
        private readonly Random _rng = new Random();

        #region Safety Check
        // 1. SAFETY CHECK
        /// <summary>
        /// Checks if the mouse is in the fail-safe position (0,0) and throws an exception if true.
        /// </summary>
        /// <returns>False if safe, throws exception if not.</returns>
        public bool CheckFailSafe()
        {
            GetCursorPos(out POINT pos);
            if (pos.X == 0 && pos.Y == 0)
            {
                // Instantly stops the bot if mouse is at top-left
                throw new OperationCanceledException("Fail-safe triggered: Mouse moved to corner.");
            }
            return false;
        }
        #endregion

        #region Mouse Movement
        // 2. MOUSE MOVEMENT
        /// <summary>
        /// Moves the mouse to the specified coordinates.
        /// </summary>
        /// <param name="dest">Target coordinates.</param>
        /// <param name="humanLike">If true, uses a human-like Bezier curve path.</param>
        public void MoveMouse(Point dest, bool humanLike = false)
        {
            CheckFailSafe(); // Safety check before moving

            if (!humanLike)
            {
                SendMouseInput(dest.X, dest.Y, MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_VIRTUALDESK);
                return;
            }

            // Human-like Bezier path
            GetCursorPos(out POINT start);
            Point pStart = new Point(start.X, start.Y);

            Point c1 = new Point(pStart.X + _rng.Next(-100, 100), pStart.Y + _rng.Next(-100, 100));
            Point c2 = new Point(dest.X + _rng.Next(-100, 100), dest.Y + _rng.Next(-100, 100));

            int steps = _rng.Next(10, 25);
            for (int i = 1; i <= steps; i++)
            {
                if (i % 5 == 0) CheckFailSafe(); // Re-check safety during the curve

                double t = i / (double)steps;
                Point pos = CalculateBezierPoint(t, pStart, c1, c2, dest);
                SendMouseInput(pos.X, pos.Y, MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_VIRTUALDESK);
                Thread.Sleep(_rng.Next(5, 15));
            }
        }
        #endregion

        #region Clicks and Scroll
        // 3. CLICKS & SCROLL
        /// <summary>
        /// Performs a left mouse click.
        /// </summary>
        /// <param name="holdTime">How long to hold the button down in ms.</param>
        public void LeftClick(int holdTime = 20)
        {
            CheckFailSafe();

            // Fix: Ensure holdTime is never less than 0
            int actualHold = Math.Max(0, holdTime);

            SendMouseAction(MOUSEEVENTF_LEFTDOWN);
            // Explicitly hold for the duration requested
            Thread.Sleep(actualHold);
            SendMouseAction(MOUSEEVENTF_LEFTUP);
        }

        /// <summary>
        /// Performs a right mouse click.
        /// </summary>
        /// <param name="holdTime">How long to hold the button down in ms.</param>
        public void RightClick(int holdTime = 20)
        {
            CheckFailSafe();

            // Fix: Ensure holdTime is never less than 0
            int actualHold = Math.Max(0, holdTime);

            SendMouseAction(MOUSEEVENTF_RIGHTDOWN);
            Thread.Sleep(actualHold);
            SendMouseAction(MOUSEEVENTF_RIGHTUP);
        }

        /// <summary>
        /// Scrolls the mouse wheel.
        /// </summary>
        /// <param name="amount">Amount to scroll (positive is up, negative is down).</param>
        public void Scroll(int amount)
        {
            var input = new INPUT { type = INPUT_MOUSE };
            input.mi.dwFlags = MOUSEEVENTF_WHEEL;
            input.mi.mouseData = (uint)amount;
            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }
        #endregion

        #region Dragging
        // 4. DRAGGING
        /// <summary>
        /// Drags the mouse from start to end.
        /// </summary>
        /// <param name="start">Start coordinates.</param>
        /// <param name="end">End coordinates.</param>
        /// <param name="humanLike">If true, uses human-like movement.</param>
        public void Drag(Point start, Point end, bool humanLike = false)
        {
            MoveMouse(start, humanLike);

            SendMouseAction(MOUSEEVENTF_LEFTDOWN);
            Thread.Sleep(30); // Brief pause to ensure the grab registers

            MoveMouse(end, humanLike);

            Thread.Sleep(30); // Brief pause before releasing
            SendMouseAction(MOUSEEVENTF_LEFTUP);
        }
        #endregion

        #region Keyboard
        // 5. KEYBOARD
        /// <summary>
        /// Presses a keyboard key.
        /// </summary>
        /// <param name="key">The virtual key to press.</param>
        /// <param name="holdTime">How long to hold the key down in ms.</param>
        public void PressKey(VirtualKey key, int holdTime = 20)
        {
            CheckFailSafe();

            // Ensure holdTime is never less than 0 to avoid Thread.Sleep crashes
            int actualHold = Math.Max(0, holdTime);

            SendKeyInput((ushort)key, KEYEVENTF_KEYDOWN); // Down
            Thread.Sleep(actualHold);
            SendKeyInput((ushort)key, KEYEVENTF_KEYUP);   // Up
        }
        #endregion

        // Private Calculations
        private void SendMouseInput(int x, int y, uint flags)
        {
            int vLeft = GetSystemMetrics(SM_XVIRTUALSCREEN);
            int vTop = GetSystemMetrics(SM_YVIRTUALSCREEN);
            int vWidth = GetSystemMetrics(SM_CXVIRTUALSCREEN);
            int vHeight = GetSystemMetrics(SM_CYVIRTUALSCREEN);

            // Convert pixels to 0-65535 normalized coordinates
            int absX = ((x - vLeft) * 65536) / vWidth;
            int absY = ((y - vTop) * 65536) / vHeight;

            var input = new INPUT
            {
                type = INPUT_MOUSE,
                mi = new MOUSEINPUT { dx = absX, dy = absY, dwFlags = flags }
            };
            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        private void SendMouseAction(uint flags)
        {
            var input = new INPUT { type = INPUT_MOUSE };
            input.mi.dwFlags = flags;
            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        private void SendKeyInput(ushort vk, uint flags)
        {
            var input = new INPUT { type = INPUT_KEYBOARD };
            input.ki.wVk = vk;
            input.ki.dwFlags = flags;
            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        private Point CalculateBezierPoint(double t, Point p0, Point p1, Point p2, Point p3)
        {
            double invT = 1 - t;
            int x = (int)(Math.Pow(invT, 3) * p0.X + 3 * Math.Pow(invT, 2) * t * p1.X + 3 * invT * Math.Pow(t, 2) * p2.X + Math.Pow(t, 3) * p3.X);
            int y = (int)(Math.Pow(invT, 3) * p0.Y + 3 * Math.Pow(invT, 2) * t * p1.Y + 3 * invT * Math.Pow(t, 2) * p2.Y + Math.Pow(t, 3) * p3.Y);
            return new Point(x, y);
        }
    }
}