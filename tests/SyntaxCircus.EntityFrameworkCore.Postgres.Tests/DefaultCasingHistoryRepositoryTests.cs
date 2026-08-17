using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SyntaxCircus.EntityFrameworkCore.Postgres.Tests;

// Model-building and script generation don't require a live connection, so this is genuinely
// unit-testable against a fake connection string despite touching a Postgres-specific type.
public class DefaultCasingHistoryRepositoryTests
{
    [Fact]
    public void UseSyntaxCircusSnakeCaseNamingConvention_snake_cases_entities_but_leaves_history_table_default()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql("Host=fake;Database=fake;Username=fake;Password=fake")
            .UseSyntaxCircusSnakeCaseNamingConvention()
            .Options;

        using var context = new TestDbContext(options);

        var entityType = context.Model.FindEntityType(typeof(TestEntity))!;
        entityType.FindProperty(nameof(TestEntity.CreatedAt))!.GetColumnName().ShouldBe("created_at");
        entityType.FindProperty(nameof(TestEntity.Name))!.GetColumnName().ShouldBe("name");

        var historyRepository = context.GetService<IHistoryRepository>();
        var script = historyRepository.GetCreateScript();

        script.ShouldContain("__EFMigrationsHistory");
        script.ShouldContain("MigrationId");
        script.ShouldContain("ProductVersion");
        script.ShouldNotContain("migration_id");
        script.ShouldNotContain("product_version");
    }

    [Fact]
    public void SnakeCaseHistoryRepository_can_still_be_opted_into_after_UseSyntaxCircusSnakeCaseNamingConvention()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql("Host=fake;Database=fake;Username=fake;Password=fake", npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history"))
            .UseSyntaxCircusSnakeCaseNamingConvention()
            .ReplaceService<IHistoryRepository, SnakeCaseHistoryRepository>()
            .Options;

        using var context = new TestDbContext(options);
        var historyRepository = context.GetService<IHistoryRepository>();

        var script = historyRepository.GetCreateScript();

        script.ShouldContain("__ef_migrations_history");
        script.ShouldContain("migration_id");
        script.ShouldContain("product_version");
    }
}
