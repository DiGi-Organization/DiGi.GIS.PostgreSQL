using DiGi.Analytical.Building.Solar;
using DiGi.Core.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.Analytical.Solar;
using DiGi.GIS.PostgreSQL.Solar.Classes;
using DiGi.GIS.PostgreSQL.Solar.Enums;
using DiGi.Solar.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Solar
{
    public static partial class Query
    {
        /// <summary>
        /// Solves the jobs of one batch of the batch solar radiation solve, several at a time, and returns one outcome per job in job order (DiGi.GIS.PostgreSQL#139).
        /// <para>Each job is solved exactly as the viewer solves a building: the neighbours are the models whose planar bounding box lies within or intersects the building's circle at the job's radius (<c>SolarNeighbourCircle</c>, as <c>gis/buildingmodel/itemsbycircle</c> picks them), none at radius 0; the shading model receives on the job's normals only; above <paramref name="casterTriangleCountMax"/> caster triangles the job is over the ceiling and not solved.</para>
        /// <para>Jobs run on up to <paramref name="maxDegreeOfParallelism"/> threads. Each one builds its own shading model - a shading element clones its face - and only reads the building models, so the outcomes do not depend on the degree of parallelism: 1 is the sequential solve. The solver also parallelises inside each solve, which is why the automatic degree is half the logical processors.</para>
        /// <para>A job that throws is a <see cref="BuildingSolarRadiationSolveOutcomeType.Failed"/> outcome carrying the exception; only a requested cancellation leaves the method, after the jobs in flight have finished.</para>
        /// </summary>
        /// <param name="buildingSolarRadiationSolveJobs">The jobs to solve.</param>
        /// <param name="neighbours">The neighbour models any job may see, with their planar bounding boxes. Null or empty solves every job without neighbours.</param>
        /// <param name="casterTriangleCountMax">The largest number of shading-only triangles a solve is run against.</param>
        /// <param name="maxDegreeOfParallelism">The largest number of jobs solved at once. 0 or less uses half the logical processors; 1 solves one job at a time.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>One outcome per job, in job order; empty when there are no jobs.</returns>
        /// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was cancelled.</exception>
        public static List<BuildingSolarRadiationSolveOutcome> BuildingSolarRadiationSolveOutcomes(this IList<BuildingSolarRadiationSolveJob>? buildingSolarRadiationSolveJobs, IList<(DiGi.Analytical.Building.Classes.BuildingModel BuildingModel, double XMin, double YMin, double XMax, double YMax)>? neighbours, int casterTriangleCountMax, int maxDegreeOfParallelism = 0, CancellationToken cancellationToken = default)
        {
            if (buildingSolarRadiationSolveJobs is null || buildingSolarRadiationSolveJobs.Count == 0)
            {
                return [];
            }

            // Whether a planar bounding box lies within or intersects a circle - the box's nearest point to the centre is inside it.
            static bool Intersects(Circle2D circle2D, double xMin, double yMin, double xMax, double yMax)
            {
                Point2D? center = circle2D.Center;
                if (center is null)
                {
                    return false;
                }

                double dx = Math.Max(Math.Max(xMin - center.X, 0), center.X - xMax);
                double dy = Math.Max(Math.Max(yMin - center.Y, 0), center.Y - yMax);

                double radius = circle2D.Radius + Core.Constants.Tolerance.Distance;

                return (dx * dx) + (dy * dy) <= radius * radius;
            }

            // Each job writes only its own slot, so the outcomes need no lock and keep the job order.
            BuildingSolarRadiationSolveOutcome?[] buildingSolarRadiationSolveOutcomes = new BuildingSolarRadiationSolveOutcome?[buildingSolarRadiationSolveJobs.Count];

            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism > 0 ? maxDegreeOfParallelism : Math.Max(1, Core.Query.ProcessorCount(0.5)),
                CancellationToken = cancellationToken
            };

            Parallel.For(0, buildingSolarRadiationSolveJobs.Count, parallelOptions, i =>
            {
                BuildingSolarRadiationSolveJob buildingSolarRadiationSolveJob = buildingSolarRadiationSolveJobs[i];
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                Stopwatch stopwatch = Stopwatch.StartNew();
                int casterTriangleCount = 0;
                try
                {
                    DiGi.Analytical.Building.Classes.BuildingModel buildingModel = buildingSolarRadiationSolveJob.BuildingModel;

                    List<DiGi.Analytical.Building.Classes.BuildingModel>? buildingModels_Surrounding = null;
                    if (buildingSolarRadiationSolveJob.Radius > 0)
                    {
                        Circle2D? circle2D = buildingModel.SolarNeighbourCircle(buildingSolarRadiationSolveJob.Radius);
                        buildingModels_Surrounding = circle2D is null || neighbours is null ? [] : [.. neighbours.Where(x => Intersects(circle2D, x.XMin, x.YMin, x.XMax, x.YMax)).Select(x => x.BuildingModel)];
                    }

                    Dictionary<string, Geometry.Spatial.Classes.Vector3D> normals = buildingSolarRadiationSolveJob.Normals;
                    ShadingModel? shadingModel = buildingModel.ToSolar(buildingModels_Surrounding, x => new GuidReference(x).ToString() is string reference_Component && normals.ContainsKey(reference_Component));
                    if (shadingModel is null)
                    {
                        // A guard: a job always holds a model, which converts even without receivers.
                        buildingSolarRadiationSolveOutcomes[i] = new BuildingSolarRadiationSolveOutcome(buildingSolarRadiationSolveJob, BuildingSolarRadiationSolveOutcomeType.NotSolvable, null, "it cannot be converted into a shading model", null, 0, 0);
                        return;
                    }

                    casterTriangleCount = shadingModel.CasterTriangleCount();
                    if (casterTriangleCount > casterTriangleCountMax)
                    {
                        buildingSolarRadiationSolveOutcomes[i] = new BuildingSolarRadiationSolveOutcome(buildingSolarRadiationSolveJob, BuildingSolarRadiationSolveOutcomeType.OverCeiling, null, null, null, 0, casterTriangleCount);
                        return;
                    }

                    List<GIS.Classes.SurfaceSolarRadiation>? surfaceSolarRadiations = shadingModel.SurfaceSolarRadiations(normals, buildingSolarRadiationSolveJob.EPWFile, Analytical.Solar.Create.SolarShadingSolverOptions());

                    stopwatch.Stop();

                    GIS.Classes.BuildingSolarRadiationResult? buildingSolarRadiationResult = buildingModel.BuildingSolarRadiationResult(buildingSolarRadiationSolveJob.CountyId, buildingSolarRadiationSolveJob.Radius, buildingSolarRadiationSolveJob.EPWFileId, surfaceSolarRadiations);
                    buildingSolarRadiationSolveOutcomes[i] = buildingSolarRadiationResult is null
                        ? new BuildingSolarRadiationSolveOutcome(buildingSolarRadiationSolveJob, BuildingSolarRadiationSolveOutcomeType.Failed, null, "the solve produced no result that can be stored", null, stopwatch.ElapsedMilliseconds, casterTriangleCount)
                        : new BuildingSolarRadiationSolveOutcome(buildingSolarRadiationSolveJob, BuildingSolarRadiationSolveOutcomeType.Solved, buildingSolarRadiationResult, null, null, stopwatch.ElapsedMilliseconds, casterTriangleCount);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // Left empty; the cancellation is rethrown once the loop has stopped.
                }
                catch (Exception exception)
                {
                    stopwatch.Stop();
                    buildingSolarRadiationSolveOutcomes[i] = new BuildingSolarRadiationSolveOutcome(buildingSolarRadiationSolveJob, BuildingSolarRadiationSolveOutcomeType.Failed, null, exception.Message, exception, stopwatch.ElapsedMilliseconds, casterTriangleCount);
                }
            });

            cancellationToken.ThrowIfCancellationRequested();

            List<BuildingSolarRadiationSolveOutcome> result = [];
            for (int i = 0; i < buildingSolarRadiationSolveOutcomes.Length; i++)
            {
                result.Add(buildingSolarRadiationSolveOutcomes[i] ?? new BuildingSolarRadiationSolveOutcome(buildingSolarRadiationSolveJobs[i], BuildingSolarRadiationSolveOutcomeType.Undefined, null, null, null, 0, 0));
            }

            return result;
        }
    }
}
