using System.Data;
using Npgsql;

namespace SyntaxCircus.EntityFrameworkCore.Postgres;

public static class PostgresMigrateOnStartupExtensions
{
    /// <summary>
    /// Runs <c>Database.MigrateAsync()</c> guarded by a Postgres advisory lock, so multiple
    /// concurrently-starting instances of the same service don't race to apply migrations.
    /// Falls back to a plain (lock-free) migrate for non-Postgres providers.
    /// </summary>
    public static async Task MigrateWithAdvisoryLockAsync<TContext>(
        this TContext context,
        long lockKey,
        CancellationToken cancellationToken = default)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) != true)
        {
            await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        var connection = (NpgsqlConnection)context.Database.GetDbConnection();
        var ownsConnection = connection.State != ConnectionState.Open;
        if (ownsConnection)
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        try
        {
            await using (var lockCommand = connection.CreateCommand())
            {
                lockCommand.CommandText = "SELECT pg_advisory_lock(@key)";
                lockCommand.Parameters.AddWithValue("key", lockKey);
                await lockCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            try
            {
                await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await using var unlockCommand = connection.CreateCommand();
                unlockCommand.CommandText = "SELECT pg_advisory_unlock(@key)";
                unlockCommand.Parameters.AddWithValue("key", lockKey);
                await unlockCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            if (ownsConnection)
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }
    }
}
