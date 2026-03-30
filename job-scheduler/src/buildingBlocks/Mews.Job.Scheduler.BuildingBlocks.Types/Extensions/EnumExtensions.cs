using System.Linq.Expressions;

namespace Mews.Job.Scheduler;

public static class EnumExtensions
{
    private static readonly Type FlagsAttributeType = typeof(FlagsAttribute);

    public static bool IsFlagEnum(this Type type)
    {
        return type.IsEnum && type.CustomAttributes.Any(a => a.AttributeType == FlagsAttributeType);
    }

    public static long ToInt64<TEnum>(this TEnum value)
        where TEnum : struct, Enum
    {
        return EnumConvert<TEnum, long>.Cast(value);
    }

    public static TEnum ToEnum<TEnum>(this long value)
        where TEnum : struct, Enum
    {
        return EnumConvert<long, TEnum>.Cast(value);
    }

    /// <summary>
    /// Helper class to work around generic enum cast limitations which normally require boxing.
    /// Allows direct casting between an enum type and a numeric integer type (regardless of the enum's underlying type).
    /// Caches the Cast implementation for each combination of TSource and TTarget.
    /// Uses .NET runtime static class caching mechanisms so there is no explicit cache in our code.
    /// This is private on purpose as it allows unsafe casts as well which would fail at runtime. Use with caution.
    /// </summary>
    private static class EnumConvert<TSource, TTarget>
    {
        public static readonly Func<TSource, TTarget> Cast = CreateConverter();

        private static Func<TSource, TTarget> CreateConverter()
        {
            var p = Expression.Parameter(typeof(TSource));
            var c = Expression.Convert(p, typeof(TTarget));
            return Expression.Lambda<Func<TSource, TTarget>>(c, p).Compile();
        }
    }
}
