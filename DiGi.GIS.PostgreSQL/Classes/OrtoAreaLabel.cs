using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// One interpretation of one epoch of a full-interpretation test area by one interpreter: a row of <see cref="Constants.TableName.OrtoAreaLabel"/> (DiGi.GIS.PostgreSQL#129).
    /// <para>The identity of a label is <c>(area, object identity, epoch, interpreter)</c>, where the object identity is exactly one of the footprint pair (<see cref="CountyId"/> with <see cref="Reference"/>) or <see cref="ObjectId"/>. Two interpreters file their own row for the same object, and the same interpreter re-upserting her or his row updates it in place. The table lives on the Main database, beside <c>building_2d</c>.</para>
    /// </summary>
    public class OrtoAreaLabel : SerializableObject, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(AreaKey))]
        private readonly string areaKey = string.Empty;

        [JsonInclude, JsonPropertyName(nameof(CountyId))]
        private readonly int? countyId;

        [JsonInclude, JsonPropertyName(nameof(EpochYear))]
        private readonly short epochYear;

        [JsonInclude, JsonPropertyName(nameof(Interpreter))]
        private readonly string interpreter = string.Empty;

        [JsonInclude, JsonPropertyName(nameof(Note))]
        private readonly string? note;

        [JsonInclude, JsonPropertyName(nameof(ObservationDateMax))]
        private readonly DateOnly? observationDateMax;

        [JsonInclude, JsonPropertyName(nameof(ObservationDateMin))]
        private readonly DateOnly? observationDateMin;

        [JsonInclude, JsonPropertyName(nameof(ObjectId))]
        private readonly string? objectId;

        [JsonInclude, JsonPropertyName(nameof(Reference))]
        private readonly string? reference;

        [JsonInclude, JsonPropertyName(nameof(State))]
        private readonly short state;

        [JsonInclude, JsonPropertyName(nameof(UpdatedAt))]
        private readonly DateTimeOffset updatedAt;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoAreaLabel"/> class.
        /// </summary>
        /// <param name="areaKey">The invariant location key of the test area the label was filed under.</param>
        /// <param name="countyId">The county part of the footprint identity, or null for a label of a drawn object.</param>
        /// <param name="reference">The building reference of the footprint identity, or null for a label of a drawn object.</param>
        /// <param name="objectId">The identifier of the drawn object the label was filed under, or null for a label of the footprint identity.</param>
        /// <param name="epochYear">The imagery epoch the label interprets.</param>
        /// <param name="state">What the interpreter saw: 0 present, 1 absent, 2 indeterminate.</param>
        /// <param name="observationDateMin">The earliest observation date of the epoch, or null when undated.</param>
        /// <param name="observationDateMax">The latest observation date of the epoch, or null when undated.</param>
        /// <param name="interpreter">The email of the user who filed the label.</param>
        /// <param name="note">The free-text note of the interpreter, or null when unnoted.</param>
        /// <param name="updatedAt">When the label was last filed or revised.</param>
        public OrtoAreaLabel(string? areaKey, int? countyId, string? reference, string? objectId, short epochYear, short state, DateOnly? observationDateMin, DateOnly? observationDateMax, string? interpreter, string? note, DateTimeOffset updatedAt)
        {
            this.areaKey = areaKey ?? string.Empty;
            this.countyId = countyId;
            this.reference = reference;
            this.objectId = objectId;
            this.epochYear = epochYear;
            this.state = state;
            this.observationDateMin = observationDateMin;
            this.observationDateMax = observationDateMax;
            this.interpreter = interpreter ?? string.Empty;
            this.note = note;
            this.updatedAt = updatedAt;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoAreaLabel"/> class as a copy of another instance.
        /// </summary>
        /// <param name="ortoAreaLabel">The instance to copy.</param>
        public OrtoAreaLabel(OrtoAreaLabel? ortoAreaLabel)
            : base(ortoAreaLabel)
        {
            if (ortoAreaLabel is not null)
            {
                areaKey = ortoAreaLabel.areaKey;
                countyId = ortoAreaLabel.countyId;
                reference = ortoAreaLabel.reference;
                objectId = ortoAreaLabel.objectId;
                epochYear = ortoAreaLabel.epochYear;
                state = ortoAreaLabel.state;
                observationDateMin = ortoAreaLabel.observationDateMin;
                observationDateMax = ortoAreaLabel.observationDateMax;
                interpreter = ortoAreaLabel.interpreter;
                note = ortoAreaLabel.note;
                updatedAt = ortoAreaLabel.updatedAt;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoAreaLabel"/> class from its JSON representation.
        /// </summary>
        /// <param name="jsonObject">The JSON object.</param>
        public OrtoAreaLabel(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the invariant location key of the test area the label was filed under.
        /// </summary>
        [JsonIgnore]
        public string AreaKey => areaKey;

        /// <summary>
        /// Gets the county part of the footprint identity, or null for a label of a drawn object.
        /// </summary>
        [JsonIgnore]
        public int? CountyId => countyId;

        /// <summary>
        /// Gets the imagery epoch the label interprets.
        /// </summary>
        [JsonIgnore]
        public short EpochYear => epochYear;

        /// <summary>
        /// Gets the email of the user who filed the label.
        /// </summary>
        [JsonIgnore]
        public string Interpreter => interpreter;

        /// <summary>
        /// Gets the free-text note of the interpreter, or null when unnoted.
        /// </summary>
        [JsonIgnore]
        public string? Note => note;

        /// <summary>
        /// Gets the latest observation date of the epoch, or null when undated.
        /// </summary>
        [JsonIgnore]
        public DateOnly? ObservationDateMax => observationDateMax;

        /// <summary>
        /// Gets the earliest observation date of the epoch, or null when undated.
        /// </summary>
        [JsonIgnore]
        public DateOnly? ObservationDateMin => observationDateMin;

        /// <summary>
        /// Gets the identifier of the drawn object the label was filed under, or null for a label of the footprint identity.
        /// </summary>
        [JsonIgnore]
        public string? ObjectId => objectId;

        /// <summary>
        /// Gets the building reference of the footprint identity, or null for a label of a drawn object.
        /// </summary>
        [JsonIgnore]
        public string? Reference => reference;

        /// <summary>
        /// Gets what the interpreter saw: 0 present, 1 absent, 2 indeterminate.
        /// </summary>
        [JsonIgnore]
        public short State => state;

        /// <summary>
        /// Gets when the label was last filed or revised.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset UpdatedAt => updatedAt;
    }
}
