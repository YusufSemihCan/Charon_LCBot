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
                    NavigationState.Charge_Modules     // Enkephalin
                }
            },

            // --- From Luxcavation (Thread) ---
            {
                NavigationState.Luxcavation_Thread, new List<NavigationState>
                {
                    NavigationState.Drive,             // Back
                    NavigationState.Luxcavation_EXP,   // Toggle Tab
                    NavigationState.Charge_Modules     // Enkephalin
                }
            },

            // --- From Mirror Dungeon ---
            {
                NavigationState.MirrorDungeon, new List<NavigationState>
                {
                    NavigationState.Drive,                // Back
                    NavigationState.MirrorDungeon_Delving // Enter
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

            // --- From Charge (Any Sub-tab) ---
            // --- From Charge (Parent) ---
            {
                NavigationState.Charge, new List<NavigationState>
                {
                    NavigationState.Window, // Close/Back
                    NavigationState.Drive,
                    NavigationState.Sinners,
                    NavigationState.Charge_Boxes,
                    NavigationState.Charge_Modules,
                    NavigationState.Charge_Lunacy
                }
            },
            {
                NavigationState.Charge_Boxes, new List<NavigationState>
                {
                    NavigationState.Window,
                    NavigationState.Drive,
                    NavigationState.Sinners,
                    NavigationState.Luxcavation_EXP,
                    NavigationState.Luxcavation_Thread,
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
                    NavigationState.Charge_Boxes,
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
                    NavigationState.Charge_Boxes,
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
