using System.ComponentModel;

namespace DiGi.GIS.PostgreSQL.Enums
{
    /// <summary>
    /// Names why a row handed to an update never reached the database.
    /// <para>A row that cannot be filed under a county part is dropped rather than written, which used to leave no trace at all: the caller received only the identifiers of the rows that were stored, so a batch that stored one of five thousand looked exactly like a batch that stored all of them. The reason is carried alongside the reference because it decides what the caller should do next - a payload defect is worth correcting and reposting, a footprint that falls outside every candidate part is not.</para>
    /// </summary>
    [Description("UpdateRejectionReason")]
    public enum UpdateRejectionReason
    {
        /// <summary>
        /// The element itself was null, so there is nothing to name.
        /// </summary>
        [Description("Undefined")] Undefined = 0,

        /// <summary>
        /// The row carries no bounding box, so no county could even be attempted. A defect in the posted payload.
        /// </summary>
        [Description("Missing Geometry")] MissingGeometry = 1,

        /// <summary>
        /// County resolution ran and named no part. Not necessarily the caller's fault - the last tier decides by geometry, and a footprint falling outside every candidate part lands here.
        /// </summary>
        [Description("County Unresolved")] CountyUnresolved = 2,

        /// <summary>
        /// The county resolved, but its partition could not be created, so every row filed under it was dropped. Server-side.
        /// </summary>
        [Description("Partition Unavailable")] PartitionUnavailable = 3,

        /// <summary>
        /// The caller did not state a county identifier, and the row carries no geometry from which one could be inferred. A defect in the posted payload.
        /// </summary>
        [Description("Missing County")] MissingCounty = 4,

        /// <summary>
        /// The row carries a coordinate reference system the reprojection does not support, or a transformation of its geometry failed (a null input, a non-finite vertex, a ring shorter than four ordinates, or a result outside the EPSG:2180 domain). The footprint is counted and never written - a partially reprojected ring is not a row (DiGi.GIS.PostgreSQL#125).
        /// </summary>
        [Description("Geometry Rejected")] GeometryRejected = 5,
    }
}
