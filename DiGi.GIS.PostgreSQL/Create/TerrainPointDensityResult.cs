using DiGi.GIS.PostgreSQL.Classes;
using System;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Create
    {
        /// <summary>
        /// Works out how densely a county partition of the terrain point table is sampled and returns it as a <see cref="TerrainPointDensityResult"/>.
        /// <para>Every figure the result carries beyond the count and the area is derived here rather than by the constructor, which assigns and nothing more.</para>
        /// <para>A figure that cannot be derived is left null rather than filled with a not-a-number. Strict JSON has no token for one, so a not-a-number reaching a response body is a serialization failure rather than a value a reader can act on.</para>
        /// </summary>
        /// <param name="countyId">The identifier of the county partition.</param>
        /// <param name="count">The number of points stored for the county.</param>
        /// <param name="area">The area the points were meant to cover, in square model units - the partition's own outline as far as the county's subdivisions cover it, which is the territory a sampling run files under it.</param>
        /// <param name="gridSize">The spacing a sampling run used, when it is known. Supplying it is what fills in <see cref="TerrainPointDensityResult.ExpectedDensity"/> and <see cref="TerrainPointDensityResult.Completeness"/>.</param>
        /// <returns>The <see cref="TerrainPointDensityResult"/>, or null when the count is negative or the area is not a usable measurement.</returns>
        public static TerrainPointDensityResult? TerrainPointDensityResult(int countyId, long count, double area, double? gridSize = null)
        {
            if (count < 0 || double.IsNaN(area) || double.IsInfinity(area) || area < 0)
            {
                return null;
            }

            double? density = area > 0 ? count / area : null;

            // Derived from the area rather than from the density so that a county holding no point at all
            // reports no spacing instead of an infinite one.
            double? spacingEquivalent = area > 0 && count > 0 ? Math.Sqrt(area / count) : null;

            double? expectedDensity = null;
            if (gridSize.HasValue && !double.IsNaN(gridSize.Value) && !double.IsInfinity(gridSize.Value) && gridSize.Value > 0)
            {
                expectedDensity = 1 / (gridSize.Value * gridSize.Value);
            }

            double? completeness = density.HasValue && expectedDensity.HasValue ? density.Value / expectedDensity.Value : null;

            return new TerrainPointDensityResult(countyId, count, area, density, spacingEquivalent, expectedDensity, completeness);
        }
    }
}
