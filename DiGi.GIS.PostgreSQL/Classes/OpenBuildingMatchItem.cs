
namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// One footprint as the matching sees it (DiGi.GIS.PostgreSQL#127): its key, the county part it is filed under, its geometry converted to NetTopologySuite once, and the area and centroid derived from it.
    /// <para>The same shape serves both sides: a BDOT10k <c>building_2d</c> row (key = <c>reference</c>, no dataset) and an open building footprint (key = <c>source_id</c>, with its register row and <c>primary_source</c>). A key is unique only per county part, so the pair (<see cref="CountyId"/>, <see cref="Key"/>) is the identity. Built through <see cref="Create.OpenBuildingMatchItem(int, string?, int?, string?, DiGi.Geometry.Planar.Interfaces.IGeometry2D?)"/>; a footprint whose geometry cannot be used carries a null <see cref="Geometry"/> and is never matched.</para>
    /// <para>Not serialized: an item lives for one county group of one run and holds a NetTopologySuite geometry.</para>
    /// </summary>
    public class OpenBuildingMatchItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBuildingMatchItem"/> class.
        /// </summary>
        /// <param name="countyId">The identifier of the county part the footprint is filed under.</param>
        /// <param name="key">The key of the footprint within its county part.</param>
        /// <param name="datasetId">The <c>building_dataset</c> register row of a dataset footprint, or null for a BDOT10k footprint.</param>
        /// <param name="primarySource">The normalised primary source of a dataset footprint, or null for a BDOT10k footprint.</param>
        /// <param name="geometry">The footprint in EPSG:2180 as a valid polygonal NetTopologySuite geometry, or null when it cannot be used.</param>
        /// <param name="area">The area of <paramref name="geometry"/> in square metres; zero when it is null.</param>
        /// <param name="centroidX">The X ordinate of the centroid of <paramref name="geometry"/>.</param>
        /// <param name="centroidY">The Y ordinate of the centroid of <paramref name="geometry"/>.</param>
        /// <param name="repaired">Whether the stored geometry was invalid and <paramref name="geometry"/> is its repair.</param>
        public OpenBuildingMatchItem(
            int countyId,
            string key,
            int? datasetId,
            string? primarySource,
            NetTopologySuite.Geometries.Geometry? geometry,
            double area,
            double centroidX,
            double centroidY,
            bool repaired)
        {
            CountyId = countyId;
            Key = key;
            DatasetId = datasetId;
            PrimarySource = primarySource;
            Geometry = geometry;
            Area = area;
            CentroidX = centroidX;
            CentroidY = centroidY;
            Repaired = repaired;
        }

        /// <summary>
        /// Gets the area of the footprint in square metres; zero when it has no usable geometry.
        /// </summary>
        public double Area { get; }

        /// <summary>
        /// Gets the X ordinate of the footprint's centroid in EPSG:2180.
        /// </summary>
        public double CentroidX { get; }

        /// <summary>
        /// Gets the Y ordinate of the footprint's centroid in EPSG:2180.
        /// </summary>
        public double CentroidY { get; }

        /// <summary>
        /// Gets the identifier of the county part the footprint is filed under.
        /// </summary>
        public int CountyId { get; }

        /// <summary>
        /// Gets the <c>building_dataset</c> register row of a dataset footprint, or null for a BDOT10k footprint.
        /// </summary>
        public int? DatasetId { get; }

        /// <summary>
        /// Gets the footprint in EPSG:2180 as a valid polygonal NetTopologySuite geometry, or null when its stored geometry cannot be used.
        /// </summary>
        public NetTopologySuite.Geometries.Geometry? Geometry { get; }

        /// <summary>
        /// Gets the key of the footprint within its county part: the BDOT10k <c>reference</c> or the dataset's <c>source_id</c>.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets the normalised primary source of a dataset footprint, or null for a BDOT10k footprint.
        /// </summary>
        public string? PrimarySource { get; }

        /// <summary>
        /// Gets whether the stored geometry was invalid and <see cref="Geometry"/> is its repair.
        /// </summary>
        public bool Repaired { get; }
    }
}
