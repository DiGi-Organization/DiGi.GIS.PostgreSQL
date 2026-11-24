using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// The outcome of one open building dataset import: how many rows the release stored, how many footprints were outside the run's scope, and how many were rejected (DiGi.GIS.PostgreSQL#125).
    /// <para>The three figures are kept apart deliberately. <see cref="FilteredCount"/> is the expected residue of a run scoped to pilot areas over a nationwide file - nothing was lost, the rows were never in scope. <see cref="RejectionCount"/> is the account of rows that could not be placed or transformed, each one named in <see cref="Rejections"/> while the list has room, and it is what figure R1 and the run report read.</para>
    /// <para><see cref="RejectionCounts"/> splits that one number per <see cref="Enums.UpdateRejectionReason"/>, so the difference between two runs' rejection figures is explained rather than guessed (DiGi.GIS.PostgreSQL#172): it is complete where <see cref="Rejections"/> is the first thousand in read order, and its entries sum to <see cref="RejectionCount"/>.</para>
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

        [JsonInclude, JsonPropertyName(nameof(RejectionCounts))]
        private readonly Dictionary<UpdateRejectionReason, long> rejectionCounts = [];

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
        /// <param name="rejectionCounts">The number of rejections per <see cref="Enums.UpdateRejectionReason"/>, or null for none; entries are copied, so the caller's dictionary is never aliased (DiGi.GIS.PostgreSQL#172).</param>
        public OpenBuildingImportResult(long rowCount, long filteredCount, long rejectionCount, IEnumerable<Rejection>? rejections, int? datasetId, IDictionary<UpdateRejectionReason, long>? rejectionCounts = null)
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

            if (rejectionCounts is not null)
            {
                // The import has finished by the time it constructs this result, but the entries are copied
                // anyway: a result never aliases a caller's collection (the same rule as the rejections).
                foreach (KeyValuePair<UpdateRejectionReason, long> pair in rejectionCounts)
                {
                    this.rejectionCounts[pair.Key] = pair.Value;
                }
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

                foreach (KeyValuePair<UpdateRejectionReason, long> pair in openBuildingImportResult.rejectionCounts)
                {
                    rejectionCounts[pair.Key] = pair.Value;
                }

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
        /// Gets the number of those rejections per <see cref="Enums.UpdateRejectionReason"/> - complete where <see cref="Rejections"/> is a capped sample, in enum order when enumerated through <c>Enum.GetValues</c>, and summing to <see cref="RejectionCount"/> (DiGi.GIS.PostgreSQL#172). Reasons the run never raised are absent rather than zero.
        /// </summary>
        [JsonIgnore]
        public Dictionary<UpdateRejectionReason, long> RejectionCounts => rejectionCounts;

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
