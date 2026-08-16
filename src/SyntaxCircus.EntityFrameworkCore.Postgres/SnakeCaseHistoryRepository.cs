using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;

namespace SyntaxCircus.EntityFrameworkCore.Postgres;

// EF1001: NpgsqlHistoryRepository is an internal EF Core/Npgsql API. There's no supported public
// extension point for renaming the migrations-history table/columns, so subclassing the internal
// type (a well-established community workaround) is the only way to do this.
#pragma warning disable EF1001

/// <summary>
/// An <see cref="IHistoryRepository"/> that names the migrations-history table and its columns in
/// snake_case (<c>__ef_migrations_history</c> / <c>migration_id</c> / <c>product_version</c>),
/// matching the rest of the schema when using EFCore.NamingConventions' snake_case convention.
/// Register with
/// <c>optionsBuilder.ReplaceService&lt;IHistoryRepository, SnakeCaseHistoryRepository&gt;()</c>.
/// </summary>
public sealed class SnakeCaseHistoryRepository(HistoryRepositoryDependencies dependencies)
    : NpgsqlHistoryRepository(dependencies)
{
    protected override void ConfigureTable(EntityTypeBuilder<HistoryRow> history)
    {
        base.ConfigureTable(history);

        history.ToTable("__ef_migrations_history");
        history.Property(row => row.MigrationId).HasColumnName("migration_id");
        history.Property(row => row.ProductVersion).HasColumnName("product_version");
    }
}
