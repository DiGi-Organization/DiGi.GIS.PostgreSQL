namespace DiGi.GIS.PostgreSQL.Constants
{
    /// <summary>
    /// Provides the closed vocabulary of the <c>primary_source</c> column of the open building footprint tables (DiGi.GIS.PostgreSQL#125).
    /// <para>Every stored row carries exactly one of these values, normalised from the row's per-object provenance by <see cref="Query.PrimarySource(System.Text.Json.Nodes.JsonObject)"/>. The vocabulary feeds figure R1 (the provenance diagram), so the values are part of the published output and must not drift.</para>
    /// </summary>
    public static class PrimarySource
    {
        /// <summary>
        /// The geometry originates in an official register or government dataset (EUBUCCO <c>gov-*</c> sources).
        /// </summary>
        public const string Register = "register";

        /// <summary>
        /// The geometry originates in OpenStreetMap (the OSM reader itself, an OSM-sourced EUBUCCO or DBSM row, or an OSM entry of the Overture <c>sources</c> array).
        /// </summary>
        public const string Osm = "osm";

        /// <summary>
        /// The geometry originates in Microsoft (the Microsoft reader's quadkey, or a Microsoft entry of the Overture <c>sources</c> array).
        /// </summary>
        public const string Microsoft = "microsoft";

        /// <summary>
        /// The geometry originates in Esri (an Esri entry of the Overture <c>sources</c> array).
        /// </summary>
        public const string Esri = "esri";

        /// <summary>
        /// The origin does not fall into any of the named categories (GHS-OBAT model output, an ESA-only DBSM row, an unrecognised source entry), or the row carries no provenance at all.
        /// </summary>
        public const string Other = "other";
    }
}
