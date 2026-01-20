using System.Collections.Generic;

namespace Charon.Logic.Navigation
{
    /// <summary>
    /// Defines the rules and graph for Navigation State Transitions.
    /// Acts as the central "Map" of where the bot can go from any given state.
    /// </summary>
    public static class NavigationRules
    {
        /// <summary>
        /// A dictionary mapping a Source State to a list of allowed Target States.
        /// This represents Direct or Single-Step logical transitions.
        /// </summary>
        public static readonly Dictionary<NavigationState, List<NavigationState>> AllowedTransitions = new()
        {
            // --- From Window ---
            { 
                NavigationState.Window, new List<NavigationState> 
                { 
                    NavigationState.Drive, 
                    NavigationState.Sinners,
                    NavigationState.Charge_Modules // Via Enkephalin
                } 
            },

            // --- From Drive ---
            {
                NavigationState.Drive, new List<NavigationState>
                {
                    NavigationState.Window,
                    NavigationState.Sinners,
                    NavigationState.Charge_Modules, // Via Enkephalin
                    NavigationState.Luxcavation_EXP,    // Enter Luxcavation (Default)
                    // NavigationState.Luxcavation_Thread - Removed, must go via EXP
                    NavigationState.MirrorDungeon       // Enter MD
                }
            },

            // --- From Sinners ---
            {
                NavigationState.Sinners, new List<NavigationState>
                {
                    NavigationState.Window,
                    NavigationState.Drive,
                    NavigationState.Charge_Modules
                }
            },

            // --- From Luxcavation (EXP) ---
            {
                NavigationState.Luxcavation_EXP, new List<NavigationState>
                {
                    NavigationState.Drive,             // Back
                    NavigationState.Luxcavation_Thread, // Toggle Tab
                    NavigationState.Charge_Modules,     // Enkephalin
                    NavigationState.ToBattle           // Enter Level
                }
            },

            // --- From Luxcavation (Thread) ---
            {
                NavigationState.Luxcavation_Thread, new List<NavigationState>
                {
                    NavigationState.Drive,             // Back
                    NavigationState.Luxcavation_EXP,   // Toggle Tab
                    NavigationState.Charge_Modules,     // Enkephalin
                    NavigationState.ToBattle           // Enter Level
                }
            },

            {
                NavigationState.MirrorDungeon, new List<NavigationState>
                {
                    NavigationState.Drive,                // Back
                    NavigationState.MirrorDungeon_Confirmation // Enter -> Confirmation
                }
            },
            
            // --- From Mirror Dungeon Confirmation ---
            {
                NavigationState.MirrorDungeon_Confirmation, new List<NavigationState>
                {
                    NavigationState.MirrorDungeon,        // Cancel
                    NavigationState.MirrorDungeon_Delving // Enter (Confirm)
                }
            },
            
            // --- From Mirror Dungeon (Delving) ---
            {
                NavigationState.MirrorDungeon_Delving, new List<NavigationState>
                {
                    NavigationState.MirrorDungeon, // Back/Cancel?
                    NavigationState.Drive          // Force Back?
                }
            },
            
            // --- ToBattle ---
            {
                NavigationState.ToBattle, new List<NavigationState>
                {
                    NavigationState.Battle,             // Enter Battle
                    NavigationState.Luxcavation_EXP,    // Back (Cancel)
                    NavigationState.Luxcavation_Thread, // Back (Cancel)
                    NavigationState.MirrorDungeon_Delving, // Potentially?
                    NavigationState.Drive               // Deep Back?
                }
            },
            
            // --- Battle ---
            {
                NavigationState.Battle, new List<NavigationState>
                {
                     NavigationState.Success, // Win
                     NavigationState.ToBattle // Give up/Retry?
                }
            },

            // --- From Charge (Any Sub-tab) ---
            // --- From Charge (Parent) ---
            {
                NavigationState.Charge, new List<NavigationState>
                {
                    NavigationState.Window, // Close/Back
                    NavigationState.Drive,
                    NavigationState.Sinners,

                    NavigationState.Charge_Modules,
                    NavigationState.Charge_Lunacy
                }
            },

            {
                NavigationState.Charge_Modules, new List<NavigationState>
                {
                    NavigationState.Window,
                    NavigationState.Drive,
                    NavigationState.Sinners,
                    NavigationState.Luxcavation_EXP,
                    NavigationState.Luxcavation_Thread,

                    NavigationState.Charge_Lunacy
                }
            },
            {
                NavigationState.Charge_Lunacy, new List<NavigationState>
                {
                    NavigationState.Window,
                    NavigationState.Drive,
                    NavigationState.Sinners,
                    NavigationState.Luxcavation_EXP,
                    NavigationState.Luxcavation_Thread,

                    NavigationState.Charge_Modules
                }
            }
            // Add others as needed
        };

        /// <summary>
        /// checks if a transition from 'from' to 'to' is logically allowed.
        /// </summary>
        public static bool CanTransition(NavigationState from, NavigationState to)
        {
            if (AllowedTransitions.TryGetValue(from, out var targets))
                return targets.Contains(to);
            
            // Allow implied parent/child transitions if necessary? 
            // For now, strict explicit graph.
            return false;
        }
    }
}
