using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace StrEnum.EntityFrameworkCore
{
    /// <summary>
    /// Allows Entity Framework to use an appropriate value converter for the given string enum.
    /// </summary>
    public class StringEnumValueConverterSelector : IValueConverterSelector
    {
        private readonly ConcurrentDictionary<Type, ValueConverterInfo> _converters = new();

        public IEnumerable<ValueConverterInfo> Select(Type modelClrType, Type? providerClrType = null)
        {
            var underlyingModelType = UnwrapNullableType(modelClrType);
            var underlyingProviderType = providerClrType != null ? UnwrapNullableType(providerClrType) : null;

            if (underlyingProviderType is null || underlyingProviderType == typeof(string))
            {
                if (_converters.TryGetValue(underlyingModelType, out var cachedConverter))
                {
                    return new[] { cachedConverter };
                }

                if (IsStringEnum(underlyingModelType))
                {
                    var converter = _converters.GetOrAdd(underlyingModelType, BuildConverterInfo);

                    return new[] { converter };
                }
            }

            return Array.Empty<ValueConverterInfo>();
        }

        private ValueConverterInfo BuildConverterInfo(Type stringEnum)
        {
            var converterType = typeof(StringEnumValueConverter<>).MakeGenericType(stringEnum);

            var converter = Activator.CreateInstance(converterType, (ConverterMappingHints?)null) as ValueConverter;

            return new ValueConverterInfo(stringEnum, typeof(string), _ => converter, null);
        }

        private static bool IsStringEnum(Type? underlyingModelType)
        {
            if (underlyingModelType is null) return false;

            if (!underlyingModelType.IsClass) return false;

            var baseClassIsGeneric = underlyingModelType.BaseType?.IsGenericType ?? false;

            if (!baseClassIsGeneric) return false;

            try
            {
                var constructedStringEnumType = typeof(StringEnum<>).MakeGenericType(underlyingModelType);

                return constructedStringEnumType.IsAssignableFrom(underlyingModelType);
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public static Type UnwrapNullableType(Type type) => Nullable.GetUnderlyingType(type) ?? type;

    }
}