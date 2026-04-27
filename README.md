# StrEnum.EntityFrameworkCore

Lets you use [StrEnum](https://github.com/StrEnum/StrEnum/) string enums with Entity Framework Core.

Supports EF Core 3.1 – 10.

## Installation

Install [StrEnum.EntityFrameworkCore](https://www.nuget.org/packages/StrEnum.EntityFrameworkCore/) via the .NET CLI:

```
dotnet add package StrEnum.EntityFrameworkCore
```

## Usage

### Defining a string enum and an entity

```csharp
public class Sport: StringEnum<Sport>
{
    public static readonly Sport RoadCycling = Define("ROAD_CYCLING");
    public static readonly Sport MountainBiking = Define("MTB");
    public static readonly Sport TrailRunning = Define("TRAIL_RUNNING");
}

public class Race
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Sport Sport { get; private set; }

    private Race()
    {
    }

    public Race(string name, Sport sport)
    {
        Id = Guid.NewGuid();
        Name = name;
        Sport = sport;
    }
}
```

### Wiring it up

Call `UseStringEnums()` when configuring your DB context:

```csharp
public class RaceContext: DbContext
{
    public DbSet<Race> Races { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(@"Server=.;Database=BestRaces;user id=*;pwd=*;")
            .UseStringEnums();
    }
}
```

That's it — EF Core can now read and write string enums.

### Migrations

EF Core stores string enums in non-nullable string columns (`NVARCHAR(MAX)` in SQL Server, `TEXT` in Postgres).

> If you'd like to store string enums as native Postgres enum types instead of `TEXT`, see [StrEnum.Npgsql.EntityFrameworkCore](https://github.com/StrEnum/StrEnum.Npgsql.EntityFrameworkCore/).

Running `dotnet ef migrations add Init` produces:

```csharp
migrationBuilder.CreateTable(
        name: "Races",
        columns: table => new
        {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Sport = table.Column<string>(type: "nvarchar(max)", nullable: false)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_Races", x => x.Id);
        });
```

To allow a nullable string enum, mark the property as non-required when configuring the entity:

```csharp
var race = builder.Entity<Race>();

race.Property(p => p.Sport).IsRequired(false);
```

### Querying

EF Core translates LINQ operations on string enums into SQL.

Add some races first:

```csharp
var context = new RaceContext();

await context.Database.EnsureDeletedAsync();
await context.Database.EnsureCreatedAsync();

var races = new[]
{
    new Race("Chornohora Sky Marathon", Sport.TrailRunning),
    new Race("Cape Town Cycle Tour", Sport.RoadCycling),
    new Race("Cape Epic", Sport.MountainBiking)
};

await context.Races.AddRangeAsync(races);

await context.SaveChangesAsync();
```

Filter by a single `Sport`:

```csharp
var trailRuns = await context.Races.Where(r => r.Sport == Sport.TrailRunning).ToArrayAsync();
```

This produces:

```sql
SELECT [r].[Id], [r].[Name], [r].[Sport]
FROM [Races] AS [r]
WHERE [r].[Sport] = N'TRAIL_RUNNING'
```

Or by multiple `Sport` values:

```csharp
var cyclingSports = new[] { Sport.MountainBiking, Sport.RoadCycling };

var cyclingRaces = await context.Races.Where(r => cyclingSports.Contains(r.Sport)).ToArrayAsync();
```

Which translates to:

```sql
SELECT [r].[Id], [r].[Name], [r].[Sport]
FROM [Races] AS [r]
WHERE [r].[Sport] IN (N'MTB', N'ROAD_CYCLING')
```

## Acknowledgements

Thanks to [Andrew Lock](https://andrewlock.net/strongly-typed-ids-in-ef-core-using-strongly-typed-entity-ids-to-avoid-primitive-obsession-part-4/) for his research on using a custom `ValueConverterSelector`.

## License

Copyright &copy; 2026 [Dmytro Khmara](https://dmytrokhmara.com).

StrEnum is licensed under the [MIT license](LICENSE.txt).
