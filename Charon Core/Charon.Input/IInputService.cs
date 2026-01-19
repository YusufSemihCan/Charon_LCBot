using System.Drawing;

namespace Charon.Input
{
    public interface IInputService
    {
        // MOUSE MOVEMENT
        /// <summary>
        /// Moves the mouse to the target coordinates.
        /// </summary>
        /// <param name="dest">Target X,Y on screen.</param>
        /// <param name="humanLike">If true, uses Bezier curves; if false, teleports instantly.</param>
        void MoveMouse(Point dest, bool humanLike = false);

        /// <summary>
        /// Drags from a start point to an end point.
        /// </summary>
        void Drag(Point start, Point end, bool humanLike = false);

        /// <summary>
        /// Scrolls the mouse wheel. Positive for up, negative for down.
        /// </summary>
        void Scroll(int amount);

        // CLICKS
        /// <summary>
        /// Performs a standard left click (down, delay, up).
        /// </summary>
        void LeftClick();

        /// <summary>
        /// Performs a standard right click (down, delay, up).
        /// </summary>
        void RightClick();

        // KEYBOARD
        /// <summary>
        /// Presses a virtual key (down, delay, up).
        /// </summary>
        void PressKey(VirtualKey key);

        // UTILITY & SAFETY
        /// <summary>
        /// Checks if the mouse is in the fail-safe position (0,0).
        /// Throws OperationCanceledException if triggered.
        /// </summary>
        bool CheckFailSafe();
    }
}