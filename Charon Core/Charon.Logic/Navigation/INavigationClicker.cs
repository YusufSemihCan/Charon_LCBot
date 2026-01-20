using Charon.Input;

namespace Charon.Logic.Navigation
{
    /// <summary>
    /// Interface for Menu and UI interactions.
    /// Handles atomic actions like Clicking, Typing, and Hovering.
    /// </summary>
    public interface INavigationClicker
    {
        /// <summary>
        /// Finds a template and clicks it.
        /// </summary>
        bool ClickTemplate(string templateName, double threshold = 0.9);

        /// <summary>
        /// Finds a template, clicks, and holds the mouse button.
        /// </summary>
        bool ClickHold(string templateName, int durationMs, double threshold = 0.9);

        /// <summary>
        /// Waits for a template to appear and then clicks it.
        /// </summary>
        bool WaitAndClick(string templateName, int timeoutMs = 2000, double threshold = 0.9);
        
        /// <summary>
        /// Moves the mouse over a template without clicking.
        /// </summary>
        bool Hover(string templateName, double threshold = 0.9);

        /// <summary>
        /// Clicks a template to focus it, then types the specified text.
        /// </summary>
        void TypeInto(string templateName, string text, double threshold = 0.9);
        
        /// <summary>
        /// Simulates a keyboard key press.
        /// </summary>
        void PressKey(VirtualKey key, int holdTime = 20);

        /// <summary>
        /// Clicks a specific screen location (Rectangle center).
        /// </summary>
        bool ClickLocation(System.Drawing.Rectangle target);
        
        /// <summary>
        /// Dismisses a menu by pressing ESC.
        /// </summary>
        void DismissWithEsc();
    }
}