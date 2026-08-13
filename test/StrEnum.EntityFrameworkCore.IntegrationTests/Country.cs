namespace StrEnum.EntityFrameworkCore.IntegrationTests;

public class Country : StringEnum<Country>
{
    public static readonly Country Ukraine = Define("UKR");
    public static readonly Country SouthAfrica = Define("ZAF");
    public static readonly Country Norway = Define("NOR");
}

/// <summary>
/// A national record, keyed on the composite of the country and the year it was set.
/// </summary>
public class NationalRecord
{
    public Country Country { get; set; } = null!;
    public int Year { get; set; }
    public string Athlete { get; set; } = "";
}
