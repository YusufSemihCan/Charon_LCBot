namespace Charon.Logic.Navigation
{
    public enum MenuNavigationState
    {
        Unknown,
        Transition,    // Any screen transition
        Connecting,    // Loading screen
        Disconnected,  // Loading Failed screen
        Settings,      // After pressing ESC on main menu
        Popups,        // Any popup window
        Window,        // In the Window menu
        Charge,        // In the Charge menu
        Sinners,       // In the Sinners menu
        // Expanded Drive options
        Drive,         // In the Drive menu
        // Luxcavation options
        Luxcavation_EXP,   // In the Luxcavation Exp menu
        Luxcavation_Thread, // In the Luxcavation Thread menu
        // Mirror Dungeon options
        MirrorDungeon,        // In the Mirror Dungeon menu
        MirrorDungeon_Delving, // In the Delving menu
        // Successful completion of Mirror Dungeon and Luxcavation
        Success       // Success screen after completing

    }
}