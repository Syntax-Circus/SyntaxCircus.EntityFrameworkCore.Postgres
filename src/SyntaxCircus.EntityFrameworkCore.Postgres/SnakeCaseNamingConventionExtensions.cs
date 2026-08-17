using Microsoft.EntityFrameworkCore.Migrations;

namespace SyntaxCircus.EntityFrameworkCore.Postgres;

public static class SnakeCaseNamingConventionExtensions
{
    /// <summary>
    /// Applies EFCore.NamingConventions' snake_case convention to your own entities/columns, while
    /// leaving the EF Core migrations-history table (<c>__EFMigrationsHistory</c>) in its
    /// framework-default naming - it's not an entity you own, so opting into snake_case for your
    /// model shouldn't silently rename it too. This is the difference from calling the raw
    /// <c>UseSnakeCaseNamingConvention()</c> (from EFCore.NamingConventions) directly, which
    /// renames the migrations-history table's columns as an unadvertised side effect since it
    /// shares the same model-building convention pipeline.
    ///
    /// If you *do* want the migrations-history table renamed to match (<c>__ef_migrations_history</c>
    /// / <c>migration_id</c> / <c>product_version</c>), call
    /// <c>.ReplaceService&lt;IHistoryRepository, SnakeCaseHistoryRepository&gt;()</c> after this -
    /// it's registered later, so it wins.
    /// </summary>
    public static DbContextOptionsBuilder UseSyntaxCircusSnakeCaseNamingConvention(this DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
        optionsBuilder.ReplaceService<IHistoryRepository, DefaultCasingHistoryRepository>();
        return optionsBuilder;
    }

    /// <summary>Generic-preserving overload, so this composes in a fluent chain the same way EFCore.NamingConventions' own overloads do.</summary>
    public static DbContextOptionsBuilder<TContext> UseSyntaxCircusSnakeCaseNamingConvention<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder)
        where TContext : DbContext
    {
        ((DbContextOptionsBuilder)optionsBuilder).UseSyntaxCircusSnakeCaseNamingConvention();
        return optionsBuilder;
    }
}
