# SyntaxCircus.EntityFrameworkCore.Postgres

[![Build](https://github.com/Syntax-Circus/SyntaxCircus.EntityFrameworkCore.Postgres/actions/workflows/build.yml/badge.svg)](https://github.com/Syntax-Circus/SyntaxCircus.EntityFrameworkCore.Postgres/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/SyntaxCircus.EntityFrameworkCore.Postgres.svg)](https://www.nuget.org/packages/SyntaxCircus.EntityFrameworkCore.Postgres)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.txt)

An auditable-entity base with a `TimeProvider`-driven `SaveChanges` interceptor, a Postgres advisory-lock-guarded migrate-on-startup helper, snake_case entity/column naming, and a snake_case migrations-history repository — for products on EF Core + Npgsql.

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

## Snake_case entity and column naming

This package depends on [`EFCore.NamingConventions`](https://github.com/efcore/EFCore.NamingConventions) and wraps its `UseSnakeCaseNamingConvention()`:

```csharp
optionsBuilder.UseNpgsql(connectionString)
    .UseSyntaxCircusSnakeCaseNamingConvention(); // entity and column names become snake_case, e.g. CreatedAt -> created_at
```

Use `UseSyntaxCircusSnakeCaseNamingConvention()`, not the raw `UseSnakeCaseNamingConvention()` from EFCore.NamingConventions directly - a naming-convention plugin applies to *every* model built from the context's convention pipeline, including the internal model EF Core uses for its own `__EFMigrationsHistory` migrations-history table. Calling the raw method alone silently renames that framework-owned table's columns as an unadvertised side effect (confirmed: it isn't a consumer-owned entity, so this is unintended - see `docs/enhancements/2026-08-17-missing-snake-case-naming-convention.md`). `UseSyntaxCircusSnakeCaseNamingConvention()` snake-cases your own entities the same way, while automatically keeping the migrations-history table in its EF Core framework-default naming (`__EFMigrationsHistory` / `MigrationId` / `ProductVersion`) unless you opt into renaming it too (below). One thing does still follow the convention: on a history table that EF creates, the primary key **constraint name** is snake_cased (`pk___ef_migrations_history` instead of `PK___EFMigrationsHistory`). The table and column names are what EF queries, and a history table created with the default names keeps working.

With the raw `UseSnakeCaseNamingConvention()`, a database whose history table was created with the default column names fails with `42703: column "migration_id" does not exist` as soon as EF reads its applied migrations.

`EFCore.NamingConventions` also ships camelCase, lower_case, and other convention variants — call its own `UseSnakeCaseNamingConvention()`/etc. directly if you want one of those without this package's migrations-history protection.

## Snake_case migrations history table

If you've opted into `UseSyntaxCircusSnakeCaseNamingConvention()` above and *do* want the migrations-history table itself (`__ef_migrations_history`, `migration_id`, `product_version`) renamed to match, you need **both** of these settings:

```csharp
optionsBuilder
    .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history")) // the table name
    .UseSyntaxCircusSnakeCaseNamingConvention()
    .ReplaceService<IHistoryRepository, SnakeCaseHistoryRepository>(); // the column names; registered after, so it wins
```

`SnakeCaseHistoryRepository` on its own renames only the columns: the table stays `__EFMigrationsHistory` unless you also call `MigrationsHistoryTable(...)`. Apply both everywhere the context is configured, including any design-time factory.

On a database that already has migrations applied under the old naming, you also need a one-time manual rename of the table and its columns, since EF has to read that table to know which migrations are applied before it can run one that would rename it.

## Contributing

Issues and pull requests are welcome:
- Keep changes focused, with a clear description of the behavior change.
- Match the existing code style (see `.editorconfig`).
- Call out any breaking changes to the public API in your PR description.

## License

MIT — see [LICENSE.txt](LICENSE.txt).
