namespace SyntaxCircus.EntityFrameworkCore.Postgres.Tests;

public class TrackableEntityExtensionsTests
{
    [Fact]
    public void AddTrackableEntityInterceptor_NullOptionsBuilder_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            TrackableEntityExtensions.AddTrackableEntityInterceptor(null!));
    }

    [Fact]
    public async Task AddTrackableEntityInterceptor_NoTimeProviderSupplied_StillStampsEntities()
    {
        var builder = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());
        builder.AddTrackableEntityInterceptor();

        await using var context = new TestDbContext(builder.Options);
        context.Entities.Add(new TestEntity { Name = "a" });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var saved = context.Entities.Single();
        saved.CreatedAt.ShouldNotBe(default);
        (DateTimeOffset.UtcNow - saved.CreatedAt).ShouldBeLessThan(TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task AddTrackableEntityInterceptor_CustomTimeProviderSupplied_UsesIt()
    {
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var builder = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());
        builder.AddTrackableEntityInterceptor(new FixedTimeProvider(now));

        await using var context = new TestDbContext(builder.Options);
        context.Entities.Add(new TestEntity { Name = "a" });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        context.Entities.Single().CreatedAt.ShouldBe(now);
    }
}
