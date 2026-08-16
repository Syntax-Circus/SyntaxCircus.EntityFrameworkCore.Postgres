namespace SyntaxCircus.EntityFrameworkCore.Postgres.Tests;

public class TrackableEntityInterceptorTests
{
    private static (TestDbContext Context, FixedTimeProvider TimeProvider) CreateContext(DateTimeOffset now)
    {
        var timeProvider = new FixedTimeProvider(now);
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new TrackableEntityInterceptor(timeProvider))
            .Options;

        return (new TestDbContext(options), timeProvider);
    }

    [Fact]
    public async Task SaveChangesAsync_AddedEntity_StampsCreatedAtAndUpdatedAt()
    {
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var (context, _) = CreateContext(now);
        await using var _ = context;

        context.Entities.Add(new TestEntity { Name = "a" });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var saved = context.Entities.Single();
        saved.CreatedAt.ShouldBe(now);
        saved.UpdatedAt.ShouldBe(now);
    }

    [Fact]
    public void SaveChanges_AddedEntity_StampsCreatedAtAndUpdatedAt()
    {
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var (context, _) = CreateContext(now);
        using var _ = context;

        context.Entities.Add(new TestEntity { Name = "a" });
        context.SaveChanges();

        var saved = context.Entities.Single();
        saved.CreatedAt.ShouldBe(now);
        saved.UpdatedAt.ShouldBe(now);
    }

    [Fact]
    public async Task SaveChangesAsync_ModifiedEntity_OnlyStampsUpdatedAt()
    {
        var createdAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var updatedAt = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var (context, timeProvider) = CreateContext(createdAt);
        await using var _ = context;

        var entity = new TestEntity { Name = "a" };
        context.Entities.Add(entity);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        timeProvider.Now = updatedAt;
        entity.Name = "b";
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        entity.CreatedAt.ShouldBe(createdAt);
        entity.UpdatedAt.ShouldBe(updatedAt);
    }

    [Fact]
    public async Task SaveChangesAsync_DeletedEntity_NotStamped()
    {
        var createdAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var (context, _) = CreateContext(createdAt);
        await using var _ = context;

        var entity = new TestEntity { Name = "a" };
        context.Entities.Add(entity);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        context.Entities.Remove(entity);

        await Should.NotThrowAsync(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SaveChangesAsync_MultipleAddedEntities_AllStampedWithSameNow()
    {
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var (context, _) = CreateContext(now);
        await using var _ = context;

        context.Entities.Add(new TestEntity { Name = "a" });
        context.Entities.Add(new TestEntity { Name = "b" });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        foreach (var entity in context.Entities)
        {
            entity.CreatedAt.ShouldBe(now);
        }
    }
}
