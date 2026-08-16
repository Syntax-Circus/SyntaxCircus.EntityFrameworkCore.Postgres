# SyntaxCircus.EntityFrameworkCore.Postgres

[![Build](https://github.com/Syntax-Circus/SyntaxCircus.EntityFrameworkCore.Postgres/actions/workflows/build.yml/badge.svg)](https://github.com/Syntax-Circus/SyntaxCircus.EntityFrameworkCore.Postgres/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.txt)

An auditable-entity base with a `TimeProvider`-driven `SaveChanges` interceptor, a Postgres advisory-lock-guarded migrate-on-startup helper, and a snake_case migrations-history repository — for products on EF Core + Npgsql.

> **No support guaranteed.** Published as-is and maintained on a best-effort basis. Issues and PRs are welcome, but there's no SLA — fork it or vendor what you need if that's not enough.

## Auditable entities

```csharp
public sealed class Widget : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}

optionsBuilder.UseNpgsql(connectionString);
optionsBuilder.AddTrackableEntityInterceptor(); // stamps CreatedAt/UpdatedAt on save
```

`AuditableEntity` gives you `Id`, `CreatedAt`, `UpdatedAt`. If you already have a base entity, just implement `ITrackableEntity` (`CreatedAt`/`UpdatedAt`) instead — the interceptor works off the interface, not the base class. `AddTrackableEntityInterceptor(timeProvider)` takes an optional `TimeProvider` for testability; defaults to `TimeProvider.System`.

## Migrate on startup, safely

```csharp
await using var scope = app.Services.CreateAsyncScope();
var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
await context.MigrateWithAdvisoryLockAsync(lockKey: 823_471); // any consistent int64 for this DbContext
```

Takes a Postgres advisory lock before calling `Database.MigrateAsync()`, so multiple instances of the same service starting up concurrently don't race to apply migrations. Falls back to a plain, lock-free migrate for non-Postgres providers.

## Snake_case migrations history table

If you're using `EFCore.NamingConventions`' `UseSnakeCaseNamingConvention()` and want the migrations-history table itself (`__ef_migrations_history`, `migration_id`, `product_version`) in the same convention:

```csharp
optionsBuilder.UseNpgsql(connectionString)
    .UseSnakeCaseNamingConvention()
    .ReplaceService<IHistoryRepository, SnakeCaseHistoryRepository>();
```

## Contributing

Issues and pull requests are welcome:
- Keep changes focused, with a clear description of the behavior change.
- Match the existing code style (see `.editorconfig`).
- Call out any breaking changes to the public API in your PR description.

## License

MIT — see [LICENSE.txt](LICENSE.txt).
