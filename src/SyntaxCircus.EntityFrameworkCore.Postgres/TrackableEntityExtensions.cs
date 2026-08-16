namespace SyntaxCircus.EntityFrameworkCore.Postgres;

public static class TrackableEntityExtensions
{
    public static DbContextOptionsBuilder AddTrackableEntityInterceptor(
        this DbContextOptionsBuilder optionsBuilder,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);
        return optionsBuilder.AddInterceptors(new TrackableEntityInterceptor(timeProvider ?? TimeProvider.System));
    }
}
