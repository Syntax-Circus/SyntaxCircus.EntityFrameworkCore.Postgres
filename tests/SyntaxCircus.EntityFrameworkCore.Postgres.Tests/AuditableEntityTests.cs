namespace SyntaxCircus.EntityFrameworkCore.Postgres.Tests;

public class AuditableEntityTests
{
    [Fact]
    public void Id_DefaultsToNonEmptyUniqueGuid()
    {
        var first = new TestEntity();
        var second = new TestEntity();

        first.Id.ShouldNotBe(Guid.Empty);
        first.Id.ShouldNotBe(second.Id);
    }

    [Fact]
    public void CreatedAtAndUpdatedAt_AreSettable()
    {
        var entity = new TestEntity();
        var now = DateTimeOffset.UtcNow;

        entity.CreatedAt = now;
        entity.UpdatedAt = now;

        entity.CreatedAt.ShouldBe(now);
        entity.UpdatedAt.ShouldBe(now);
    }
}
