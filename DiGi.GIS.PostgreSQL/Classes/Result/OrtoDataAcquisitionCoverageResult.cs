using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// How far the acquisition-date backfill (<see cref="Modify.UpdateOrtoDatasAcquisitionAsync"/>) has reached in one county part: the stored series of <c>orto_datas</c> against their rows in <c>orto_data_acquisition</c>.
    /// <para>Completeness is measured per series, not per image: counting the images of <c>orto_datas</c> would detoast every stored photo, while a series either has acquisition rows or does not. <see cref="WithoutAcquisitionSeriesCount"/> is the backlog - series never backfilled, or re-fetched since (a re-fetch drops a series' acquisition rows) - and a re-run of the backfill over the part brings it to zero.</para>
    /// <para>Quality is measured per image, over the acquisition rows: <see cref="DayCount"/>, <see cref="MonthCount"/> and <see cref="YearCount"/> are dated by an index sheet at that precision, <see cref="UndefinedCount"/> matched no sheet. An undated share that stays high after a re-run means the sheet index (<c>orto_sheet</c>) does not cover the part or its years, not that the backfill is behind.</para>
    /// </summary>
    public class OrtoDataAcquisitionCoverageResult : SerializableResult, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(AcquisitionDate_First))]
        private readonly DateTimeOffset? acquisitionDate_First;

        [JsonInclude, JsonPropertyName(nameof(AcquisitionDate_Last))]
        private readonly DateTimeOffset? acquisitionDate_Last;

        [JsonInclude, JsonPropertyName(nameof(CountyId))]
        private readonly int countyId;

        [JsonInclude, JsonPropertyName(nameof(CreatedAt_Last))]
        private readonly DateTimeOffset? createdAt_Last;

        [JsonInclude, JsonPropertyName(nameof(DatedSeriesCount))]
        private readonly long datedSeriesCount;

        [JsonInclude, JsonPropertyName(nameof(DayCount))]
        private readonly long dayCount;

        [JsonInclude, JsonPropertyName(nameof(MonthCount))]
        private readonly long monthCount;

        [JsonInclude, JsonPropertyName(nameof(SeriesCount))]
        private readonly long seriesCount;

        [JsonInclude, JsonPropertyName(nameof(UndefinedCount))]
        private readonly long undefinedCount;

        [JsonInclude, JsonPropertyName(nameof(WithAcquisitionSeriesCount))]
        private readonly long withAcquisitionSeriesCount;

        [JsonInclude, JsonPropertyName(nameof(YearCount))]
        private readonly long yearCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoDataAcquisitionCoverageResult"/> class.
        /// </summary>
        /// <param name="countyId">The identifier of the county part measured.</param>
        /// <param name="seriesCount">The series (rows) <c>orto_datas</c> holds in the part.</param>
        /// <param name="withAcquisitionSeriesCount">How many of those series have at least one acquisition row.</param>
        /// <param name="datedSeriesCount">How many of those series have at least one image an index sheet dated.</param>
        /// <param name="dayCount">The images of those series dated to the day.</param>
        /// <param name="monthCount">The images of those series dated to the month.</param>
        /// <param name="yearCount">The images of those series dated to the year.</param>
        /// <param name="undefinedCount">The images of those series no index sheet dated.</param>
        /// <param name="acquisitionDate_First">The earliest acquisition date, or null when no image is dated.</param>
        /// <param name="acquisitionDate_Last">The latest acquisition date, or null when no image is dated.</param>
        /// <param name="createdAt_Last">When an acquisition row of the part was last inserted or changed, or null when it has none.</param>
        public OrtoDataAcquisitionCoverageResult(int countyId, long seriesCount, long withAcquisitionSeriesCount, long datedSeriesCount, long dayCount, long monthCount, long yearCount, long undefinedCount, DateTimeOffset? acquisitionDate_First, DateTimeOffset? acquisitionDate_Last, DateTimeOffset? createdAt_Last)
        {
            this.countyId = countyId;
            this.seriesCount = seriesCount;
            this.withAcquisitionSeriesCount = withAcquisitionSeriesCount;
            this.datedSeriesCount = datedSeriesCount;
            this.dayCount = dayCount;
            this.monthCount = monthCount;
            this.yearCount = yearCount;
            this.undefinedCount = undefinedCount;
            this.acquisitionDate_First = acquisitionDate_First;
            this.acquisitionDate_Last = acquisitionDate_Last;
            this.createdAt_Last = createdAt_Last;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoDataAcquisitionCoverageResult"/> class by copying an existing one.
        /// </summary>
        /// <param name="ortoDataAcquisitionCoverageResult">The <see cref="OrtoDataAcquisitionCoverageResult"/> to copy from.</param>
        public OrtoDataAcquisitionCoverageResult(OrtoDataAcquisitionCoverageResult? ortoDataAcquisitionCoverageResult)
            : base(ortoDataAcquisitionCoverageResult)
        {
            if (ortoDataAcquisitionCoverageResult is not null)
            {
                countyId = ortoDataAcquisitionCoverageResult.countyId;
                seriesCount = ortoDataAcquisitionCoverageResult.seriesCount;
                withAcquisitionSeriesCount = ortoDataAcquisitionCoverageResult.withAcquisitionSeriesCount;
                datedSeriesCount = ortoDataAcquisitionCoverageResult.datedSeriesCount;
                dayCount = ortoDataAcquisitionCoverageResult.dayCount;
                monthCount = ortoDataAcquisitionCoverageResult.monthCount;
                yearCount = ortoDataAcquisitionCoverageResult.yearCount;
                undefinedCount = ortoDataAcquisitionCoverageResult.undefinedCount;
                acquisitionDate_First = ortoDataAcquisitionCoverageResult.acquisitionDate_First;
                acquisitionDate_Last = ortoDataAcquisitionCoverageResult.acquisitionDate_Last;
                createdAt_Last = ortoDataAcquisitionCoverageResult.createdAt_Last;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoDataAcquisitionCoverageResult"/> class from a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> containing the serialized data.</param>
        public OrtoDataAcquisitionCoverageResult(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the earliest acquisition date of the part's images, or null when no image is dated.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? AcquisitionDate_First => acquisitionDate_First;

        /// <summary>
        /// Gets the latest acquisition date of the part's images, or null when no image is dated.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? AcquisitionDate_Last => acquisitionDate_Last;

        /// <summary>
        /// Gets the identifier of the county part measured.
        /// </summary>
        [JsonIgnore]
        public int CountyId => countyId;

        /// <summary>
        /// Gets when an acquisition row of the part was last inserted or changed, or null when it has none: the last backfill that changed something, not the last one that ran.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? CreatedAt_Last => createdAt_Last;

        /// <summary>
        /// Gets how many series have at least one image an index sheet dated.
        /// </summary>
        [JsonIgnore]
        public long DatedSeriesCount => datedSeriesCount;

        /// <summary>
        /// Gets the images dated to the day.
        /// </summary>
        [JsonIgnore]
        public long DayCount => dayCount;

        /// <summary>
        /// Gets the images the acquisition rows describe, dated or not.
        /// <para>Derived from the four precision counts rather than counted separately, so it cannot disagree with them.</para>
        /// </summary>
        [JsonIgnore]
        public long ImageCount => dayCount + monthCount + yearCount + undefinedCount;

        /// <summary>
        /// Gets the images dated to the month.
        /// </summary>
        [JsonIgnore]
        public long MonthCount => monthCount;

        /// <summary>
        /// Gets the series (rows) <c>orto_datas</c> holds in the part.
        /// </summary>
        [JsonIgnore]
        public long SeriesCount => seriesCount;

        /// <summary>
        /// Gets the images no index sheet dated.
        /// </summary>
        [JsonIgnore]
        public long UndefinedCount => undefinedCount;

        /// <summary>
        /// Gets how many series have at least one acquisition row.
        /// </summary>
        [JsonIgnore]
        public long WithAcquisitionSeriesCount => withAcquisitionSeriesCount;

        /// <summary>
        /// Gets how many series have no acquisition row: the backfill backlog of the part.
        /// <para>Derived from the two stored counts rather than counted separately, so it cannot disagree with them.</para>
        /// </summary>
        [JsonIgnore]
        public long WithoutAcquisitionSeriesCount => seriesCount - withAcquisitionSeriesCount;

        /// <summary>
        /// Gets the images dated to the year only.
        /// </summary>
        [JsonIgnore]
        public long YearCount => yearCount;
    }
}
