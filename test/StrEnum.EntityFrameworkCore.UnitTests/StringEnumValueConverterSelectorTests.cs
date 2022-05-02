using System;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace StrEnum.EntityFrameworkCore.UnitTests;

public class StringEnumValueConverterSelectorTests
{
    public class Country : StringEnum<Country>
    {
        public static readonly Country Ukraine = Define("UKR");
    }

    [Theory]
    [InlineData(typeof(Country), typeof(string))]
    [InlineData(typeof(Country), null)]
    public void Select_ShouldReturnCorrectValueConverter(Type modelType, Type? providerType)
    {
        var converterSelector = new StringEnumValueConverterSelector();

        var converters = converterSelector.Select(modelType, providerType).ToArray();

        converters.Should().HaveCount(1);

        var converterInfo = converters.Single();

        converterInfo.ModelClrType.Should().Be(modelType);
        converterInfo.ProviderClrType.Should().Be(typeof(string));
        
        var converter = converterInfo.Create();

        converter.Should().BeOfType<StringEnumValueConverter<Country>>();
    }

    [Fact]
    public void Select_ShouldOnlyInstantiateAValueConverterOnce()
    {
        var converterSelector = new StringEnumValueConverterSelector();

        var converters = converterSelector.Select(typeof(Country), null).ToArray();

        var converter1 = converters.Single().Create();

        converters = converterSelector.Select(typeof(Country), null).ToArray();

        var converter2 = converters.Single().Create();

        converter1.Should().BeSameAs(converter2);
    }

    [Theory]
    [InlineData(typeof(int), typeof(string))]
    [InlineData(typeof(Country), typeof(Guid))]
    public void Select_ShouldReturnNothingForWrongArguments(Type modelType, Type? providerType)
    {
        var converterSelector = new StringEnumValueConverterSelector();

        var converters = converterSelector.Select(modelType, providerType).ToArray();

        converters.Should().BeEmpty();
    }
}