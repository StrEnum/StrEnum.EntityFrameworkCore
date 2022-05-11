# StrEnum.EntityFrameworkCore

Allows to use [StrEnum](https://github.com/StrEnum/StrEnum/) string enums with Entity Framework Core.

Supports EF Core 3.1, EF Core 5.0, and EF Core 6.0.

## How do I install it?

You can install [StrEnum.EntityFrameworkCore](https://www.nuget.org/packages/StrEnum.EntityFrameworkCore/) using the .NET CLI:

```
dotnet add package StrEnum.EntityFrameworkCore
```

Or using the Package Manager console:

```
PM> Install-Package StrEnum.EntityFrameworkCore
```

## How do I use it?

Define a string enum and an entity that uses it:

```csharp
public class SportsType: StringEnum<SportsType>
{
    public static readonly SportsType RoadCycling = Define("ROAD_CYCLING");
    public static readonly SportsType MountainBiking = Define("MTB");
    public static readonly SportsType TrailRunning = Define("TRAIL_RUNNING");
}

public class Race
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public SportsType Sport { get; private set; }

    private Race()
    {
    }

    public Race(string name, SportsType sport)
    {
        Id = Guid.NewGuid();
        Name = name;
        Sport = sport;
    }
}
```

And call the `UseStringEnums()` method when configuring your DB context:

```csharp
public class RaceContext: DbContext
{
    public DbSet<Race> Races { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=.;Database=BestRaces;user id=*;pwd=*;")
            .UseStringEnums();
    }
}
```

That's it! EF Core is now able to deal with string enums.

### Migrations

EF Core will store string enums in non-nullable string columns (`NVARCHAR(MAX)` in SQL Server, `TEXT` in Postgres). 

Running `dotnet ef migrations add Init` will produce the following migration:

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

In order to store a nullable string enum, mark the property is non-required when configuring your entity:

```csharp
var race = builder.Entity<Race>();

race.Property(p => p.Sport).IsRequired(false);
```

### Querying

EF Core will translate LINQ operations on string enums into SQL.

Let's add some races first:

```csharp
var context = new RaceContext();

await context.Database.EnsureDeletedAsync();
await context.Database.EnsureCreatedAsync();

var races = new[]
{
    new Race("Chornohora Sky Marathon", SportsType.TrailRunning),
    new Race("Cape Town Cycle Tour", SportsType.RoadCycling),
    new Race("Cape Epic", SportsType.MountainBiking)
};

await context.Races.AddRangeAsync(races);

await context.SaveChangesAsync();
```

And filter by a single SportsType:

```csharp
var trailRuns = await context.Races.Where(o => o.Sport == SportsType.TrailRunning).ToArrayAsync();
```

That will produce the following SQL:

```sql
SELECT [r].[Id], [r].[Name], [r].[Sport]
FROM [Races] AS [r]
WHERE [r].[Sport] = N'TRAIL_RUNNING'
```

You can also query by multiple SportsType values:

```csharp
var cyclingSports = new[] { SportsType.MountainBiking, SportsType.RoadCycling };

var racesThatRequireABicycle = await context.Races.Where(o => cyclingSports.Contains(o.Sport)).ToArrayAsync();
```
Which will translate to the following SQL:

```sql
SELECT [r].[Id], [r].[Name], [r].[Sport]
FROM [Races] AS [r]
WHERE [r].[Sport] IN (N'MTB', N'ROAD_CYCLING')
```

## Acknowledgements

Thanks to [Andrew Lock](https://andrewlock.net/strongly-typed-ids-in-ef-core-using-strongly-typed-entity-ids-to-avoid-primitive-obsession-part-4/) for his research on using custom `ValueConverterSelector`.

## License

Copyright &copy; 2022 [Dmitry Khmara](https://dmitrykhmara.com).

StrEnum is licensed under the [MIT license](LICENSE.txt).