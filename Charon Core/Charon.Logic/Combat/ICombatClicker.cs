using Charon.Input;

namespace Charon.Logic.Combat
{
    /// <summary>
    /// Interface for gameplay-specific interactions involving complex mouse movements.
    /// Dedicated to mechanics like Drag-and-Drop or intricate path following.
    /// </summary>
    public interface ICombatClicker
    {
        /// <summary>
        /// Drags an element from a start template to an end template.
        /// </summary>
        /// <param name="startTemplate">The template to grab (Mouse Down).</param>
        /// <param name="endTemplate">The template to drop onto (Mouse Up).</param>
        /// <param name="threshold">Matching confidence threshold.</param>
        /// <returns>True if both templates were found and the drag was executed.</returns>
        bool Drag(string startTemplate, string endTemplate, double threshold = 0.9);

        /// <summary>
        /// Performs a continuous drag operation through a sequence of templates.
        /// Useful for drawing shapes or following complex paths in-game.
        /// </summary>
        /// <param name="templates">Ordered list of templates (Start -> Waypoints -> End).</param>
        /// <param name="threshold">Matching confidence threshold.</param>
        /// <returns>True if all templates were found and the path was traced.</returns>
        bool DragChain(string[] templates, double threshold = 0.9);
    }
}
