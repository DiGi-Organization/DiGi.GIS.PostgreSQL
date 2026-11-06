using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Normalises a footprint's per-object provenance into the closed <c>primary_source</c> vocabulary the footprint tables store (DiGi.GIS.PostgreSQL#125).
        /// <para>Each dataset states its origin differently, so the evidence is read in a fixed order and the first match wins, making the result independent of key order: the reader's own dataset key (the OSM reader writes <c>dataset = "osm"</c>), then the Microsoft reader's <c>quadkey</c>, then the entries of an Overture <c>sources</c> array in their published order, then the source-valued columns - EUBUCCO's <c>*_source</c> columns and DBSM's <c>source</c> column, where a <c>gov-*</c> value is a register, a value containing <c>osm</c> is OpenStreetMap, a value containing <c>microsoft</c> is Microsoft and a value containing <c>esri</c> is Esri. A DBSM value of <c>ESA</c> classifies to none of them and becomes <c>other</c>, and so does a GHS-OBAT row, whose only evidence is the <c>overture_id</c> link. A row with no provenance at all becomes <c>other</c>, so the column is populated for every stored row.</para>
        /// <para>Among several source-valued columns the strongest evidence wins (<c>register</c> before <c>osm</c> before <c>microsoft</c> before <c>esri</c>), so an EUBUCCO row whose geometry comes from a register and whose height comes from OpenStreetMap is filed as the register that produced its geometry.</para>
        /// </summary>
        /// <param name="provenance">The per-object provenance, or null when the dataset defines none for the row.</param>
        /// <returns>One of the <see cref="Constants.PrimarySource"/> values; never null.</returns>
        public static string PrimarySource(JsonObject? provenance)
        {
            // Classifies one source-valued string, or null when it names none of the categories.
            static string? Classify(string? value)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return null;
                }

                if (value!.StartsWith("gov-", StringComparison.OrdinalIgnoreCase))
                {
                    return Constants.PrimarySource.Register;
                }

                if (value.IndexOf("osm", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return Constants.PrimarySource.Osm;
                }

                if (value.IndexOf("microsoft", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return Constants.PrimarySource.Microsoft;
                }

                if (value.IndexOf("esri", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return Constants.PrimarySource.Esri;
                }

                return null;
            }

            // Orders the vocabulary so the strongest evidence compares lowest: a register produced the geometry
            // itself, OpenStreetMap contributed to it, Microsoft delivered it, Esri imagery only informed it.
            static int Rank(string primarySource)
            {
                if (string.Equals(primarySource, Constants.PrimarySource.Register, StringComparison.Ordinal))
                {
                    return 0;
                }

                if (string.Equals(primarySource, Constants.PrimarySource.Osm, StringComparison.Ordinal))
                {
                    return 1;
                }

                if (string.Equals(primarySource, Constants.PrimarySource.Microsoft, StringComparison.Ordinal))
                {
                    return 2;
                }

                if (string.Equals(primarySource, Constants.PrimarySource.Esri, StringComparison.Ordinal))
                {
                    return 3;
                }

                return 4;
            }

            if (provenance is null)
            {
                return Constants.PrimarySource.Other;
            }

            // The OSM reader stamps its own dataset key on every footprint.
            if (provenance["dataset"] is JsonValue datasetValue && datasetValue.TryGetValue<string>(out string? dataset) && string.Equals(dataset, "osm", StringComparison.OrdinalIgnoreCase))
            {
                return Constants.PrimarySource.Osm;
            }

            // Microsoft features carry no origin of their own; the quadkey the file was fetched under is the source identity.
            if (provenance.ContainsKey("quadkey"))
            {
                return Constants.PrimarySource.Microsoft;
            }

            // Overture: the published sources array, first recognised entry wins.
            if (provenance["sources"] is JsonArray sources)
            {
                foreach (JsonNode? node in sources)
                {
                    string? source_Dataset = (node as JsonObject)?["dataset"] is JsonValue datasetValue_Source && datasetValue_Source.TryGetValue<string>(out string? text) ? text : null;

                    string? classified_Source = Classify(source_Dataset);
                    if (classified_Source is not null)
                    {
                        return classified_Source;
                    }
                }
            }

            // EUBUCCO's *_source columns and DBSM's source column: every value is weighed, strongest evidence first.
            string? best = null;
            foreach (KeyValuePair<string, JsonNode?> pair in provenance)
            {
                bool sourceColumn = pair.Key.IndexOf("_source", StringComparison.OrdinalIgnoreCase) >= 0
                    || string.Equals(pair.Key, "source", StringComparison.OrdinalIgnoreCase);

                if (!sourceColumn || pair.Value is not JsonValue value || !value.TryGetValue<string>(out string? text_Value))
                {
                    continue;
                }

                string? classified = Classify(text_Value);
                if (classified is not null && (best is null || Rank(classified) < Rank(best)))
                {
                    best = classified;
                }
            }

            return best ?? Constants.PrimarySource.Other;
        }
    }
}
