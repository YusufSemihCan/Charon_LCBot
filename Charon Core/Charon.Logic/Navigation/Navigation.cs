using Charon.Input;
using Charon.Vision;
using System.Threading;
using System;
using Emgu.CV;
using Emgu.CV.Structure;

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
                    
                 case NavigationState.ToBattle:
                    return NavigateFromToBattle(target);
                    
                 // case NavigationState.Battle: // TODO
            }

            return false;
        }

        public NavigationState SynchronizeState()
        {
            // Hybrid Approach: 
            // - Use COLOR (Bgr) for Active/Inactive tab differentiation (Luxcavation, Charge Sub-tabs)
            // - Use GRAYSCALE for Main Menu buttons (Window, Drive, Sinners) as they rely on shape/contrast and user legacy assets are gray.
            
            using var screenColor = _vision.CaptureRegion(_vision.ScreenResolution);
            using var screenGray = screenColor.Convert<Gray, byte>();

            // CRITICAL: Check OVERLAY menus first. 

            // 1. Charge Menu (Overlay)
            // ChargeLabel is missing. Check sub-tabs directly.
            // Using Color for Active tabs as they are distinct.
            if (!_locator.Find(screenColor, NavigationAssets.ButtonActiveChargeBoxes).IsEmpty)
                _currentState = NavigationState.Charge_Boxes;
            else if (!_locator.Find(screenColor, NavigationAssets.ButtonActiveChargeModules).IsEmpty)
                _currentState = NavigationState.Charge_Modules;
            else if (!_locator.Find(screenColor, NavigationAssets.ButtonActiveChargeLunacy).IsEmpty)
                _currentState = NavigationState.Charge_Lunacy;
            
            // 2. Mirror Dungeon Popups (Overlay)
            else if (!_locator.Find(screenGray, NavigationAssets.MDDungeonProgress).IsEmpty)
            {
                _currentState = NavigationState.MirrorDungeon_Delving;
            }
            // CRITICAL USER REQUEST: Prioritize Sinners Active Button over ToBattle Identifier.
            // ToBattle identifier might be visible in Sinners screen.
            else if (!_locator.Find(screenGray, NavigationAssets.ButtonActiveSinners).IsEmpty)
                _currentState = NavigationState.Sinners;

            // 3. ToBattle (Pre-Battle Screen)
            else if (!_locator.Find(screenColor, NavigationAssets.IconToBattle).IsEmpty)
            {
                _currentState = NavigationState.ToBattle;
            }
            else if (!_locator.Find(screenGray, NavigationAssets.ButtonMDInfinityMirror).IsEmpty)
            {
                _currentState = NavigationState.MirrorDungeon;
            }
            // 3. Luxcavation (Overlay/Fullscreen)
            // Prioritize Active Buttons (Color distinct)
            else if (!_locator.Find(screenColor, NavigationAssets.ButtonActiveLuxcavationEXP).IsEmpty)
                _currentState = NavigationState.Luxcavation_EXP;
            else if (!_locator.Find(screenColor, NavigationAssets.ButtonActiveLuxcavationThread).IsEmpty)
                _currentState = NavigationState.Luxcavation_Thread;
            
            // Text Fallback (New Assets)
            else if (!_locator.Find(screenColor, NavigationAssets.TextLuxcavationEXP).IsEmpty)
                 _currentState = NavigationState.Luxcavation_EXP;
            else if (!_locator.Find(screenColor, NavigationAssets.TextLuxcavationThread).IsEmpty)
                 _currentState = NavigationState.Luxcavation_Thread;

            // 4. Main Zones (Background)
            // Use GRAYSCALE as requested for shape-based matching (Drive, Window, Sinners)
            // Checked LAST because their anchors might be visible 'behind' overlays.
            else if (!_locator.Find(screenGray, NavigationAssets.ButtonActiveWindow).IsEmpty)
                _currentState = NavigationState.Window;
            else if (!_locator.Find(screenGray, NavigationAssets.ButtonActiveDrive).IsEmpty 
                  || !_locator.Find(screenGray, NavigationAssets.ButtonTextDrive).IsEmpty) // Fallback to Text
                _currentState = NavigationState.Drive;
            // Sinners Moved Up
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
                    return ClickTransition(NavigationAssets.EnkephalinBox, NavigationState.Charge_Modules);

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
                    return ClickTransition(NavigationAssets.EnkephalinBox, NavigationState.Charge_Modules);
                    
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
                    return ClickTransition(NavigationAssets.EnkephalinBox, NavigationState.Charge_Modules);
                
                case NavigationState.Luxcavation_EXP:
                case NavigationState.Luxcavation_Thread:
                    // Enter Luxcavation first (Defaults to EXP)
                    if (ClickTransition(NavigationAssets.ButtonLuxcavation, NavigationState.Luxcavation_EXP))
                    {
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
                return ClickTransition(NavigationAssets.ButtonInActiveChargeBoxes, NavigationState.Charge_Boxes);
            
            if (target == NavigationState.Charge_Modules)
                return ClickTransition(NavigationAssets.ButtonInActiveChargeModules, NavigationState.Charge_Modules);

            if (target == NavigationState.Charge_Lunacy)
                return ClickTransition(NavigationAssets.ButtonInActiveChargeLunacy, NavigationState.Charge_Lunacy);

            // If we are trying to leave Charge? 
            // We can return to potential Previous States: Window, Drive, Sinners, Luxcavation
            if (target == NavigationState.Window || target == NavigationState.Drive || target == NavigationState.Sinners || 
                target == NavigationState.Luxcavation_EXP || target == NavigationState.Luxcavation_Thread)
            {
                 // Method: 1. Try Cancel Button 2. Try ESC
                 // "Get us back into previous state which we opened charge from" means we assume target IS that state.
                 // We rely on SynchronizeState to tell us where we landed.

                 // 1. Try Cancel
                 if (_clicker.ClickTemplate(NavigationAssets.ButtonChargeCancel))
                 {
                      Thread.Sleep(500);
                      if (SynchronizeState() == target) return true;
                      // If Cancel led elsewhere, we continue recursive navigate or return false? 
                      // If we are out of Charge, let's return recursive to fixpath.
                      if (!_currentState.ToString().StartsWith("Charge")) return NavigateTo(target);
                 }

                 // 2. Try ESC (Fallback)
                 _clicker.DismissWithEsc();
                 Thread.Sleep(500);
                 SynchronizeState();
                 if (_currentState == target) return true;
                 if (!_currentState.ToString().StartsWith("Charge")) return NavigateTo(target);
            }

            return false;
        }

        private bool NavigateFromLuxcavation(NavigationState target)
        {
            if (target == NavigationState.ToBattle)
            {
                // Enter Best Level
                return EnterLuxcavationLevel();
            }

            // Direct access to Charge via EnkephalinBox (available in Luxcavation too)
            if (target == NavigationState.Charge || target == NavigationState.Charge_Boxes || target == NavigationState.Charge_Modules || target == NavigationState.Charge_Lunacy)
            {
                 // Rule: Luxcavation -> Charge allowed via Enkephalin.
                 // We don't know exact sub-state we land on (Boxes vs Modules), but Rule allows 'Charge'.
                 // Let's assume 'Charge' is the representative state for entering the menu.
                 if (ClickTransition(NavigationAssets.EnkephalinBox, NavigationState.Charge_Modules))
                 {
                      return NavigateTo(target);
                 }
            }
            // Leaving Luxcavation
            if (target == NavigationState.Drive || target == NavigationState.Window)
            {
                // Prioritize ESC as requested
                _clicker.DismissWithEsc();
                Thread.Sleep(1000); // Wait for transition
                if (SynchronizeState() == NavigationState.Drive) return NavigateTo(target);

                // Fallback: Physical Back Button (if ESC failed to move detection)
                if (ClickTransition(NavigationAssets.ButtonBackLuxcavation, NavigationState.Drive))
                    return NavigateTo(target);
            }

            // Standard Toggle between EXP/Thread using INACTIVE buttons
            if (target == NavigationState.Luxcavation_EXP)
                return ClickTransition(NavigationAssets.ButtonInActiveLuxcavationEXP, NavigationState.Luxcavation_EXP);
            
            if (target == NavigationState.Luxcavation_Thread)
                return ClickTransition(NavigationAssets.ButtonInActiveLuxcavationThread, NavigationState.Luxcavation_Thread);

            // Chain Navigation: If target is not local, go back to Drive to find path
            if (ClickTransition(NavigationAssets.ButtonBackLuxcavation, NavigationState.Drive))
            {
                return NavigateTo(target);
            }

            return false;
        }

        private bool NavigateFromMirrorDungeon(NavigationState target)
        {
            if (target == NavigationState.Drive)
                 return ClickTransition(NavigationAssets.ButtonBackMirrorDungeon, NavigationState.Drive);
            
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

        // --- Missing Methods Re-Added ---

        /// <summary>
        /// Handles navigation from the 'ToBattle' (Pre-Battle) state.
        /// </summary>
        /// <param name="target">The target state (Battle or Back to Luxcavation).</param>
        private bool NavigateFromToBattle(NavigationState target)
        {
            if (target == NavigationState.Battle)
            {
                // Press 'Enter' key to start battle
                _input.PressKey(VirtualKey.ENTER);
                Thread.Sleep(2000); // Wait for load
                // We should eventually detect 'Battle' state
                return true; 
            }
            
            // Going Back from ToBattle -> Luxcavation
            if (target == NavigationState.Luxcavation_EXP || target == NavigationState.Luxcavation_Thread)
            {
                // User prefers ESC
                _clicker.DismissWithEsc();
                Thread.Sleep(1000);
                return SynchronizeState() == target;
            }
            
            return false;
        }

        /// <summary>
        /// Scans for the highest available Luxcavation EXP level (9 down to 1).
        /// Identifies the correct 'Enter' button by matching its row (Y-coordinate) with the level text.
        /// </summary>
        /// <returns>True if a level was found and the enter button was clicked; otherwise false.</returns>
        private bool EnterLuxcavationLevel()
        {
            // Scanning Levels: 9 down to 1
            // Simple Scan First (What is visible?)
            using var screen = _vision.CaptureRegion(_vision.ScreenResolution);
            
            // Levels to check (High to Low)
            string[] levels = {
                NavigationAssets.TextLuxcavationLevel9,
                NavigationAssets.TextLuxcavationLevel8,
                NavigationAssets.TextLuxcavationLevel7,
                NavigationAssets.TextLuxcavationLevel6,
                NavigationAssets.TextLuxcavationLevel5,
                NavigationAssets.TextLuxcavationLevel4,
                NavigationAssets.TextLuxcavationLevel3,
                NavigationAssets.TextLuxcavationLevel2,
                NavigationAssets.TextLuxcavationLevel1
            };

            System.Drawing.Rectangle bestLevelRect = System.Drawing.Rectangle.Empty;
            string foundLevel = "";

            foreach (var levelAsset in levels)
            {
                var rect = _locator.Find(screen, levelAsset, 0.85); // High confidence for text
                if (!rect.IsEmpty)
                {
                    bestLevelRect = rect;
                    foundLevel = levelAsset;
                    break; // Found highest visible
                }
            }

            if (bestLevelRect.IsEmpty)
            {
                // TODO: Scroll logic if nothing high is found.
                return false;
            }

            // Find ALL Enter Buttons on screen (both variants)
            var allButtons = new List<System.Drawing.Rectangle>();
            allButtons.AddRange(_locator.FindAll(screen, NavigationAssets.ButtonLuxcavationEnter2, 0.85));
            allButtons.AddRange(_locator.FindAll(screen, NavigationAssets.ButtonLuxcavationEnter3, 0.85));

            // Filter for the button that matches the Text's ROW.
            // Text Height ~25px. We allow a margin.
            int textMidY = bestLevelRect.Y + bestLevelRect.Height / 2;
            int toleranceY = 25; // +/- 25px vertical tolerance

            System.Drawing.Rectangle bestBtn = System.Drawing.Rectangle.Empty;
            double minDistanceX = double.MaxValue;

            foreach (var btn in allButtons)
            {
                int btnMidY = btn.Y + btn.Height / 2;
                if (Math.Abs(btnMidY - textMidY) < toleranceY)
                {
                    // This button is in the same row.
                    // Pick the closest one X-wise (usually there's only one to the right, but just in case)
                    // We typically expect Button X > Text X
                    double dist = Math.Abs(btn.X - bestLevelRect.X);
                    if (dist < minDistanceX)
                    {
                        minDistanceX = dist;
                        bestBtn = btn;
                    }
                }
            }
            
            if (!bestBtn.IsEmpty)
            {
                Console.WriteLine($"[Luxcavation] Found Level {foundLevel}. Clicking Enter at {bestBtn} (Row Match).");
                return _clicker.ClickLocation(bestBtn);
            }
            else
            {
                Console.WriteLine($"[Luxcavation] Found Text for {foundLevel} but NO Enter button in Row (Y~{textMidY}). Found {allButtons.Count} buttons total on screen.");
            }

            return false;
        }

        // --- Helpers ---

        private bool ClickTransition(string template, NavigationState expectedNextState)
        {
            // SAFETY: Enforce the NavigationRules graph here.
            // If the current state is known, we must ensure moving to 'expectedNextState' is allowed.
            if (_currentState != NavigationState.Unknown && !NavigationRules.CanTransition(_currentState, expectedNextState))
            {
                // This means the code is trying to do a move that is NOT defined in NavigationRules.cs
                Console.WriteLine($"[BLOCKED] Logic attempted illegal transition: {_currentState} -> {expectedNextState}");
                return false;
            }

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