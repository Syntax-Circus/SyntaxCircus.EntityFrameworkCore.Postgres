namespace SyntaxCircus.EntityFrameworkCore.Postgres.Tests.Infrastructure;

internal sealed class TestEntity : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}

internal sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public DbSet<TestEntity> Entities => Set<TestEntity>();
}
