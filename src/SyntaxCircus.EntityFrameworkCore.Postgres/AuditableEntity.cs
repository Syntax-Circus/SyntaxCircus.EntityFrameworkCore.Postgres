namespace SyntaxCircus.EntityFrameworkCore.Postgres;

public abstract class AuditableEntity : ITrackableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
