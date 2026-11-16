using DiGi.GIS.PostgreSQL.Solar.Enums;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL.Solar.Classes
{
    /// <summary>
    /// Holds one batch of the batch solar radiation solve once it has been read: its solve jobs, the neighbours they may see and, in the order a sequential run met them, the buildings stepped over before solving (DiGi.GIS.PostgreSQL#139).
    /// <para>Read ahead while the previous batch is solved, so it carries what happened instead of counting it: the task aggregates it in order once the batch is solved.</para>
    /// </summary>
    internal sealed class BuildingSolarRadiationSolveBatch
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingSolarRadiationSolveBatch"/> class.
        /// </summary>
        /// <param name="buildingCount">The number of buildings of the batch.</param>
        /// <param name="buildingSolarRadiationSolveJobs">The solve jobs, receiver by receiver and radius by radius.</param>
        /// <param name="neighbours">The neighbour models any job may see, with their planar bounding boxes.</param>
        /// <param name="issues">The buildings stepped over while reading, in order; a null reference is a building with no stored model, an over-ceiling issue is counted without a sample.</param>
        /// <param name="receivers">The receivers in order, with their number of pending radii and whether a weather file was read for them; one with weather owns its next jobs, one per radius.</param>
        public BuildingSolarRadiationSolveBatch(int buildingCount, List<BuildingSolarRadiationSolveJob> buildingSolarRadiationSolveJobs, List<(DiGi.Analytical.Building.Classes.BuildingModel BuildingModel, double XMin, double YMin, double XMax, double YMax)> neighbours, List<(string? Reference, BuildingSolarRadiationSolveOutcomeType BuildingSolarRadiationSolveOutcomeType, int Count, string? Reason)> issues, List<(string Reference, int Count, bool HasWeather)> receivers)
        {
            BuildingCount = buildingCount;
            BuildingSolarRadiationSolveJobs = buildingSolarRadiationSolveJobs;
            Neighbours = neighbours;
            Issues = issues;
            Receivers = receivers;
        }

        /// <summary>
        /// Gets the number of buildings of the batch.
        /// </summary>
        public int BuildingCount { get; }

        /// <summary>
        /// Gets the solve jobs, receiver by receiver and radius by radius.
        /// </summary>
        public List<BuildingSolarRadiationSolveJob> BuildingSolarRadiationSolveJobs { get; }

        /// <summary>
        /// Gets the buildings stepped over while reading, in order.
        /// </summary>
        public List<(string? Reference, BuildingSolarRadiationSolveOutcomeType BuildingSolarRadiationSolveOutcomeType, int Count, string? Reason)> Issues { get; }

        /// <summary>
        /// Gets the neighbour models any job may see, with their planar bounding boxes.
        /// </summary>
        public List<(DiGi.Analytical.Building.Classes.BuildingModel BuildingModel, double XMin, double YMin, double XMax, double YMax)> Neighbours { get; }

        /// <summary>
        /// Gets the receivers in order, with their number of pending radii and whether a weather file was read for them.
        /// </summary>
        public List<(string Reference, int Count, bool HasWeather)> Receivers { get; }
    }
}
