using Npgsql;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Energy
{
    public static partial class Create
    {
        /// <summary>
        /// Asynchronously creates the partition of the <see cref="Constants.TableName.BuildingHeatDemand"/> table for one county part (DiGi.GIS.PostgreSQL#143).
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="countyId">The id of the county part the partition holds.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the partition exists afterwards; otherwise, false.</returns>
        public static async Task<bool> TableAsync_BuildingHeatDemand_Partition(this NpgsqlConnection? npgsqlConnection, int countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = PostgreSQL.Query.PartitionCommandText(Constants.TableName.BuildingHeatDemand, countyId);

            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
                return true;
            }
            catch (NpgsqlException ex)
            {
                Serilog.Modify.Log(ex, "{Type}: the partition of county part {CountyId} could not be created", nameof(TableAsync_BuildingHeatDemand_Partition), countyId);
                return false;
            }
        }
    }
}
