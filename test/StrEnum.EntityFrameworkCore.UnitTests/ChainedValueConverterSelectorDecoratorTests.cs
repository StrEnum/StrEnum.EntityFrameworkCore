using System;
using System.Linq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

namespace StrEnum.EntityFrameworkCore.UnitTests;

public class ChainedValueConverterSelectorDecoratorTests
{
    [Fact]
    public void Select_SelectsDefaultValueConverters()
    {
        var decorator = new ChainedValueConverterSelectorDecorator(new ValueConverterSelectorDependencies());

        // Act
        var converters = decorator.Select(typeof(Guid), typeof(string)).ToArray();

        converters.Should().HaveCount(1);

        var converter = converters.Single();

        converter.ModelClrType.Should().Be<Guid>();
        converter.ProviderClrType.Should().Be<string>();
    }

    public class Country: StringEnum<Country>
    {
    }

    [Fact]
    public void Select_SelectsStringEnumValueConverters()
    {
        var decorator = new ChainedValueConverterSelectorDecorator(new ValueConverterSelectorDependencies());

        // Act
        var converters = decorator.Select(typeof(Country), typeof(string)).ToArray();

        converters.Should().HaveCount(1);

        var converter = converters.Single();

        converter.ModelClrType.Should().Be<Country>();
        converter.ProviderClrType.Should().Be<string>();
    }
}