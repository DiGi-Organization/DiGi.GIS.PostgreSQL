using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// The outcome of one open building dataset import: how many rows the release stored, how many footprints were outside the run's scope, and how many were rejected (DiGi.GIS.PostgreSQL#125).
    /// <para>The three figures are kept apart deliberately. <see cref="FilteredCount"/> is the expected residue of a run scoped to pilot areas over a nationwide file - nothing was lost, the rows were never in scope. <see cref="RejectionCount"/> is the account of rows that could not be placed or transformed, each one named in <see cref="Rejections"/> while the list has room, and it is what figure R1 and the run report read.</para>
    /// <para><see cref="DatasetId"/> identifies the register row the run upserted in building_dataset, so a report can name it without re-querying the register (DiGi.GIS.PostgreSQL#131).</para>
    /// </summary>
    public class OpenBuildingImportResult : SerializableResult, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(DatasetId))]
        private readonly int? datasetId;

        [JsonInclude, JsonPropertyName(nameof(FilteredCount))]
        private readonly long filteredCount;

        [JsonInclude, JsonPropertyName(nameof(RejectionCount))]
        private readonly long rejectionCount;

        [JsonInclude, JsonPropertyName(nameof(Rejections))]
        private readonly List<Rejection> rejections = [];

        [JsonInclude, JsonPropertyName(nameof(RowCount))]
        private readonly long rowCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBuildingImportResult"/> class.
        /// </summary>
        /// <param name="rowCount">The number of rows stored by the import.</param>
        /// <param name="filteredCount">The number of footprints skipped as out of scope.</param>
        /// <param name="rejectionCount">The number of footprints rejected before the database.</param>
        /// <param name="rejections">The rejected footprints, each named with the reason, or null for none.</param>
        /// <param name="datasetId">The identifier of the register row the import upserted in building_dataset, or null when the result carries none (DiGi.GIS.PostgreSQL#131).</param>
        public OpenBuildingImportResult(long rowCount, long filteredCount, long rejectionCount, IEnumerable<Rejection>? rejections, int? datasetId)
        {
            this.rowCount = rowCount;
            this.filteredCount = filteredCount;
            this.rejectionCount = rejectionCount;
            this.datasetId = datasetId;

            if (rejections is not null)
            {
                // The list is built by the import that is constructing this result and handed over whole, so
                // the items are taken as they are - only the list itself is copied, to stop the caller's
                // collection aliasing the result's.
                this.rejections = [.. rejections];
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBuildingImportResult"/> class by copying an existing one.
        /// </summary>
        /// <param name="openBuildingImportResult">The source result to copy.</param>
        public OpenBuildingImportResult(OpenBuildingImportResult? openBuildingImportResult)
            : base(openBuildingImportResult)
        {
            if (openBuildingImportResult is not null)
            {
                datasetId = openBuildingImportResult.datasetId;
                filteredCount = openBuildingImportResult.filteredCount;
                rejectionCount = openBuildingImportResult.rejectionCount;
                rowCount = openBuildingImportResult.rowCount;

                foreach (Rejection rejection in openBuildingImportResult.rejections)
                {
                    if (Core.Query.Clone(rejection) is Rejection rejection_Clone)
                    {
                        rejections.Add(rejection_Clone);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBuildingImportResult"/> class from a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The JSON object containing the result.</param>
        public OpenBuildingImportResult(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the identifier of the register row the import upserted in building_dataset, or null when the result carries none.
        /// </summary>
        [JsonIgnore]
        public int? DatasetId => datasetId;

        /// <summary>
        /// Gets the number of footprints skipped as out of the run's scope - outside the scope bounding box, or resolved to a county part outside the run's scope. Not a loss: those rows belonged to another run.
        /// </summary>
        [JsonIgnore]
        public long FilteredCount => filteredCount;

        /// <summary>
        /// Gets the number of footprints rejected before the database.
        /// </summary>
        [JsonIgnore]
        public long RejectionCount => rejectionCount;

        /// <summary>
        /// Gets the rejected footprints, each named with the reason it was dropped, capped so a run over a corrupt file does not hold every bad row in memory.
        /// </summary>
        [JsonIgnore]
        public List<Rejection> Rejections => rejections;

        /// <summary>
        /// Gets the number of rows stored by the import.
        /// </summary>
        [JsonIgnore]
        public long RowCount => rowCount;
    }
}
