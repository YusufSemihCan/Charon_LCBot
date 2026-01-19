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
        /// <param name="destination">Target X,Y on screen.</param>
        /// <param name="humanLike">
        /// If TRUE: Uses Bezier curves and variable speed (Stealth).
        /// If FALSE: Teleports instantly (Speed).
        /// </param>
        void MoveMouse(Point destination, bool humanLike = true);

        /// Moves mouse to start, holds left click, moves to end, releases.
        void Drag(Point start, Point end, bool humanLike = true);

        /// <summary>
        /// Scrolls the mouse wheel.
        /// Positive = Up, Negative = Down. (Standard 'click' is 120).
        /// </summary>
        void Scroll(int scrollAmount, bool humanLike = true);

        // =========================================================
        // CLICKS
        // =========================================================

        // Left Click (Down + Delay + Up).
        void LeftClick(bool humanLike = true);

        // Right Click (Down + Delay + Up).
        void RightClick(bool humanLike = true);

        // =========================================================
        // KEYBOARD
        // =========================================================

        // Presses a key (Down + Delay + Up).
        void PressKey(VirtualKey key, bool humanLike = true);

        // Checks if the mouse is in a "Fail-Safe" position (top-left corner).
        bool CheckFailSafe();
    }
}