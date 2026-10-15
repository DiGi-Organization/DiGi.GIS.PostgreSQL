using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Create
    {
        /// <summary>
        /// Creates a copy of a stored solar radiation result filed under the given county part and carrying the given shading factors, the two things that differ between the result a caller holds and the row the <see cref="Constants.TableName.BuildingSolarRadiation"/> table stores.
        /// <para>A write passes the resolved county part and null factors: the <c>object</c> column holds the result without factors, which go to the packed <c>shading_factors</c> column. A read passes the factors unpacked from that column back. Every other member is copied as it is.</para>
        /// </summary>
        /// <param name="buildingSolarRadiationResult">The result to copy.</param>
        /// <param name="countyId">The county part the copy is filed under.</param>
        /// <param name="shadingFactors">The shading factors of each surface, in surface order, or null to remove every surface's factors.</param>
        /// <returns>The copy, or null when <paramref name="buildingSolarRadiationResult"/> is null or <paramref name="shadingFactors"/> does not hold one entry per surface.</returns>
        public static GIS.Classes.BuildingSolarRadiationResult? BuildingSolarRadiationResult(this GIS.Classes.BuildingSolarRadiationResult? buildingSolarRadiationResult, int countyId, IList<byte[]?>? shadingFactors)
        {
            if (buildingSolarRadiationResult is null)
            {
                return null;
            }

            List<GIS.Classes.SurfaceSolarRadiation>? surfaceSolarRadiations = buildingSolarRadiationResult.SurfaceSolarRadiations;
            if (shadingFactors is not null && shadingFactors.Count != (surfaceSolarRadiations?.Count ?? 0))
            {
                return null;
            }

            List<GIS.Classes.SurfaceSolarRadiation>? surfaceSolarRadiations_New = null;
            if (surfaceSolarRadiations is not null)
            {
                surfaceSolarRadiations_New = [];
                for (int i = 0; i < surfaceSolarRadiations.Count; i++)
                {
                    GIS.Classes.SurfaceSolarRadiation surfaceSolarRadiation = surfaceSolarRadiations[i];
                    surfaceSolarRadiations_New.Add(new GIS.Classes.SurfaceSolarRadiation(
                        surfaceSolarRadiation.Reference,
                        surfaceSolarRadiation.Area,
                        surfaceSolarRadiation.Normal,
                        surfaceSolarRadiation.SkyVisibility,
                        surfaceSolarRadiation.GroundVisibility,
                        surfaceSolarRadiation.Irradiation,
                        surfaceSolarRadiation.Beam,
                        surfaceSolarRadiation.Diffuse,
                        surfaceSolarRadiation.Ground,
                        surfaceSolarRadiation.IrradiationUnshaded,
                        surfaceSolarRadiation.Energy,
                        shadingFactors?[i]));
                }
            }

            return new GIS.Classes.BuildingSolarRadiationResult(
                buildingSolarRadiationResult.Reference,
                countyId,
                buildingSolarRadiationResult.BuildingModelId,
                buildingSolarRadiationResult.Radius,
                buildingSolarRadiationResult.SolverVersion,
                buildingSolarRadiationResult.WeatherSourceType,
                buildingSolarRadiationResult.WeatherSourceId,
                buildingSolarRadiationResult.WeatherPeriodStart,
                buildingSolarRadiationResult.WeatherPeriodEnd,
                buildingSolarRadiationResult.UserEmail,
                buildingSolarRadiationResult.CreatedAt,
                surfaceSolarRadiations_New);
        }
    }
}
