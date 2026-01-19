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

        // 1. SAFETY CHECK
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

        // 2. MOUSE MOVEMENT
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

        // 3. CLICKS & SCROLL
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

        public void RightClick(int holdTime = 20)
        {
            CheckFailSafe();

            // Fix: Ensure holdTime is never less than 0
            int actualHold = Math.Max(0, holdTime);

            SendMouseAction(MOUSEEVENTF_RIGHTDOWN);
            Thread.Sleep(actualHold);
            SendMouseAction(MOUSEEVENTF_RIGHTUP);
        }

        public void Scroll(int amount)
        {
            var input = new INPUT { type = INPUT_MOUSE };
            input.mi.dwFlags = MOUSEEVENTF_WHEEL;
            input.mi.mouseData = (uint)amount;
            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        // 4. DRAGGING
        public void Drag(Point start, Point end, bool humanLike = false)
        {
            MoveMouse(start, humanLike);

            SendMouseAction(MOUSEEVENTF_LEFTDOWN);
            Thread.Sleep(30); // Brief pause to ensure the grab registers

            MoveMouse(end, humanLike);

            Thread.Sleep(30); // Brief pause before releasing
            SendMouseAction(MOUSEEVENTF_LEFTUP);
        }

        // 5. KEYBOARD
        public void PressKey(VirtualKey key)
        {
            CheckFailSafe();
            SendKeyInput((ushort)key, 0); // Down
            Thread.Sleep(_rng.Next(50, 100));
            SendKeyInput((ushort)key, KEYEVENTF_KEYUP); // Up
        }

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