using System.ComponentModel;

namespace DiGi.GIS.PostgreSQL.Solar.Enums
{
    /// <summary>
    /// Represents how a single building and radius solve of the batch solar radiation solve ended.
    /// </summary>
    public enum BuildingSolarRadiationSolveOutcomeType
    {
        /// <summary>
        /// The solve did not run: the run was cancelled before it started.
        /// </summary>
        [Description("Undefined")] Undefined,

        /// <summary>
        /// The solve produced a result that can be stored.
        /// </summary>
        [Description("Solved")] Solved,

        /// <summary>
        /// The building could not be solved at this radius, for a reason given with the outcome.
        /// </summary>
        [Description("Not solvable")] NotSolvable,

        /// <summary>
        /// The shading model holds more caster triangles than the ceiling allows.
        /// </summary>
        [Description("Over ceiling")] OverCeiling,

        /// <summary>
        /// The solve threw, or produced nothing that can be stored.
        /// </summary>
        [Description("Failed")] Failed,
    }
}
