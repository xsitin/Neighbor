using System.Collections.Generic;
using System.Linq;

namespace BoardCommon.Enums
{
    public static class Categories
    {
        public static string Human = "Сожитель";
        public static string Place = "Койкоместо";
        public static string Flat = "Квартира";
        public static string Tenant = "Арендатор";

        public static readonly List<string> Enumerated =
            typeof(Categories).GetFields()
                .Where(x => x.FieldType == typeof(string))
                .Select(x => (string) x.GetValue(typeof(Categories))).ToList();
    }
}