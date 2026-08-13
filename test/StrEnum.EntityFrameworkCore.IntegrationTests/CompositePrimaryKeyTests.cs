using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace StrEnum.EntityFrameworkCore.IntegrationTests;

/// <summary>
/// Verifies that a string enum can take part in a composite primary key end to end: schema creation,
/// insert, lookup by the composite key, update, and query. EF Core orders key values internally, so it
/// rejects key types that aren't comparable — <see cref="StringEnum{TEnum}"/> implements
/// <see cref="IComparable{T}"/> and <see cref="IComparable"/> as of StrEnum 2.1.0.
/// </summary>
public class CompositePrimaryKeyTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public CompositePrimaryKeyTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    public void Dispose() => _connection.Dispose();

    private class RecordsContext : DbContext
    {
        private readonly SqliteConnection _connection;

        public DbSet<NationalRecord> NationalRecords => Set<NationalRecord>();

        public RecordsContext(SqliteConnection connection) => _connection = connection;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlite(_connection)
                .UseStringEnums();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NationalRecord>()
                .HasKey(r => new { r.Country, r.Year });
        }
    }

    private RecordsContext CreateContext() => new(_connection);

    [Fact]
    public void Builds_a_composite_primary_key_that_includes_a_string_enum()
    {
        using var context = CreateContext();

        var primaryKey = context.Model.FindEntityType(typeof(NationalRecord))!.FindPrimaryKey()!;

        primaryKey.Properties.Select(p => p.Name).Should().Equal(nameof(NationalRecord.Country), nameof(NationalRecord.Year));

        var countryProperty = primaryKey.Properties.First();

        countryProperty.ClrType.Should().Be<Country>();

        // UseStringEnums supplies the converter through the value converter selector rather than an
        // explicit HasConversion call, so it surfaces on the type mapping.
        countryProperty.GetTypeMapping().Converter.Should().BeOfType<StringEnumValueConverter<Country>>();
    }

    [Fact]
    public async Task Round_trips_a_row_keyed_on_a_string_enum()
    {
        await using (var seedContext = CreateContext())
        {
            await seedContext.Database.EnsureCreatedAsync();

            seedContext.NationalRecords.AddRange(
                new NationalRecord { Country = Country.Ukraine,     Year = 2024, Athlete = "Olha Bilyk" },
                new NationalRecord { Country = Country.SouthAfrica, Year = 2024, Athlete = "Thabo Nkosi" },
                new NationalRecord { Country = Country.Norway,      Year = 2025, Athlete = "Ingrid Dahl" });

            await seedContext.SaveChangesAsync();
        }

        // Look the row up by the composite key, which is the path that orders key values internally.
        await using (var updateContext = CreateContext())
        {
            var record = await updateContext.NationalRecords.FindAsync(Country.SouthAfrica, 2024);

            record!.Athlete.Should().Be("Thabo Nkosi");

            record.Athlete = "Thabo Nkosi-Mokoena";

            await updateContext.SaveChangesAsync();
        }

        await using var queryContext = CreateContext();

        var records = await queryContext.NationalRecords
            .Where(r => r.Year == 2024)
            .ToArrayAsync();

        records.Should().HaveCount(2);
        records.Single(r => r.Country == Country.SouthAfrica).Athlete.Should().Be("Thabo Nkosi-Mokoena");
        records.Single(r => r.Country == Country.Ukraine).Athlete.Should().Be("Olha Bilyk");
    }

    [Fact]
    public async Task Stores_the_key_column_as_the_member_value()
    {
        await using var context = CreateContext();

        await context.Database.EnsureCreatedAsync();

        context.NationalRecords.Add(new NationalRecord { Country = Country.Ukraine, Year = 2026, Athlete = "Olha Bilyk" });

        await context.SaveChangesAsync();

        await using var command = _connection.CreateCommand();
        command.CommandText = "SELECT \"Country\" FROM \"NationalRecords\" WHERE \"Year\" = 2026";

        var storedValue = await command.ExecuteScalarAsync();

        storedValue.Should().Be("UKR");
    }
}
