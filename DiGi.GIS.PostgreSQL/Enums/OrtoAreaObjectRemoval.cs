using System.ComponentModel;

namespace DiGi.GIS.PostgreSQL.Enums
{
    /// <summary>
    /// Names the outcome of removing a polygon drawn in a full-interpretation test area (<see cref="Classes.OrtoAreaObjectPostgreSQLConverter.RemoveAsync(Npgsql.NpgsqlConnection?, string?, string?, string?, int, System.Threading.CancellationToken)"/>).
    /// <para>Drawn polygons are shared between interpreters, and their labels feed the agreement between them, so a removal is refused unless the caller drew the polygon and no other interpreter has labelled it. The reason is reported because it decides what the caller is told: a polygon someone else drew, or one someone else has already marked, has to stay.</para>
    /// </summary>
    [Description("OrtoAreaObjectRemoval")]
    public enum OrtoAreaObjectRemoval
    {
        /// <summary>
        /// The outcome is not known.
        /// </summary>
        [Description("Undefined")] Undefined = 0,

        /// <summary>
        /// The polygon and the caller's own labels on it were removed.
        /// </summary>
        [Description("Removed")] Removed = 1,

        /// <summary>
        /// The area holds no polygon under the identifier (also when it was removed already, or the table does not exist yet).
        /// </summary>
        [Description("Not Found")] NotFound = 2,

        /// <summary>
        /// Another interpreter drew the polygon; only the one who drew it may remove it.
        /// </summary>
        [Description("Not Creator")] NotCreator = 3,

        /// <summary>
        /// Another interpreter has labelled the polygon; removing it would delete their labels, so it stays.
        /// </summary>
        [Description("Labelled By Others")] LabelledByOthers = 4,
    }
}
