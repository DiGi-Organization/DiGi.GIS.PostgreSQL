using DiGi.GIS.PostgreSQL.Enums;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// One row of <c>building_dataset_match</c> (DiGi.GIS.PostgreSQL#127): a pair of overlapping footprints joined by an edge of the overlap graph, or a single footprint that is an omission, a commission or excluded by the minimum area.
    /// <para>A BDOT10k footprint is named by (<see cref="BDOTCountyId"/>, <see cref="BDOTReference"/>) and a dataset footprint by (<see cref="SourceCountyId"/>, <see cref="SourceId"/>) - keys are unique only per county part, and a <c>building_2d.id</c> is renumbered by a BDOT10k re-import, so neither side is keyed by a row id. A component of several footprints is one row per edge, every row carrying the component's class and identifier, so a footprint is counted once by counting its key distinctly.</para>
    /// <para>The measures (<see cref="IoU"/>, <see cref="AreaRatio"/>, <see cref="CentroidDistance"/>, <see cref="Dx"/>, <see cref="Dy"/>) describe the pair of the row and are null on a single-footprint row. <see cref="PointX"/> and <see cref="PointY"/> are the representative point of the component - the centroid of its owning footprint - shared by every row of it, so a box scope takes a component whole or not at all.</para>
    /// </summary>
    public class OpenBuildingMatch
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBuildingMatch"/> class.
        /// </summary>
        /// <param name="countyId">The county part owning the row: the county of the component's owning footprint.</param>
        /// <param name="componentId">The identifier of the component, unique within one (dataset, owning county part) snapshot.</param>
        /// <param name="openBuildingMatchClass">The class of the component.</param>
        /// <param name="bDOTCountyId">The county part of the BDOT10k footprint, or null on a commission row or an excluded dataset footprint.</param>
        /// <param name="bDOTReference">The reference of the BDOT10k footprint, or null on a commission row or an excluded dataset footprint.</param>
        /// <param name="sourceCountyId">The county part of the dataset footprint, or null on an omission row or an excluded BDOT10k footprint.</param>
        /// <param name="sourceId">The source identifier of the dataset footprint, or null on an omission row or an excluded BDOT10k footprint.</param>
        /// <param name="primarySource">The normalised primary source of the dataset footprint, or null when the row has none.</param>
        /// <param name="ioU">The intersection over union of the pair, or null on a single-footprint row.</param>
        /// <param name="areaRatio">The dataset footprint's area divided by the BDOT10k footprint's area, or null on a single-footprint row.</param>
        /// <param name="centroidDistance">The distance between the two centroids in metres, or null on a single-footprint row.</param>
        /// <param name="dx">The X offset of the dataset centroid from the BDOT10k centroid in metres, or null on a single-footprint row.</param>
        /// <param name="dy">The Y offset of the dataset centroid from the BDOT10k centroid in metres, or null on a single-footprint row.</param>
        /// <param name="pointX">The X ordinate of the component's representative point in EPSG:2180.</param>
        /// <param name="pointY">The Y ordinate of the component's representative point in EPSG:2180.</param>
        public OpenBuildingMatch(
            int countyId,
            long componentId,
            OpenBuildingMatchClass openBuildingMatchClass,
            int? bDOTCountyId,
            string? bDOTReference,
            int? sourceCountyId,
            string? sourceId,
            string? primarySource,
            double? ioU,
            double? areaRatio,
            double? centroidDistance,
            double? dx,
            double? dy,
            double pointX,
            double pointY)
        {
            CountyId = countyId;
            ComponentId = componentId;
            OpenBuildingMatchClass = openBuildingMatchClass;
            BDOTCountyId = bDOTCountyId;
            BDOTReference = bDOTReference;
            SourceCountyId = sourceCountyId;
            SourceId = sourceId;
            PrimarySource = primarySource;
            IoU = ioU;
            AreaRatio = areaRatio;
            CentroidDistance = centroidDistance;
            Dx = dx;
            Dy = dy;
            PointX = pointX;
            PointY = pointY;
        }

        /// <summary>
        /// Gets the dataset footprint's area divided by the BDOT10k footprint's area, or null on a single-footprint row.
        /// </summary>
        public double? AreaRatio { get; }

        /// <summary>
        /// Gets the county part of the BDOT10k footprint, or null when the row has none.
        /// </summary>
        public int? BDOTCountyId { get; }

        /// <summary>
        /// Gets the reference of the BDOT10k footprint, or null when the row has none.
        /// </summary>
        public string? BDOTReference { get; }

        /// <summary>
        /// Gets the distance between the two centroids in metres, or null on a single-footprint row.
        /// </summary>
        public double? CentroidDistance { get; }

        /// <summary>
        /// Gets the identifier of the component, unique within one (dataset, owning county part) snapshot.
        /// </summary>
        public long ComponentId { get; }

        /// <summary>
        /// Gets the county part owning the row: the county of the component's owning footprint.
        /// </summary>
        public int CountyId { get; }

        /// <summary>
        /// Gets the X offset of the dataset centroid from the BDOT10k centroid in metres, or null on a single-footprint row.
        /// </summary>
        public double? Dx { get; }

        /// <summary>
        /// Gets the Y offset of the dataset centroid from the BDOT10k centroid in metres, or null on a single-footprint row.
        /// </summary>
        public double? Dy { get; }

        /// <summary>
        /// Gets the intersection over union of the pair, or null on a single-footprint row.
        /// </summary>
        public double? IoU { get; }

        /// <summary>
        /// Gets the class of the component.
        /// </summary>
        public OpenBuildingMatchClass OpenBuildingMatchClass { get; }

        /// <summary>
        /// Gets the X ordinate of the component's representative point in EPSG:2180.
        /// </summary>
        public double PointX { get; }

        /// <summary>
        /// Gets the Y ordinate of the component's representative point in EPSG:2180.
        /// </summary>
        public double PointY { get; }

        /// <summary>
        /// Gets the normalised primary source of the dataset footprint, or null when the row has none.
        /// </summary>
        public string? PrimarySource { get; }

        /// <summary>
        /// Gets the county part of the dataset footprint, or null when the row has none.
        /// </summary>
        public int? SourceCountyId { get; }

        /// <summary>
        /// Gets the source identifier of the dataset footprint, or null when the row has none.
        /// </summary>
        public string? SourceId { get; }
    }
}
