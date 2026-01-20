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
        // --- State Anchors (Used to verify looking at a screen) ---
        public const string ButtonActiveWindow = "Button_Active_Window";
        public const string ButtonActiveDrive = "Button_Active_Drive";
        public const string ButtonActiveSinners = "Button_Active_Sinners";
        
        public const string ButtonInActiveWindow = "Button_InActive_Window";
        public const string ButtonInActiveDrive = "Button_InActive_Drive";
        public const string ButtonInActiveSinners = "Button_InActive_Sinners";

        // --- Common UI ---
        public const string ButtonBack = "Button_Back";
        public const string ButtonCancel = "Button_Cancel";
        public const string ButtonHalt = "Button_Halt"; // Stop/Pause
        public const string ButtonResume = "Button_Resume";
        
        public const string ButtonTextCancel = "Button_Text_Cancel";
        public const string ButtonTextHalt = "Button_Text_Halt";
        public const string ButtonTextResume = "Button_Text_Resume";

        // --- Drive / ID Entry ---
        public const string ButtonLuxcavation = "Button_Luxcavation";
        public const string ButtonTextLuxcavation = "Button_Text_Luxcavation";
        
        // Luxcavation Tabs
        public const string ButtonActiveLuxcavationEXP = "Button_Active_LuxcavationEXP";
        public const string ButtonInActiveLuxcavationEXP = "Button_InActive_LuxcavationEXP";
        
        public const string ButtonActiveLuxcavationThread = "Button_Active_LuxcavationThread";
        public const string ButtonInActiveLuxcavationThread = "Button_InActive_LuxcavationThread";
        
        // Panels for robust state detection
        public const string PanelLuxcavationEXP = "Panel_LuxcavationEXP";
        public const string PanelLuxcavationThread = "Panel_LuxcavationThread";
        
        // Legacy keys mapped for compatibility (or update logic to use specific ones)
        // Used for Clicking: We typically click the inactive one to switch.
        public const string ButtonLuxcavationEXP = "Button_InActive_LuxcavationEXP";       
        public const string ButtonLuxcavationThread = "Button_InActive_LuxcavationThread";
        
        public const string ButtonMirrorDungeon = "Button_MirrorDungeon";
        public const string ButtonMDEnter = "Button_MD_Enter";
        public const string ButtonTextMDEnter = "Button_Text_MD_Enter";
        public const string ButtonTextMD = "Button_Text_MD";
        
        public const string ButtonMDInfinityMirror = "Button_MD_InfinityMirror";
        public const string ButtonTextMDInfinityMirror = "Button_Text_MD_InfinityMirror";
        public const string ButtonMDS6 = "Button_MD_S6"; // Season 6 Banner?

        public const string MDDungeonProgress = "MD_DungeonProgress";
        public const string MDStarlightCount = "MD_StarlightCount";
        public const string MDStarlightIcon = "MD_StarlightIcon";

        // --- Charge Menu ---
        public const string ChargeLabel = "Charge_Label";
        public const string ButtonChargeConfirm = "Button_Charge_Confirm";
        public const string ButtonChargeCancel = "Button_Charge_Cancel";
        
        public const string ChargeBoxesWindow = "Charge_Boxes_Window";
        public const string ChargeModulesWindow = "Charge_Modules_Window";
        public const string ChargeLunacyWindow = "Charge_Lunacy_Window";
        
        // Active/InActive Tabs for Charge
        public const string ButtonActiveChargeBoxes = "Button_Active_Charge_Boxes";
        public const string ButtonInActiveChargeBoxes = "Button_InActive_Charge_Boxes";
        
        public const string ButtonActiveChargeModules = "Button_Active_Charge_Modules";
        public const string ButtonInActiveChargeModules = "Button_InActive_Charge_Modules";
        
        public const string ButtonActiveChargeLunacy = "Button_Active_Charge_Lunacy";
        public const string ButtonInActiveChargeLunacy = "Button_InActive_Charge_Lunacy";
        
        // Legacy/Generic (Can map to InActive for clicking if needed)
        public const string ChargeBoxes = ButtonInActiveChargeBoxes;
        public const string ChargeModules = ButtonInActiveChargeModules;
        public const string ChargeLunacy = ButtonInActiveChargeLunacy;

        public const string Enkephalin = "Enkephalin";
        public const string EnkephalinBox = "EnkephalinBox";
        public const string EnkephalinNumbers = "Enkephalin_Numbers";

        // --- Numbers / Dials ---
        public const string NumberDial = "NumberDial";
        public const string NumberDialDecrement = "NumberDial_Decrement";
        public const string NumberDialIncrement = "NumberDial_Increment";
        public const string NumberDialMaximize = "NumberDial_Maximize";
        public const string NumberDialMinimize = "NumberDial_Minimize";
        
        public const string NumberDialTransparent = "NumberDialTransparent";
        public const string NumberDialTransparentDecrement = "NumberDialTransparent_Decrement";
        public const string NumberDialTransparentIncrement = "NumberDialTransparent_Increment";
        public const string NumberDialTransparentMaximize = "NumberDialTransparent_Maximize";
        public const string NumberDialTransparentMinimize = "NumberDialTransparent_Minimize";

        // --- Legacy / To Be Cleaned Up (Kept for compatibility if code relies on them) ---
        // Ideally mapped to new ones if names match
        public const string BtnEnterGame = "Btn_Enter_Game"; // Not in list? Keeping for safety
        public const string BtnRetryConnection = "Btn_Retry_Connection"; // Not in list? Keeping
    }
}