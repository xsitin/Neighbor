using System.Collections.Immutable;
using System.Linq;

namespace Common.Enums;

public static class Categories
{
    public const string Human = "Сожитель";
    public const string Place = "Койкоместо";
    public const string Flat = "Квартира";
    public const string Tenant = "Арендатор";
        
    public static readonly ImmutableList<string> Enumerated =
        typeof(Categories).GetFields()
            .Where(x => x.FieldType == typeof(string))
            .Select(x => (string) x.GetValue(typeof(Categories))).ToImmutableList();
}