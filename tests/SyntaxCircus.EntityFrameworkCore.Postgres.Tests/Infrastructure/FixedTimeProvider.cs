namespace SyntaxCircus.EntityFrameworkCore.Postgres.Tests.Infrastructure;

/// <summary>A <see cref="TimeProvider"/> whose "now" is settable, so a single test can simulate time passing between saves.</summary>
internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public override DateTimeOffset GetUtcNow() => Now;
}
