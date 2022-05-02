using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace StrEnum.EntityFrameworkCore;

/// <summary>
/// Allows Entity Framework to receive value converters from both the default <see cref="ValueConverterSelector"/> and the <see cref="StringEnumValueConverterSelector"/>.
/// </summary>
internal class ChainedValueConverterSelectorDecorator : IValueConverterSelector
{
    private readonly ValueConverterSelector _defaultSelector;
    private readonly StringEnumValueConverterSelector _stringEnumSelector;

    public ChainedValueConverterSelectorDecorator(ValueConverterSelectorDependencies defaultSelectorDependencies)
    {
        _defaultSelector = new ValueConverterSelector(defaultSelectorDependencies);
        _stringEnumSelector = new StringEnumValueConverterSelector();
    }

    public IEnumerable<ValueConverterInfo> Select(Type modelClrType, Type? providerClrType = null)
    {
        var defaultConverters = _defaultSelector.Select(modelClrType, providerClrType);
        var stringEnumConverters = _stringEnumSelector.Select(modelClrType, providerClrType);

        foreach (var converterInfo in defaultConverters.Concat(stringEnumConverters))
        {
            yield return converterInfo;
        }
    }
}