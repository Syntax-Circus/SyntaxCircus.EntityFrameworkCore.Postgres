namespace SyntaxCircus.EntityFrameworkCore.Postgres.Tests;

public class SnakeCaseNamingConventionTests
{
    [Fact]
    public void Model_WithSnakeCaseNamingConventionApplied_UsesSnakeCaseColumnNames()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .UseSnakeCaseNamingConvention()
            .Options;

        using var context = new TestDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(TestEntity))!;

        entityType.FindProperty(nameof(TestEntity.CreatedAt))!.GetColumnName().ShouldBe("created_at");
        entityType.FindProperty(nameof(TestEntity.Name))!.GetColumnName().ShouldBe("name");
    }
}
