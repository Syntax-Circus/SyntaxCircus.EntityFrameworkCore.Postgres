namespace SyntaxCircus.EntityFrameworkCore.Postgres.Tests;

// MigrateWithAdvisoryLockAsync's Npgsql code path (pg_advisory_lock/unlock, connection
// open/close) needs a real Postgres connection and is out of scope for this unit suite — see the
// package README/plan notes. Only the directly reachable, connection-free guard clause is tested
// here; the rest is integration-test territory (e.g. Testcontainers).
public class PostgresMigrateOnStartupExtensionsTests
{
    [Fact]
    public async Task MigrateWithAdvisoryLockAsync_NullContext_ThrowsArgumentNullException()
    {
        await Should.ThrowAsync<ArgumentNullException>(() =>
            PostgresMigrateOnStartupExtensions.MigrateWithAdvisoryLockAsync<TestDbContext>(null!, lockKey: 1));
    }
}
