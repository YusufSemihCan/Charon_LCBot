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
        Charge_Boxes,
        Charge_Modules,
        Charge_Lunacy,

        // Luxcavation
        Luxcavation_EXP,
        Luxcavation_Thread,

        // Mirror Dungeon (MD)
        MirrorDungeon,
        MirrorDungeon_Delving,
        
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

        // Text Anchors (Restored for Build)
        public const string TextLuxcavationEXP = "Text_LuxcavationEXP_Stage"; // Placeholder or actual filename if known
        public const string TextLuxcavationThread = "Text_LuxcavationThread"; // Matches partial file 'Text_LuxcavationThread.png' found earlier
        
        // Legacy keys mapped for compatibility (or update logic to use specific ones)
        // Used for Clicking: We typically click the inactive one to switch.
        public const string ButtonLuxcavationEXP = ButtonInActiveLuxcavationEXP;       
        public const string ButtonLuxcavationThread = ButtonInActiveLuxcavationThread;
        
        public const string ButtonMirrorDungeon = "Button_MirrorDungeon";
        public const string ButtonMDEnter = "Button_MirrorDungeon_Enter";
        
        public const string ButtonMDInfinityMirror = "Button_A_MirrorDungeon_InfinityMirror";
        public const string ButtonInActiveMDInfinityMirror = "Button_I_MirrorDungeon_InfinityMirror";
        public const string ButtonMDS6 = "Button_MirrorDungeon_S6"; // Season 6 Banner?

        public const string MDDungeonProgress = "MD_DungeonProgress";
        public const string MDStarlightCount = "MD_StarlightCount";
        public const string MDStarlightIcon = "MD_StarlightIcon";

        // --- Charge Menu ---
        // ChargeLabel seemingly removed/missing. We will detect via tabs.
        // public const string ChargeLabel = "Charge_Label"; 
        
        public const string ButtonChargeConfirm = ButtonConfirm; // Map to generic
        public const string ButtonChargeCancel = ButtonCancel;   // Map to generic
        
        // Active/InActive Tabs for Charge
        public const string ButtonActiveChargeBoxes = "Button_A_Charge_Boxes";
        public const string ButtonInActiveChargeBoxes = "Button_I_Charge_Boxes";
        
        public const string ButtonActiveChargeModules = "Button_A_Charge_Modules";
        public const string ButtonInActiveChargeModules = "Button_I_Charge_Modules";
        
        public const string ButtonActiveChargeLunacy = "Button_A_Charge_Lunacy";
        public const string ButtonInActiveChargeLunacy = "Button_I_Charge_Lunacy";
        
        // Legacy/Generic (Can map to InActive for clicking if needed)
        public const string ChargeBoxes = ButtonInActiveChargeBoxes;
        public const string ChargeModules = ButtonInActiveChargeModules;
        public const string ChargeLunacy = ButtonInActiveChargeLunacy;

        public const string EnkephalinBox = "Icon_EnkehpalinBox"; // Note: Typo in filename 'Enkehpalin'

        public const string NumberDialTransparent = "NumberDialTransparent";
        public const string NumberDialTransparentDecrement = "NumberDialTransparent_Decrement";
        public const string NumberDialTransparentIncrement = "NumberDialTransparent_Increment";
        public const string NumberDialTransparentMaximize = "NumberDialTransparent_Maximize";
        public const string NumberDialTransparentMinimize = "NumberDialTransparent_Minimize";
    }
}