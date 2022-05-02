using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace StrEnum.EntityFrameworkCore
{
    /// <summary>
    /// Provides the way to convert string enum members' values from and to strings
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    public class StringEnumValueConverter<TEnum> : ValueConverter<StringEnum<TEnum>, string>
        where TEnum : StringEnum<TEnum>, new()
    {
        public StringEnumValueConverter(ConverterMappingHints? mappingHints = null)
            : base(@enum => (string)@enum, value => StringEnum<TEnum>.Parse(value), mappingHints)
        {
        }
    }
}