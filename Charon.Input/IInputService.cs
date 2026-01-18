using System.Drawing; // For Point

namespace Charon.Input
{
    public interface IInputService
    {
        // =========================================================
        // MOUSE MOVEMENT
        // =========================================================

        /// <summary>
        /// Moves the mouse to the target coordinates.
        /// </summary>
        /// <param name="destination">Target X,Y on screen.</param>
        /// <param name="humanLike">
        /// If TRUE: Uses Bezier curves and variable speed (Stealth).
        /// If FALSE: Teleports instantly (Speed).
        /// </param>
        void MoveMouse(Point destination, bool humanLike = true);

        /// <summary>
        /// Moves mouse to start, holds left click, moves to end, releases.
        /// </summary>
        void Drag(Point start, Point end, bool humanLike = true);

        /// <summary>
        /// Scrolls the mouse wheel.
        /// Positive = Up, Negative = Down. (Standard 'click' is 120).
        /// </summary>
        void Scroll(int scrollAmount, bool humanLike = true);

        // =========================================================
        // CLICKS
        // =========================================================

        /// <summary>
        /// Left Click (Down + Delay + Up).
        /// </summary>
        void LeftClick(bool humanLike = true);

        /// <summary>
        /// Right Click (Down + Delay + Up).
        /// </summary>
        void RightClick(bool humanLike = true);

        // =========================================================
        // KEYBOARD
        // =========================================================

        /// <summary>
        /// Presses a key (Down + Delay + Up).
        /// </summary>
        void PressKey(VirtualKey key, bool humanLike = true);
    }
}