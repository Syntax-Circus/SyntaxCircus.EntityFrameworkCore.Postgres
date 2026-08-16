namespace SyntaxCircus.EntityFrameworkCore.Postgres;

public interface ITrackableEntity
{
    DateTimeOffset CreatedAt { get; set; }

    DateTimeOffset UpdatedAt { get; set; }
}
