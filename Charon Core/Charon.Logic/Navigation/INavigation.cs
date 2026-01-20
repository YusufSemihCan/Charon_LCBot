namespace Charon.Logic.Navigation
{
    public interface INavigation
    {
        /// <summary>
        /// Gets the current detected menu state of the bot.
        /// </summary>
        NavigationState CurrentState { get; }

        /// <summary>
        /// Gets or sets whether to use human-like mouse movement.
        /// </summary>
        bool HumanLikeMovement { get; set; }

        /// <summary>
        /// Gets or sets whether to automatically clear the cursor before scanning.
        /// </summary>
        bool AutoClearCursor { get; set; }

        /// <summary>
        /// Attempts to navigate the bot to a specific menu target.
        /// </summary>
        bool NavigateTo(NavigationState target);

        /// <summary>
        /// Scans the screen for anchors to sync the internal state with the game UI.
        /// </summary>
        NavigationState SynchronizeState();
    }
}