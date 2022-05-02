using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace StrEnum.EntityFrameworkCore.UnitTests;

public class StringEnumValueConverterTests
{
    public class Country : StringEnum<Country>
    {
        public static readonly Country Ukraine = Define("UKR");
        public static readonly Country SouthAfrica = Define("ZAF");
    }

    public static IEnumerable<object?[]> FromStringTestCases =>
            new[]
            {
                new object?[] { "UKR", Country.Ukraine},
                new object?[] { "SouthAfrica", Country.SouthAfrica}
            };

    [Theory]
    [MemberData(nameof(FromStringTestCases))]
    public void ConvertFromString_ShouldConvertToStringEnumByNameOrValue(string value, Country expectedMember)
    {
        var converter = new StringEnumValueConverter<Country>();

        var actualCountry = converter.ConvertFromProvider(value);

        actualCountry.Should().Be(expectedMember);
    }

    [Fact]
    public void ConvertToString_ShouldConvertMemberValueToString()
    {
        var converter = new StringEnumValueConverter<Country>();

        var stringValue = converter.ConvertToProvider(Country.Ukraine);

        stringValue.Should().Be("UKR");
    }
}