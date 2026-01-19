using System.Drawing;

namespace Charon.Input
{
    public interface IInputService
    {
        // MOUSE MOVEMENT
        /// Moves the mouse to the target coordinates.
        /// <param name="dest">Target X,Y on screen.</param>
        /// <param name="humanLike">If true, uses Bezier curves; if false, teleports instantly.</param>
        void MoveMouse(Point dest, bool humanLike = false);

        /// Drags from a start point to an end point.
        void Drag(Point start, Point end, bool humanLike = false);

        /// Scrolls the mouse wheel. Positive for up, negative for down.
        void Scroll(int amount);

        // CLICKS
        /// Performs a click with a specific hold duration.
        /// <param name="holdTime">Duration in milliseconds to keep the button pressed.</param>
        void LeftClick(int holdTime = 20);
        void RightClick(int holdTime = 20);

        // KEYBOARD
        /// Presses a virtual key (down, delay, up).
        /// <param name="key">The VirtualKey to press.</param>
        /// <param name="holdTime">Duration in milliseconds to keep the key pressed.</param>
        void PressKey(VirtualKey key, int holdTime = 50);

        // UTILITY & SAFETY
        /// Checks if the mouse is in the fail-safe position (0,0).
        /// Throws OperationCanceledException if triggered.
        bool CheckFailSafe();
    }
}