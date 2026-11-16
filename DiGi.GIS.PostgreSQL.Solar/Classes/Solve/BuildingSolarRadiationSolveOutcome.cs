using DiGi.GIS.PostgreSQL.Solar.Enums;
using System;

namespace DiGi.GIS.PostgreSQL.Solar.Classes
{
    /// <summary>
    /// Holds how one <see cref="BuildingSolarRadiationSolveJob"/> ended: its result, or why it has none (DiGi.GIS.PostgreSQL#139).
    /// </summary>
    public class BuildingSolarRadiationSolveOutcome
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingSolarRadiationSolveOutcome"/> class.
        /// </summary>
        /// <param name="buildingSolarRadiationSolveJob">The job the outcome belongs to.</param>
        /// <param name="buildingSolarRadiationSolveOutcomeType">How the job ended.</param>
        /// <param name="buildingSolarRadiationResult">The result to store; set only for <see cref="BuildingSolarRadiationSolveOutcomeType.Solved"/>.</param>
        /// <param name="reason">Why the job has no result; null for a solved job.</param>
        /// <param name="exception">The exception a failed solve threw, or null.</param>
        /// <param name="milliseconds">The time spent building the shading model and solving, in ms, up to the failure for a failed job; 0 for a job over the ceiling or not solvable.</param>
        public BuildingSolarRadiationSolveOutcome(BuildingSolarRadiationSolveJob buildingSolarRadiationSolveJob, BuildingSolarRadiationSolveOutcomeType buildingSolarRadiationSolveOutcomeType, GIS.Classes.BuildingSolarRadiationResult? buildingSolarRadiationResult, string? reason, Exception? exception, long milliseconds)
        {
            BuildingSolarRadiationSolveJob = buildingSolarRadiationSolveJob;
            BuildingSolarRadiationSolveOutcomeType = buildingSolarRadiationSolveOutcomeType;
            BuildingSolarRadiationResult = buildingSolarRadiationResult;
            Reason = reason;
            Exception = exception;
            Milliseconds = milliseconds;
        }

        /// <summary>
        /// Gets the result to store; set only for <see cref="BuildingSolarRadiationSolveOutcomeType.Solved"/>.
        /// </summary>
        public GIS.Classes.BuildingSolarRadiationResult? BuildingSolarRadiationResult { get; }

        /// <summary>
        /// Gets the job the outcome belongs to.
        /// </summary>
        public BuildingSolarRadiationSolveJob BuildingSolarRadiationSolveJob { get; }

        /// <summary>
        /// Gets how the job ended.
        /// </summary>
        public BuildingSolarRadiationSolveOutcomeType BuildingSolarRadiationSolveOutcomeType { get; }

        /// <summary>
        /// Gets the exception a failed solve threw, or null.
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// Gets the time spent building the shading model and solving, in ms, up to the failure for a failed job; 0 for a job over the ceiling or not solvable.
        /// </summary>
        public long Milliseconds { get; }

        /// <summary>
        /// Gets why the job has no result; null for a solved job.
        /// </summary>
        public string? Reason { get; }
    }
}
