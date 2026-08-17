using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;

namespace SyntaxCircus.EntityFrameworkCore.Postgres;

// EF1001: NpgsqlHistoryRepository is an internal EF Core/Npgsql API. There's no supported public
// extension point for renaming the migrations-history table/columns, so subclassing the internal
// type (a well-established community workaround, mirroring SnakeCaseHistoryRepository) is the
// only way to do this.
#pragma warning disable EF1001

/// <summary>
/// An <see cref="IHistoryRepository"/> that keeps the migrations-history table and its columns in
/// EF Core's own framework-default naming (<c>__EFMigrationsHistory</c> / <c>MigrationId</c> /
/// <c>ProductVersion</c>), regardless of any naming convention (e.g. EFCore.NamingConventions'
/// snake_case) registered on the same <see cref="DbContextOptionsBuilder"/>.
///
/// Naming-convention plugins apply to every model built from the context's convention set,
/// including the migrations-history model - so <c>UseSnakeCaseNamingConvention()</c> alone
/// silently renames <c>__EFMigrationsHistory</c>'s columns too, unless something explicitly
/// resets them. <see cref="UseSyntaxCircusSnakeCaseNamingConvention"/> registers this
/// automatically so that opting into snake_case for your own entities doesn't also rename a
/// framework-owned table out from under an already-deployed database. Consumers who *do* want the
/// migrations-history table renamed to match should opt into
/// <see cref="SnakeCaseHistoryRepository"/> instead, which is still available and still wins when
/// registered after this one.
/// </summary>
public sealed class DefaultCasingHistoryRepository(HistoryRepositoryDependencies dependencies)
    : NpgsqlHistoryRepository(dependencies)
{
    protected override void ConfigureTable(EntityTypeBuilder<HistoryRow> history)
    {
        base.ConfigureTable(history);

        history.ToTable("__EFMigrationsHistory");
        history.Property(row => row.MigrationId).HasColumnName(nameof(HistoryRow.MigrationId));
        history.Property(row => row.ProductVersion).HasColumnName(nameof(HistoryRow.ProductVersion));
    }
}
