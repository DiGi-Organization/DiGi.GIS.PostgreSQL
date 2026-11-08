namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// One footprint prepared for the binary COPY into an open building footprint table (DiGi.GIS.PostgreSQL#125).
    /// <para>It is the buffered unit between the import loop and the write: the loop reprojects a <c>Footprint</c>, files it under a county part and fills one of these, and the flush streams each county's rows into that county's partition. The geometry is held only as its EPSG:2180 envelope and as the serialized footprint in <c>object</c>, because those are the two shapes the table stores - nothing is derived a second time at write time.</para>
    /// </summary>
    public class OpenBuildingRow
    {
        /// <summary>
        /// Gets or sets the identifier of the county part the row is filed under; it is the partition key.
        /// </summary>
        public int CountyId { get; set; }

        /// <summary>
        /// Gets or sets the identifier the source dataset gives the footprint, or null when the source gives none.
        /// <para>This is the <c>source_id</c> column, and the third element of the table's uniqueness key.</para>
        /// </summary>
        public string? SourceId { get; set; }

        /// <summary>
        /// Gets or sets the per-object provenance serialized to JSON, or null when the dataset defines none for the row.
        /// </summary>
        public string? Provenance { get; set; }

        /// <summary>
        /// Gets or sets the normalised origin of the geometry, one of the <see cref="Constants.PrimarySource"/> values. Never null.
        /// </summary>
        public string PrimarySource { get; set; } = Constants.PrimarySource.Other;

        /// <summary>
        /// Gets or sets the minimum X of the reprojected footprint envelope, in EPSG:2180 (easting in X).
        /// </summary>
        public double MinX { get; set; }

        /// <summary>
        /// Gets or sets the minimum Y of the reprojected footprint envelope, in EPSG:2180.
        /// </summary>
        public double MinY { get; set; }

        /// <summary>
        /// Gets or sets the maximum X of the reprojected footprint envelope, in EPSG:2180.
        /// </summary>
        public double MaxX { get; set; }

        /// <summary>
        /// Gets or sets the maximum Y of the reprojected footprint envelope, in EPSG:2180.
        /// </summary>
        public double MaxY { get; set; }

        /// <summary>
        /// Gets or sets the serialized footprint in EPSG:2180 - geometry, source attributes, provenance and licence exception - as the <c>object</c> column stores it.
        /// </summary>
        public string Object { get; set; } = string.Empty;
    }
}
