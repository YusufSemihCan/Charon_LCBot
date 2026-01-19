namespace Charon.Logic.Navigation
{
    public interface INavigation
    {
        /// <summary>
        /// Gets the current detected menu state of the bot.
        /// </summary>
        NavigationState CurrentState { get; }

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