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

        public bool HumanLikeMovement
        {
            get => _clicker.HumanLikeMovement;
            set => _clicker.HumanLikeMovement = value;
        }

        public bool AutoClearCursor
        {
            get => _clicker.AutoClearCursor;
            set => _clicker.AutoClearCursor = value;
        }

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
                case NavigationState.Charge_Modules:
                case NavigationState.Charge_Lunacy:
                    return NavigateFromCharge(target);

                 case NavigationState.Luxcavation_EXP:
                 case NavigationState.Luxcavation_Thread:
                    return NavigateFromLuxcavation(target);

                 case NavigationState.MirrorDungeon:
                 case NavigationState.MirrorDungeon_Delving:
                    return NavigateFromMirrorDungeon(target);

                 case NavigationState.MirrorDungeon_Confirmation:
                    return NavigateFromMirrorDungeonConfirmation(target);
                    
                 case NavigationState.ToBattle:
                    return NavigateFromToBattle(target);
                    
                 // case NavigationState.Battle: // TODO
            }

            return false;
        }

        public NavigationState SynchronizeState()
        {
            // Optional: Move mouse away to avoid obstructing vision
            if (AutoClearCursor)
            {
                 _clicker.ClearCursor();
                 Thread.Sleep(50); 
            }

            // Hybrid Approach: 
            // - Use COLOR (Bgr) for Active/Inactive tab differentiation (Luxcavation, Charge Sub-tabs)
            // - Use GRAYSCALE for Main Menu buttons (Window, Drive, Sinners) as they rely on shape/contrast and user legacy assets are gray.
            
            using var screenColor = _vision.CaptureRegion(_vision.ScreenResolution);
            using var screenGray = screenColor.Convert<Gray, byte>();

            // CRITICAL: Check OVERLAY menus first. 

            // 1. Charge Menu (Overlay)
            // ChargeLabel is missing. Check sub-tabs directly.
            // Using Color for Charge tabs as Gray is ambiguous (Active vs Inactive).
            if (!_locator.Find(screenColor, NavigationAssets.ButtonActiveChargeModules).IsEmpty)
                _currentState = NavigationState.Charge_Modules;
            else if (!_locator.Find(screenColor, NavigationAssets.ButtonActiveChargeLunacy).IsEmpty)
                _currentState = NavigationState.Charge_Lunacy;
            
            // 2. Mirror Dungeon Popups (Overlay)
            else if (!_locator.Find(screenGray, NavigationAssets.MDDungeonProgress).IsEmpty)
            {
                _currentState = NavigationState.MirrorDungeon_Delving;
            }
            // Confirmation Overlay must be checked before Main Screen
            else if (!_locator.Find(screenGray, NavigationAssets.TextMirrorDungeonConfirmation).IsEmpty)
            {
                _currentState = NavigationState.MirrorDungeon_Confirmation;
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
            // Mirror Dungeon Main State (Rental Button Identifier)
            else if (!_locator.Find(screenGray, NavigationAssets.ButtonMirrorDungeonRental).IsEmpty)
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
                     if (ClickTransition(NavigationAssets.ButtonMDS6, NavigationState.MirrorDungeon))
                     {
                         return NavigateTo(target);
                     }
                     return false;
            }
            return false;
        }

        private bool NavigateFromCharge(NavigationState target)
        {
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
            if (target == NavigationState.Charge || target == NavigationState.Charge_Modules || target == NavigationState.Charge_Lunacy)
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
            {
                if (ClickTransition(NavigationAssets.ButtonInActiveLuxcavationThread, NavigationState.Luxcavation_Thread))
                {
                    return true;
                }
                // If already active or just switched
                if (_currentState == NavigationState.Luxcavation_Thread)
                {
                   // Enter Logic for Thread
                   // Assuming we want to enter battle? (Usually implies ToBattle target, but if target is just 'Thread' we stay?)
                   // Wait, if target is ToBattle, we need to choose based on CURRENT state (EXP vs Thread).
                }
                return true; 
            }

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
            
            // To Delving (Start)
            if (target == NavigationState.MirrorDungeon_Confirmation || target == NavigationState.MirrorDungeon_Delving)
            {
                 // Click Enter to start (or resume)
                 // This takes us to Confirmation screen usually
                 if (ClickTransition(NavigationAssets.ButtonMDEnter, NavigationState.MirrorDungeon_Confirmation))
                 {
                      if (target == NavigationState.MirrorDungeon_Confirmation) return true;
                      return NavigateTo(target); // Chain loop will handle Confirmation -> Delving
                 }
            }
            return false;
        }

        private bool NavigateFromMirrorDungeonConfirmation(NavigationState target)
        {
            if (target == NavigationState.MirrorDungeon)
            {
                 return ClickTransition(NavigationAssets.ButtonCancel, NavigationState.MirrorDungeon);
            }

            if (target == NavigationState.MirrorDungeon_Delving)
            {
                if (ClickTransition(NavigationAssets.ButtonEnter, NavigationState.MirrorDungeon_Delving))
                {
                    // Potentially wait for loading?
                    Thread.Sleep(2000); 
                    return true;
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
            
            // Going Back from ToBattle -> Luxcavation or Drive
            if (target == NavigationState.Luxcavation_EXP || target == NavigationState.Luxcavation_Thread || 
                target == NavigationState.Drive || target == NavigationState.Window)
            {
                // Try physical back button first (More reliable than ESC in some states)
                if (ClickTransition(NavigationAssets.ButtonBackLuxcavation, NavigationState.Luxcavation_EXP))
                {
                    // If we reached Luxcavation, and we want to go further to Drive/Window, chain it.
                    if (target == NavigationState.Drive || target == NavigationState.Window)
                    {
                        return NavigateTo(target);
                    }
                    return true;
                }

                // Fallback to ESC
                _clicker.DismissWithEsc();
                Thread.Sleep(1000);
                var newState = SynchronizeState();
                if (newState == target || (target == NavigationState.Drive && newState == NavigationState.Luxcavation_EXP))
                {
                     if (target == NavigationState.Drive) return NavigateTo(NavigationState.Drive);
                     return true;
                }
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
            // Dispatch based on active tab
            if (_currentState == NavigationState.Luxcavation_Thread)
            {
                return EnterLuxcavationThreadLevel();
            }
            
            // Default to EXP Logic
            return EnterLuxcavationEXPLevel();
        }

        private bool EnterLuxcavationEXPLevel()
        {
            // Scanning Levels: 9 down to 1
            // Simple Scan First (What is visible?)
            using var screen = _vision.CaptureRegion(_vision.ScreenResolution);
            
            // Strategy: Find ALL Enter buttons. Sort by X Descending. Pick the first one.
            // This assumes the "Enter" button we want is the right-most.
            var allButtons = new List<System.Drawing.Rectangle>();
            
            allButtons.AddRange(_locator.FindAll(screen, NavigationAssets.ButtonLuxcavationEnter3, 0.85));
            allButtons.AddRange(_locator.FindAll(screen, NavigationAssets.ButtonLuxcavationEnter2, 0.85));

            if (allButtons.Count > 0)
            {
                // Sort Descending by X (Rightmost first)
                allButtons.Sort((a, b) => b.X.CompareTo(a.X));
                
                var bestBtn = allButtons[0];
                Console.WriteLine($"[Luxcavation EXP] Clicking Rightmost Enter Button at {bestBtn}");
                return _clicker.ClickLocation(bestBtn);
            }
            
            Console.WriteLine("[Luxcavation EXP] No Enter buttons found.");
            return false;
        }

        private bool EnterLuxcavationThreadLevel()
        {
            // 1. Find Leftmost Enter Button (Button_LuxcavationThread_Enter)
            using var screen = _vision.CaptureRegion(_vision.ScreenResolution);
            var enterButtons = _locator.FindAll(screen, NavigationAssets.ButtonLuxcavationThreadEnter, 0.85);
            
            if (enterButtons.Count == 0)
            {
                Console.WriteLine("[Luxcavation Thread] No Thread Enter buttons found.");
                return false;
            }

            // Sort Ascending by X (Leftmost first)
            enterButtons.Sort((a, b) => a.X.CompareTo(b.X));
            var bestEnter = enterButtons[0];
            Console.WriteLine($"[Luxcavation Thread] Clicking Leftmost Enter Button at {bestEnter}");
            
            if (!_clicker.ClickLocation(bestEnter)) return false;
            
            // 2. Wait for Popup
            Thread.Sleep(1500); // Wait for animation
            using var screenPopup = _vision.CaptureRegion(_vision.ScreenResolution);
            
            // 3. Scan for Highest Level Text
            string[] threadLevels = {
                NavigationAssets.TextLuxcavationThreadLevel60,
                NavigationAssets.TextLuxcavationThreadLevel50,
                NavigationAssets.TextLuxcavationThreadLevel40,
                NavigationAssets.TextLuxcavationThreadLevel30,
                NavigationAssets.TextLuxcavationThreadLevel20
            };

            foreach (var level in threadLevels)
            {
                var levelRect = _locator.Find(screenPopup, level, 0.85);
                if (levelRect != System.Drawing.Rectangle.Empty)
                {
                    Console.WriteLine($"[Luxcavation Thread] Found Level: {level} at {levelRect}");
                    
                    // 4. Find matching "Enter" button in the popup
                    var popupButtons = _locator.FindAll(screenPopup, NavigationAssets.ButtonLuxcavationThreadLevelEnter, 0.85);
                    
                    // Filter: Must be roughly on the same Y-plane (within ~20px)
                    var targetButton = popupButtons
                        .OrderBy(b => Math.Abs(b.Y - levelRect.Y))
                        .FirstOrDefault(b => Math.Abs(b.Y - levelRect.Y) < 50); // Generous Y-checking
                         
                    if (targetButton != System.Drawing.Rectangle.Empty)
                    {
                         Console.WriteLine($"[Luxcavation Thread] Clicking Popup Enter at {targetButton}");
                         return _clicker.ClickLocation(targetButton);
                    }
                }
            }
            
            Console.WriteLine("[Luxcavation Thread] Could not identify any valid level in popup.");
            return false;

            System.Drawing.Rectangle bestLevelRect = System.Drawing.Rectangle.Empty;
            string foundLevelAsset = "";

            foreach(var lvl in threadLevels)
            {
                var rect = _locator.Find(screenPopup, lvl, 0.85);
                if (!rect.IsEmpty)
                {
                    bestLevelRect = rect;
                    foundLevelAsset = lvl;
                    Console.WriteLine($"[Luxcavation Thread] Found Highest Level: {lvl}");
                    break; // Found highest
                }
            }
            
            if (bestLevelRect.IsEmpty)
            {
                Console.WriteLine("[Luxcavation Thread] No level text found in popup.");
                // Fallback: Maybe just find the 'Enter' button directly if layout is simple?
                // But let's stick to requirement.
                return false;
            }

            // 4. Find Button_LuxcavationThread_Level_Enter near the text
            // They are likely in the same row or grouped. 
            // Note: FindAll might return multiple if list is long. We want the one closest to our text.
            var levelEnterButtons = _locator.FindAll(screenPopup, NavigationAssets.ButtonLuxcavationThreadLevelEnter, 0.85);
            
            System.Drawing.Rectangle bestLevelEnter = System.Drawing.Rectangle.Empty;
            double minDistance = double.MaxValue;
            int textMidY = bestLevelRect.Y + bestLevelRect.Height / 2;

            foreach(var btn in levelEnterButtons)
            {
                int btnMidY = btn.Y + btn.Height / 2;
                // Vertical alignment check
                if (Math.Abs(btnMidY - textMidY) < 50) 
                {
                    // Check horizontal proximity? Or just take it.
                    double dist = Math.Abs(btn.X - bestLevelRect.X) + Math.Abs(btn.Y - bestLevelRect.Y);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        bestLevelEnter = btn;
                    }
                }
            }

            if (!bestLevelEnter.IsEmpty)
            {
                Console.WriteLine($"[Luxcavation Thread] Clicking Level Enter for {foundLevelAsset} at {bestLevelEnter}");
                return _clicker.ClickLocation(bestLevelEnter);
            }

            Console.WriteLine("[Luxcavation Thread] Found level text but no matching Enter button.");
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