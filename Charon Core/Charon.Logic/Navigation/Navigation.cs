using Charon.Input;
using Charon.Vision;
using System.Threading;
using System;

using Charon.Logic.Combat;

namespace Charon.Logic.Navigation
{
    public class Navigation : INavigation
    {
        private readonly INavigationClicker _clicker;
        private readonly ICombatClicker _combat;
        private readonly IVisionService _vision;
        private readonly IVisionLocator _locator;
        private readonly IInputService _input;
        private NavigationState _currentState = NavigationState.Unknown;

        public NavigationState CurrentState => _currentState;

        public Navigation(INavigationClicker clicker, ICombatClicker combat, IVisionService vision, IVisionLocator locator, IInputService input)
        {
            _clicker = clicker ?? throw new ArgumentNullException(nameof(clicker));
            _combat = combat ?? throw new ArgumentNullException(nameof(combat));
            _vision = vision ?? throw new ArgumentNullException(nameof(vision));
            _locator = locator ?? throw new ArgumentNullException(nameof(locator));
            _input = input ?? throw new ArgumentNullException(nameof(input));
        }

        public bool NavigateTo(NavigationState target)
        {
            _input.CheckFailSafe();

            // 1. Identify where we are
            SynchronizeState();
            
            if (_currentState == target) return true;

            // Navigation Tree Logic
            switch (_currentState)
            {
                case NavigationState.Window:
                    return NavigateFromWindow(target);

                case NavigationState.Sinners:
                    return NavigateFromSinners(target);

                case NavigationState.Drive:
                    return NavigateFromDrive(target);

                case NavigationState.Charge:
                case NavigationState.Charge_Boxes:
                case NavigationState.Charge_Modules:
                case NavigationState.Charge_Lunacy:
                    return NavigateFromCharge(target);

                 case NavigationState.Luxcavation_EXP:
                 case NavigationState.Luxcavation_Thread:
                    return NavigateFromLuxcavation(target);

                 case NavigationState.MirrorDungeon:
                 case NavigationState.MirrorDungeon_Delving:
                    return NavigateFromMirrorDungeon(target);
            }

            return false;
        }

        public NavigationState SynchronizeState()
        {
            using var screen = _vision.CaptureRegionGray(_vision.ScreenResolution);

            // Anchors define our state
            if (!_locator.Find(screen, NavigationAssets.ButtonActiveWindow).IsEmpty)
                _currentState = NavigationState.Window;
            else if (!_locator.Find(screen, NavigationAssets.ButtonActiveDrive).IsEmpty)
                _currentState = NavigationState.Drive;
            else if (!_locator.Find(screen, NavigationAssets.ButtonActiveSinners).IsEmpty)
                _currentState = NavigationState.Sinners;
            else if (!_locator.Find(screen, NavigationAssets.ChargeLabel).IsEmpty)
            {
                // We are in some Charge state, check specific sub-tabs using Active Buttons
                if (!_locator.Find(screen, NavigationAssets.ButtonActiveChargeBoxes).IsEmpty)
                    _currentState = NavigationState.Charge_Boxes;
                else if (!_locator.Find(screen, NavigationAssets.ButtonActiveChargeModules).IsEmpty)
                    _currentState = NavigationState.Charge_Modules;
                else if (!_locator.Find(screen, NavigationAssets.ButtonActiveChargeLunacy).IsEmpty)
                    _currentState = NavigationState.Charge_Lunacy;
                else
                    _currentState = NavigationState.Charge; // Default/Parent
            }
            else if (!_locator.Find(screen, NavigationAssets.ButtonTextLuxcavation).IsEmpty)
            {
                 // Use Panels for robust detection as requested
                 if (!_locator.Find(screen, NavigationAssets.PanelLuxcavationEXP).IsEmpty)
                    _currentState = NavigationState.Luxcavation_EXP;
                 else if (!_locator.Find(screen, NavigationAssets.PanelLuxcavationThread).IsEmpty)
                    _currentState = NavigationState.Luxcavation_Thread;
                 // Fallback to Active buttons if panels fail (redundancy)
                 else if (!_locator.Find(screen, NavigationAssets.ButtonActiveLuxcavationEXP).IsEmpty)
                    _currentState = NavigationState.Luxcavation_EXP;
                 else if (!_locator.Find(screen, NavigationAssets.ButtonActiveLuxcavationThread).IsEmpty)
                    _currentState = NavigationState.Luxcavation_Thread;
                 else
                    _currentState = NavigationState.Unknown; // Should default to one
            }
            else if (!_locator.Find(screen, NavigationAssets.ButtonTextMD).IsEmpty)
            {
                 if (!_locator.Find(screen, NavigationAssets.MDDungeonProgress).IsEmpty)
                    _currentState = NavigationState.MirrorDungeon_Delving;
                 else
                    _currentState = NavigationState.MirrorDungeon;
            }
            else
                _currentState = NavigationState.Unknown;

            return _currentState;
        }

