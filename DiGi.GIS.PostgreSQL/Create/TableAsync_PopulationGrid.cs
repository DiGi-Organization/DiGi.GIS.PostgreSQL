using Npgsql;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Create
    {
        /// <summary>
        /// Asynchronously creates the census population grid table in the PostgreSQL database.
        /// <para>One row per cell of the GUS 1 km population grid of one census year (DiGi.GIS.PostgreSQL#106). <c>population</c> is null where statistical secrecy hides the cell's value - a hidden cell is never stored as zero, because a comparison reading it as zero would invent an error of exactly the modelled sum. The envelope columns (<c>min_x</c> … <c>max_y</c>, EPSG:2180, easting in X) carry the cell box for the GiST prefilter and <c>object</c> carries the whole <see cref="DiGi.Geometry.Planar.Classes.PolygonalFace2D"/>, so a point-in-cell test runs in C# the way every other geometry read in this repository does - the schema has no PostGIS. The table lives on the Main database, beside <c>building_2d</c>, because a comparison statement reads the grid, the buildings, the occupancy and the county outline on one database (DiGi.GIS.PostgreSQL#106). Every statement is idempotent.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table was created successfully; otherwise, false.</returns>
        public static async Task<bool> TableAsync_PopulationGrid(this NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = $@"
                CREATE TABLE IF NOT EXISTS {Constants.TableName.PopulationGrid} (
                    id BIGINT GENERATED ALWAYS AS IDENTITY,
                    cell_id TEXT NOT NULL,
                    census_year SMALLINT NOT NULL,
                    population INT,
                    min_x DOUBLE PRECISION,
                    min_y DOUBLE PRECISION,
                    max_x DOUBLE PRECISION,
                    max_y DOUBLE PRECISION,
                    object JSONB,
                    created_at timestamptz DEFAULT now(),
                    PRIMARY KEY (id),
                    UNIQUE (cell_id, census_year)
                );

                -- Spatial prefilter: the box columns answer which cells reach a county's bounding box,
                -- the same GiST box index shape building_2d uses for its own bbox searches.
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.PopulationGrid}_bbox
                ON {Constants.TableName.PopulationGrid} USING gist (box(point(min_x, min_y), point(max_x, max_y)));

                -- Serves the census-year filter of every county read.
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.PopulationGrid}_census_year
                ON {Constants.TableName.PopulationGrid} (census_year);
                ";

            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
                return true;
            }
            catch (NpgsqlException npgsqlException)
            {
                Serilog.Modify.Log(npgsqlException, "{Method} failed for table {TableName}", nameof(TableAsync_PopulationGrid), Constants.TableName.PopulationGrid);
                return false;
            }
        }
    }
}
