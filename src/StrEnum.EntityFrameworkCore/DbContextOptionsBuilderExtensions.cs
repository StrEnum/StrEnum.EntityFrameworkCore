using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace StrEnum.EntityFrameworkCore;

public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Allows Entity Framework to handle string enums.
    /// </summary>
    public static DbContextOptionsBuilder UseStringEnums(this DbContextOptionsBuilder builder)
    {
        return builder.ReplaceService<IValueConverterSelector, ChainedValueConverterSelectorDecorator>();
    }
}