        // --- State Specific Navigation Logic ---

        private bool NavigateFromWindow(NavigationState target)
        {
            switch (target)
            {
                case NavigationState.Drive:
                    return ClickTransition(NavigationAssets.ButtonInActiveDrive, NavigationState.Drive);
                case NavigationState.Sinners:
                    return ClickTransition(NavigationAssets.ButtonInActiveSinners, NavigationState.Sinners);
                case NavigationState.Charge:
                case NavigationState.Charge_Boxes:
                case NavigationState.Charge_Modules:
                case NavigationState.Charge_Lunacy:
                    // Charge is accessible from Window via EnkephalinBox (Window variant often same or distinct)
                    // If EnkephalinBox works:
                    return ClickTransition(NavigationAssets.EnkephalinBox, NavigationState.Charge);

                // Chain Nav: Go to Drive for these
                case NavigationState.Luxcavation_EXP:
                case NavigationState.Luxcavation_Thread:
                case NavigationState.MirrorDungeon:
                case NavigationState.MirrorDungeon_Delving:
                     if (ClickTransition(NavigationAssets.ButtonInActiveDrive, NavigationState.Drive))
                     {
                         return NavigateTo(target);
                     }
                     return false;
            }
            return false;
        }

        private bool NavigateFromSinners(NavigationState target)
        {
            switch (target)
            {
                // If we are in Sinners, we can use the main nav bar
                case NavigationState.Window:
                    return ClickTransition(NavigationAssets.ButtonInActiveWindow, NavigationState.Window);
                case NavigationState.Drive:
                    return ClickTransition(NavigationAssets.ButtonInActiveDrive, NavigationState.Drive);
                case NavigationState.Charge:
                case NavigationState.Charge_Boxes:
                case NavigationState.Charge_Modules:
                case NavigationState.Charge_Lunacy:
                    return ClickTransition(NavigationAssets.EnkephalinBox, NavigationState.Charge);
                    
                // Chain Nav: Go to Drive for these
                case NavigationState.Luxcavation_EXP:
                case NavigationState.Luxcavation_Thread:
                case NavigationState.MirrorDungeon:
                case NavigationState.MirrorDungeon_Delving:
                     if (ClickTransition(NavigationAssets.ButtonInActiveDrive, NavigationState.Drive))
                     {
                         return NavigateTo(target);
                     }
                     return false;
            }
            return false;
        }

        private bool NavigateFromDrive(NavigationState target)
        {
            switch (target)
            {
                // If we are in Drive, we can use the main nav bar
                case NavigationState.Window:
                    return ClickTransition(NavigationAssets.ButtonInActiveWindow, NavigationState.Window);
                case NavigationState.Sinners:
                    return ClickTransition(NavigationAssets.ButtonInActiveSinners, NavigationState.Sinners);
                case NavigationState.Charge:
                    return ClickTransition(NavigationAssets.EnkephalinBox, NavigationState.Charge);
                
                case NavigationState.Luxcavation_EXP:
                case NavigationState.Luxcavation_Thread:
                    // Enter Luxcavation first
                    if (_clicker.ClickTemplate(NavigationAssets.ButtonLuxcavation))
                    {
                         Thread.Sleep(500);
                         SynchronizeState();
                         if (_currentState == NavigationState.Luxcavation_EXP || _currentState == NavigationState.Luxcavation_Thread)
                             return NavigateTo(target);
                    }
                    return false;

                case NavigationState.MirrorDungeon:
                case NavigationState.MirrorDungeon_Delving:
                     if (ClickTransition(NavigationAssets.ButtonMirrorDungeon, NavigationState.MirrorDungeon))
                     {
                         return NavigateTo(target);
                     }
                     return false;
            }
            return false;
        }

        private bool NavigateFromCharge(NavigationState target)
        {
            // First, are we trying to switch sub-tabs?
            if (target == NavigationState.Charge_Boxes)
                return ClickTransition(NavigationAssets.ChargeBoxes, NavigationState.Charge_Boxes);
            
            if (target == NavigationState.Charge_Modules)
                return ClickTransition(NavigationAssets.ChargeModules, NavigationState.Charge_Modules);

            if (target == NavigationState.Charge_Lunacy)
                return ClickTransition(NavigationAssets.ChargeLunacy, NavigationState.Charge_Lunacy);

            // If we are trying to leave Charge? 
            // "Charge state which we cant get anywhere within it" implies we might be stuck or have to close it.
            // Usually there is a close button or we click one of the main nav buttons if they are visible.
            // Assuming Charge is an overlay:
            if (target == NavigationState.Window || target == NavigationState.Drive || target == NavigationState.Sinners)
            {
                // Try closing Charge first? Or if Nav bar is visible, click it.
                // If "Charge state which we cant get anywhere within it" means it blocks nav bar, 
                // we must close it. Usually 'ButtonCancel' or clicking outside.
                // NavigationAssets has 'ButtonChargeCancel'.
                if (_clicker.ClickTemplate(NavigationAssets.ButtonChargeCancel))
                {
                    Thread.Sleep(500);
                    SynchronizeState();
                    // Recursive call to navigate from where we landed (likely Window)
                    return NavigateTo(target);
                }
            }

            return false;
        }

