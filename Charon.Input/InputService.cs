using System;
using System.Drawing;
using System.Threading;
using System.Runtime.InteropServices;
using Charon.Vision;
using static Charon.Input.NativeMethods;

namespace Charon.Input
{
    /// <summary>
    /// Handles Mouse and Keyboard Input Simulation.
    /// Has options for Human-Like movement (Bezier curves, variable speed).
    /// </summary>
    public class InputService : IInputService
    {
        private readonly Random _rng = new Random();
        private readonly IVisionService _vision;

        public InputService(IVisionService vision)
        {
            _vision = vision;
        }

        /// <summary>
        /// Throws an exception if the mouse is at (0,0).
        /// Call this at the start of every major input action.
        /// </summary>
        public bool CheckFailSafe()
        {
            GetCursorPos(out POINT pos);
            if (pos.X == 0 && pos.Y == 0)
            {
                throw new OperationCanceledException("Fail-safe triggered: Mouse moved to corner.");
            }
            return false;
        }

        public void MoveMouse(Point dest, bool humanLike = true)
        {
            CheckFailSafe(); // SAFETY CHECK

            if (!humanLike)
            {
                SendMouseInput(dest.X, dest.Y, MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_VIRTUALDESK);
                return;
            }

            GetCursorPos(out POINT start);
            Point pStart = new Point(start.X, start.Y);

            Point c1 = new Point(pStart.X + _rng.Next(-100, 100), pStart.Y + _rng.Next(-100, 100));
            Point c2 = new Point(dest.X + _rng.Next(-100, 100), dest.Y + _rng.Next(-100, 100));

            int steps = _rng.Next(10, 25);
            for (int i = 1; i <= steps; i++)
            {
                // Re-check safety during long movements
                if (i % 5 == 0) CheckFailSafe();

                double t = i / (double)steps;
                Point pos = CalculateBezierPoint(t, pStart, c1, c2, dest);
                SendMouseInput(pos.X, pos.Y, MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_VIRTUALDESK);
                Thread.Sleep(_rng.Next(5, 15));
            }
        }

        public void Drag(Point start, Point end, bool humanLike = true)
        {
            MoveMouse(start, humanLike);
            SendMouseAction(MOUSEEVENTF_LEFTDOWN);
            Thread.Sleep(_rng.Next(100, 200));

            MoveMouse(end, humanLike);
            Thread.Sleep(_rng.Next(100, 200));
            SendMouseAction(MOUSEEVENTF_LEFTUP);
        }

        public void Scroll(int amount, bool humanLike = true)
        {
            var input = CreateMouseInput(0, 0, MOUSEEVENTF_WHEEL);
            input.mi.mouseData = (uint)amount;
            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        public void LeftClick(bool humanLike = true)
        {
            CheckFailSafe(); // SAFETY CHECK
            SendMouseAction(MOUSEEVENTF_LEFTDOWN);
            if (humanLike) Thread.Sleep(_rng.Next(50, 100));
            SendMouseAction(MOUSEEVENTF_LEFTUP);
        }

        public void RightClick(bool humanLike = true)
        {
            SendMouseAction(MOUSEEVENTF_RIGHTDOWN);
            if (humanLike) Thread.Sleep(_rng.Next(50, 100));
            SendMouseAction(MOUSEEVENTF_RIGHTUP);
        }

        public void PressKey(VirtualKey key, bool humanLike = true)
        {
            SendKeyInput((ushort)key, 0); // Down
            if (humanLike) Thread.Sleep(_rng.Next(50, 100));
            SendKeyInput((ushort)key, KEYEVENTF_KEYUP); // Up
        }

        // =========================================================
        // MULTI-MONITOR COORDINATE CALCULATIONS
        // =========================================================

        private void SendMouseInput(int x, int y, uint flags)
        {
            // Get the entire Virtual Desktop metrics to handle negative coordinates (left-side monitors)
            int vLeft = GetSystemMetrics(SM_XVIRTUALSCREEN);
            int vTop = GetSystemMetrics(SM_YVIRTUALSCREEN);
            int vWidth = GetSystemMetrics(SM_CXVIRTUALSCREEN);
            int vHeight = GetSystemMetrics(SM_CYVIRTUALSCREEN);

            // Normalize coordinates to the 0-65535 range required by Windows SendInput
            int absX = ((x - vLeft) * 65536) / vWidth;
            int absY = ((y - vTop) * 65536) / vHeight;

            var input = CreateMouseInput(absX, absY, flags);
            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        private void SendMouseAction(uint flags)
        {
            var input = CreateMouseInput(0, 0, flags);
            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        private void SendKeyInput(ushort vk, uint flags)
        {
            var input = new INPUT { type = INPUT_KEYBOARD };
            input.ki.wVk = vk;
            input.ki.dwFlags = flags;
            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        private INPUT CreateMouseInput(int x, int y, uint flags)
        {
            return new INPUT
            {
                type = INPUT_MOUSE,
                mi = new MOUSEINPUT { dx = x, dy = y, dwFlags = flags }
            };
        }

        private Point CalculateBezierPoint(double t, Point p0, Point p1, Point p2, Point p3)
        {
            // Cubic Bezier Formula
            double invT = 1 - t;
            int x = (int)(Math.Pow(invT, 3) * p0.X + 3 * Math.Pow(invT, 2) * t * p1.X + 3 * invT * Math.Pow(t, 2) * p2.X + Math.Pow(t, 3) * p3.X);
            int y = (int)(Math.Pow(invT, 3) * p0.Y + 3 * Math.Pow(invT, 2) * t * p1.Y + 3 * invT * Math.Pow(t, 2) * p2.Y + Math.Pow(t, 3) * p3.Y);
            return new Point(x, y);
        }
    }
}