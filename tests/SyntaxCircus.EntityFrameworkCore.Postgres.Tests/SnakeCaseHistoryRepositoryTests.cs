using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SyntaxCircus.EntityFrameworkCore.Postgres.Tests;

// Model-building and script generation don't require a live connection, so this is genuinely
// unit-testable against a fake connection string despite touching a Postgres-specific type.
public class SnakeCaseHistoryRepositoryTests
{
    private static TestDbContext CreateNpgsqlContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql("Host=fake;Database=fake;Username=fake;Password=fake", npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history"))
            .ReplaceService<IHistoryRepository, SnakeCaseHistoryRepository>()
            .Options;

        return new TestDbContext(options);
    }

    [Fact]
    public void GetCreateScript_UsesSnakeCaseTableAndColumnNames()
    {
        using var context = CreateNpgsqlContext();
        var historyRepository = context.GetService<IHistoryRepository>();

        var script = historyRepository.GetCreateScript();

        script.ShouldContain("__ef_migrations_history");
        script.ShouldContain("migration_id");
        script.ShouldContain("product_version");
    }

    [Fact]
    public void GetCreateIfNotExistsScript_UsesSnakeCaseTableAndColumnNames()
    {
        using var context = CreateNpgsqlContext();
        var historyRepository = context.GetService<IHistoryRepository>();

        var script = historyRepository.GetCreateIfNotExistsScript();

        script.ShouldContain("__ef_migrations_history");
        script.ShouldContain("migration_id");
        script.ShouldContain("product_version");
    }
}
