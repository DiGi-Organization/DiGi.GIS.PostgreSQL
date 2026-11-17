using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// The id and location of one <c>epw_file</c> row, without the weather file itself.
    /// <para>What a client positioning many buildings needs to pick each one's station in memory, as the server-side solar solve does with <see cref="Query.NearestEPWFileId"/>, and then to read only the stations it uses by id (DiGi.GIS.WebAPI#93).</para>
    /// </summary>
    public class EPWFileLocationResult : SerializableResult, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(Id))]
        private readonly int id;

        [JsonInclude, JsonPropertyName(nameof(X))]
        private readonly double x;

        [JsonInclude, JsonPropertyName(nameof(Y))]
        private readonly double y;

        /// <summary>
        /// Initializes a new instance of the <see cref="EPWFileLocationResult"/> class.
        /// </summary>
        /// <param name="id">The id of the <c>epw_file</c> row.</param>
        /// <param name="x">The X coordinate of the station, EPSG:2180.</param>
        /// <param name="y">The Y coordinate of the station, EPSG:2180.</param>
        public EPWFileLocationResult(int id, double x, double y)
        {
            this.id = id;
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EPWFileLocationResult"/> class by copying an existing one.
        /// </summary>
        /// <param name="ePWFileLocationResult">The <see cref="EPWFileLocationResult"/> to copy from.</param>
        public EPWFileLocationResult(EPWFileLocationResult? ePWFileLocationResult)
            : base(ePWFileLocationResult)
        {
            if (ePWFileLocationResult is not null)
            {
                id = ePWFileLocationResult.id;
                x = ePWFileLocationResult.x;
                y = ePWFileLocationResult.y;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EPWFileLocationResult"/> class from a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> containing the serialized data.</param>
        public EPWFileLocationResult(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the id of the <c>epw_file</c> row.
        /// </summary>
        [JsonIgnore]
        public int Id => id;

        /// <summary>
        /// Gets the X coordinate of the station, EPSG:2180.
        /// </summary>
        [JsonIgnore]
        public double X => x;

        /// <summary>
        /// Gets the Y coordinate of the station, EPSG:2180.
        /// </summary>
        [JsonIgnore]
        public double Y => y;
    }
}
