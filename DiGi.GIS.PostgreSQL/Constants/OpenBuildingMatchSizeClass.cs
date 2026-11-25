using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL.Constants
{
    /// <summary>
    /// The footprint size classes the open building matching reports omission and commission by (DiGi.GIS.PostgreSQL#127).
    /// <para>Detection of buildings is reliable above 50 m² and not below it (ISPRS urban object detection benchmark, Rottensteiner et al., 2014), and BDOT10k holds many small farm and utility buildings, so a single minimum-area cut hides where the datasets disagree. The classes are reported rather than excluded: below 10 m², 10 to 50 m², and 50 m² and more.</para>
    /// </summary>
    public static class OpenBuildingMatchSizeClass
    {
        /// <summary>
        /// The upper bounds in square metres of every size class but the last, ascending: a footprint belongs to the first class whose bound exceeds its area, and to the last class when none does.
        /// </summary>
        public static readonly IReadOnlyList<double> Bounds = [10, 50];
    }
}
