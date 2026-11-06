using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Represents a background task that creates the four full-interpretation test-area tables in PostgreSQL.
    /// <para>Runs the exact DDL the converters run at the top of every <c>UpdateAsync</c> (<c>TableAsync_OrtoArea</c> and its siblings), so nothing here diverges from the schema. <c>orto_area</c>, <c>orto_area_object</c> and <c>orto_area_label</c> are created on the Main database and <c>orto_area_image</c> on the Storage database, each followed by an <c>ANALYZE</c>. Every statement is idempotent, so re-running the task is a no-op. This is the explicit, repeatable materialisation path for the test-area tables (DiGi.GIS.PostgreSQL#130).</para>
    /// </summary>
    public class PostgreSQLCreateOrtoAreaTablesTask : BackgroundTask, IGISPostgreSQLObject
    {
        /// <summary>
        /// Gets the OrtoArea PostgreSQL converter used to create the <c>orto_area</c> table.
        /// </summary>
        private readonly OrtoAreaPostgreSQLConverter ortoAreaPostgreSQLConverter;

        /// <summary>
        /// Gets the OrtoAreaObject PostgreSQL converter used to create the <c>orto_area_object</c> table.
        /// </summary>
        private readonly OrtoAreaObjectPostgreSQLConverter ortoAreaObjectPostgreSQLConverter;

        /// <summary>
        /// Gets the OrtoAreaLabel PostgreSQL converter used to create the <c>orto_area_label</c> table.
        /// </summary>
        private readonly OrtoAreaLabelPostgreSQLConverter ortoAreaLabelPostgreSQLConverter;

        /// <summary>
        /// Gets the OrtoAreaImage PostgreSQL converter used to create the <c>orto_area_image</c> table.
        /// </summary>
        private readonly OrtoAreaImagePostgreSQLConverter ortoAreaImagePostgreSQLConverter;

        /// <summary>
        /// Constructor with Dependency Injection.
        /// </summary>
        /// <param name="ortoAreaPostgreSQLConverter">The converter for the <c>orto_area</c> table (Main).</param>
        /// <param name="ortoAreaObjectPostgreSQLConverter">The converter for the <c>orto_area_object</c> table (Main).</param>
        /// <param name="ortoAreaLabelPostgreSQLConverter">The converter for the <c>orto_area_label</c> table (Main).</param>
        /// <param name="ortoAreaImagePostgreSQLConverter">The converter for the <c>orto_area_image</c> table (Storage).</param>
        public PostgreSQLCreateOrtoAreaTablesTask(OrtoAreaPostgreSQLConverter ortoAreaPostgreSQLConverter, OrtoAreaObjectPostgreSQLConverter ortoAreaObjectPostgreSQLConverter, OrtoAreaLabelPostgreSQLConverter ortoAreaLabelPostgreSQLConverter, OrtoAreaImagePostgreSQLConverter ortoAreaImagePostgreSQLConverter)
        {
            this.ortoAreaPostgreSQLConverter = ortoAreaPostgreSQLConverter ?? throw new ArgumentNullException(nameof(ortoAreaPostgreSQLConverter));
            this.ortoAreaObjectPostgreSQLConverter = ortoAreaObjectPostgreSQLConverter ?? throw new ArgumentNullException(nameof(ortoAreaObjectPostgreSQLConverter));
            this.ortoAreaLabelPostgreSQLConverter = ortoAreaLabelPostgreSQLConverter ?? throw new ArgumentNullException(nameof(ortoAreaLabelPostgreSQLConverter));
            this.ortoAreaImagePostgreSQLConverter = ortoAreaImagePostgreSQLConverter ?? throw new ArgumentNullException(nameof(ortoAreaImagePostgreSQLConverter));
        }

        /// <summary>
        /// Executes the background task that creates the four test-area tables (and their indexes), then analyzes each.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. Returns true only if all four tables were created successfully; otherwise, false.</returns>
        protected override async Task<bool> ExecuteAsync()
        {
            bool area = await this.ortoAreaPostgreSQLConverter.CreateTableAsync(0);
            bool areaObject = await this.ortoAreaObjectPostgreSQLConverter.CreateTableAsync(0);
            bool areaLabel = await this.ortoAreaLabelPostgreSQLConverter.CreateTableAsync(0);
            bool areaImage = await this.ortoAreaImagePostgreSQLConverter.CreateTableAsync(0);

            return area && areaObject && areaLabel && areaImage;
        }
    }
}
