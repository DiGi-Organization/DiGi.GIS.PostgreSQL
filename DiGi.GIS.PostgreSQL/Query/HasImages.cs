using DiGi.GIS.PostgreSQL.Classes;
using System.Text.Json.Nodes;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Decides whether a stored orthophoto series holds at least one image.
        /// <para>Read from the serialized <c>Values</c> array of <see cref="TableSerializableObject{TSerializableObject}.Object"/> - the same key the SQL reads as <c>object-&gt;'Values'</c> - so the series is not deserialized. A series with no images is a fetch in which every request failed, not data (DiGi.GIS.WebAPI#81); <see cref="OrtoDatasPostgreSQLConverter.UpdateAsync(System.Collections.Generic.IEnumerable{OrtoDatas}, System.Collections.Generic.IEnumerable{int}, AdministrativeAreal2DPostgreSQLConverter, double, int, System.Threading.CancellationToken)"/> refuses to store one.</para>
        /// </summary>
        /// <param name="ortoDatas">The stored series.</param>
        /// <returns>True when the series holds at least one image; false when it is null, carries no object, or its <c>Values</c> array is missing or empty.</returns>
        public static bool HasImages(this OrtoDatas? ortoDatas)
        {
            return ortoDatas?.Object?["Values"] is JsonArray jsonArray && jsonArray.Count > 0;
        }
    }
}
