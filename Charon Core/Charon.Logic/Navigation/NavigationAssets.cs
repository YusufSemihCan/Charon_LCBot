namespace Charon.Logic.Navigation
{
    public enum NavigationState
    {
        Unknown,
        Transition,      // Any screen transition/fade
        Connecting,      // Loading/Connecting screen
        Disconnected,    // "Connection Failed" or similar
        Settings,        // Settings menu (Esc)
        Popups,          // Generic popups

        // Main Zones
        Window,          // The main "Window" dashboard
        Drive,           // The "Drive" bus menu
        Sinners,         // Character management

        // Sub-menus (Charge)
        Charge,
        Charge_Modules,
        Charge_Lunacy,

        // Luxcavation
        Luxcavation_EXP,
        Luxcavation_Thread,

        // Mirror Dungeon (MD)
        MirrorDungeon,
        MirrorDungeon_Confirmation,
        MirrorDungeon_Delving,
        
        // Battle Entry
        ToBattle,        // Pre-battle screen (Enter/Back)
        Battle,          // The actual battle
        
        Success          // Generic Success screen
    }

    public static class NavigationAssets
    {
        // --- State Anchors (Active Buttons: Used to verify current state) ---
        public const string ButtonActiveWindow = "Button_A_Window";
        public const string ButtonActiveDrive = "Button_A_Drive";
        public const string ButtonActiveSinners = "Button_A_Sinners";
        
        // --- Navigation Buttons (Inactive Buttons: Click to transition) ---
        public const string ButtonInActiveWindow = "Button_I_Window";
        public const string ButtonInActiveDrive = "Button_I_Drive";
        public const string ButtonInActiveSinners = "Button_I_Sinners";
        
        // Text Fallbacks (Restored for Build/Resiliency)
        public const string ButtonTextDrive = "Button_Text_Active_Drive"; 
        public const string ButtonTextLuxcavation = "Button_Text_Luxcavation";

        // --- Common UI ---
        public const string ButtonBack = "Button_Back";
        public const string ButtonBackLuxcavation = "Button_Back_Luxcavation";
        public const string ButtonBackMirrorDungeon = "Button_Back_MirrorDungeon";
        
        public const string ButtonConfirm = "Button_Confirm";
        public const string ButtonEnter = "Button_Enter";
        public const string ButtonCancel = "Button_Cancel";
        
        // --- Drive / ID Entry ---
        public const string ButtonLuxcavation = "Button_Luxcavation"; // Main Entry Button
        
        // Luxcavation Tabs
        public const string ButtonActiveLuxcavationEXP = "Button_A_Luxcavation_EXP";
        public const string ButtonInActiveLuxcavationEXP = "Button_I_Luxcavation_EXP";
        
        public const string ButtonActiveLuxcavationThread = "Button_A_Luxcavation_Thread";
        public const string ButtonInActiveLuxcavationThread = "Button_I_Luxcavation_Thread";

        // Legacy keys mapped for compatibility (or update logic to use specific ones)
        // Used for Clicking: We typically click the inactive one to switch.
        public const string TextLuxcavationEXP = "Text_LuxcavationEXP_Stage"; // Placeholder
        public const string TextLuxcavationThread = "Text_LuxcavationThread"; 
        
        // Legacy Mappings (Restored)
        public const string ButtonLuxcavationEXP = ButtonInActiveLuxcavationEXP;       
        public const string ButtonLuxcavationThread = ButtonInActiveLuxcavationThread;

        // Luxcavation EXP Levels
        public const string TextLuxcavationLevel1 = "Text_LuxcavationEXP_Level-1";
        public const string TextLuxcavationLevel2 = "Text_LuxcavationEXP_Level-2";
        public const string TextLuxcavationLevel3 = "Text_LuxcavationEXP_Level-3";
        public const string TextLuxcavationLevel4 = "Text_LuxcavationEXP_Level-4";
        public const string TextLuxcavationLevel5 = "Text_LuxcavationEXP_Level-5";
        public const string TextLuxcavationLevel6 = "Text_LuxcavationEXP_Level-6";
        public const string TextLuxcavationLevel7 = "Text_LuxcavationEXP_Level-7";
        public const string TextLuxcavationLevel8 = "Text_LuxcavationEXP_Level-8";
        public const string TextLuxcavationLevel9 = "Text_LuxcavationEXP_Level-9";

        // Luxcavation Enter Buttons (Use closest Y match)
        // Note: Filenames are Button_LuxcavationEXP_Enter_2 and _3, but logic should probably trying to match any "Enter" looking button in that zone.
        // We will define the base template names.
        public const string ButtonLuxcavationEnter2 = "Button_LuxcavationEXP_Enter_2"; 
        public const string ButtonLuxcavationEnter3 = "Button_LuxcavationEXP_Enter_3";
        
        // ToBattle
        public const string IconToBattle = "Icons_ToBattle_Identifier";      
        public const string ButtonMirrorDungeon = "Button_MirrorDungeon";
        public const string ButtonMDEnter = "Button_MirrorDungeon_Enter";
        
        public const string ButtonMDInfinityMirror = "Button_A_MirrorDungeon_InfinityMirror";
        public const string ButtonInActiveMDInfinityMirror = "Button_I_MirrorDungeon_InfinityMirror";
        public const string ButtonMDS6 = "Button_MirrorDungeon_S6"; // Season 6 Banner?

        public const string ButtonMirrorDungeonRental = "Button_MirrorDungeon_Rental";
        public const string TextMirrorDungeonConfirmation = "Text_MirrorDungeon_Confirmation";

        public const string MDDungeonProgress = "MD_DungeonProgress";
        public const string MDStarlightCount = "MD_StarlightCount";
        public const string MDStarlightIcon = "MD_StarlightIcon";

        // --- Charge Menu ---
        // ChargeLabel seemingly removed/missing. We will detect via tabs.
        // public const string ChargeLabel = "Charge_Label"; 
        
        public const string ButtonChargeConfirm = ButtonConfirm; // Map to generic
        public const string ButtonChargeCancel = ButtonCancel;   // Map to generic

        public const string ButtonLuxcavationThreadEnter = "Button_LuxcavationThread_Enter";
        public const string ButtonLuxcavationThreadLevelEnter = "Button_LuxcavationThread_Level_Enter";
        
        // Luxcavation Thread Levels (Popup)
        public const string TextLuxcavationThreadLevel60 = "Text_LuxcavationThread_Level-60";
        public const string TextLuxcavationThreadLevel50 = "Text_LuxcavationThread_Level-50";
        public const string TextLuxcavationThreadLevel40 = "Text_LuxcavationThread_Level-40";
        public const string TextLuxcavationThreadLevel30 = "Text_LuxcavationThread_Level-30";
        public const string TextLuxcavationThreadLevel20 = "Text_LuxcavationThread_Level-20";
        
        // Active/InActive Tabs for Charge
        public const string ButtonActiveChargeModules = "Button_A_Charge_Modules";
        public const string ButtonInActiveChargeModules = "Button_I_Charge_Modules";
        
        public const string ButtonActiveChargeLunacy = "Button_A_Charge_Lunacy";
        public const string ButtonInActiveChargeLunacy = "Button_I_Charge_Lunacy";

        public const string EnkephalinBox = "Icon_EnkephalinBox";
    }
}