        private bool NavigateFromLuxcavation(NavigationState target)
        {
            // Direct access to Charge via EnkephalinBox (available in Luxcavation too)
            if (target == NavigationState.Charge || target == NavigationState.Charge_Boxes || target == NavigationState.Charge_Modules || target == NavigationState.Charge_Lunacy)
            {
                 // Clicking EnkephalinBox usually opens Charge_Boxes (or last used?)
                 // We will land in a Charge state, then NavigateTo(target) ensures we get to correct tab.
                 if (_clicker.ClickTemplate(NavigationAssets.EnkephalinBox))
                 {
                     Thread.Sleep(500);
                     SynchronizeState();
                     // If we successfully entered any Charge state, we are good to proceed
                     if (_currentState.ToString().StartsWith("Charge"))
                        return NavigateTo(target);
                 }
            }

            // ESC Strategy: "Return to previous state"
            // Typically Luxcavation -> Drive
            if (target == NavigationState.Drive || target == NavigationState.Window || target == NavigationState.Sinners)
            {
                _clicker.DismissWithEsc();
                Thread.Sleep(500);
                var newState = SynchronizeState();
                
                // If we reached target, great.
                if (newState == target) return true;
                
                // If we reached Drive (from Lux), and target is Window/Sinners, we can recurse.
                if (newState == NavigationState.Drive) return NavigateTo(target);
                
                // If ESC failed to move us (stuck), try ButtonBack as fallback
                if (newState == NavigationState.Luxcavation_EXP || newState == NavigationState.Luxcavation_Thread)
                {
                     return ClickTransition(NavigationAssets.ButtonBack, NavigationState.Drive) && NavigateTo(target);
                }
            }

            if (target == NavigationState.Drive) // Fallback explicit
                return ClickTransition(NavigationAssets.ButtonBack, NavigationState.Drive);
            
            // Toggle between EXP/Thread using INACTIVE buttons
            if (target == NavigationState.Luxcavation_EXP)
                return ClickTransition(NavigationAssets.ButtonInActiveLuxcavationEXP, NavigationState.Luxcavation_EXP);
            
            if (target == NavigationState.Luxcavation_Thread)
                return ClickTransition(NavigationAssets.ButtonInActiveLuxcavationThread, NavigationState.Luxcavation_Thread);

            // Chain Navigation: If target is not local, go back to Drive to find path
            if (ClickTransition(NavigationAssets.ButtonBack, NavigationState.Drive))
            {
                return NavigateTo(target);
            }

            return false;
        }

        private bool NavigateFromMirrorDungeon(NavigationState target)
        {
            if (target == NavigationState.Drive)
                 return ClickTransition(NavigationAssets.ButtonBack, NavigationState.Drive);
            
            if (target == NavigationState.MirrorDungeon_Delving)
            {
                 // Click Enter to start (or resume)
                 if (_clicker.ClickTemplate(NavigationAssets.ButtonMDEnter))
                 {
                     Thread.Sleep(500); // Animation
                     SynchronizeState();
                     
                     // Case 1: Progress Popup appeared (Delving)
                     if (_currentState == NavigationState.MirrorDungeon_Delving) return true;
                     
                     // Case 2: Confirmation Popup appeared.
                     // The state might still look like 'MirrorDungeon' (parent) because progress isn't up yet.
                     // Or it might be 'Unknown' if popup obscures anchors.
                     // Try clicking Enter again (Confirm)
                     if (_clicker.ClickTemplate(NavigationAssets.ButtonMDEnter)) 
                     {
                         Thread.Sleep(500);
                         SynchronizeState();
                         if (_currentState == NavigationState.MirrorDungeon_Delving) return true;
                     }
                 }
            }

            return false;
        }

        // --- Helpers ---

        private bool ClickTransition(string template, NavigationState expectedNextState)
        {
            if (_clicker.ClickTemplate(template))
            {
                Thread.Sleep(500); // Wait for UI animation
                SynchronizeState(); // Verify new state
                
                // If we entered a sub-menu (like Charge) that might not have a main anchor, assume success if Unknown?
                // For now, strict check.
                if (_currentState == expectedNextState) return true;
                
                // Handle overlay cases (e.g. Charge might overlay Window)
                if (expectedNextState == NavigationState.Charge && _currentState != NavigationState.Unknown)
                {
                    _currentState = NavigationState.Charge; // Forced assumption for overlays
                    return true;
                }
            }
            return false;
        }
    }
}