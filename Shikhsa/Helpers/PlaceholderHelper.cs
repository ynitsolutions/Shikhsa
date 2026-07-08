namespace Shikhsa.Helpers
{
   public static class PlaceholderHelper
{
    public static Dictionary<string, string> CreateDictionary(params object[] objects)
    {
        Dictionary<string, string> dict = new(StringComparer.OrdinalIgnoreCase);

        foreach (var obj in objects)
        {
            if (obj == null)
                continue;

            string prefix = GetPrefix(obj);

            AddObject(dict, obj, prefix);
        }

        return dict;
    }

    public static string ReplacePlaceholders(
        string text,
        Dictionary<string, string> dict)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        foreach (var item in dict)
        {
            text = text.Replace(item.Key, item.Value ?? "");
        }

        return text;
    }

    private static void AddObject(
        Dictionary<string, string> dict,
        object obj,
        string prefix)
    {
        var props = obj.GetType().GetProperties();

        foreach (var p in props)
        {
            if (!IsSimpleType(p.PropertyType))
                continue;

            object? value = p.GetValue(obj);

            dict[$"{{{{{prefix}.{p.Name}}}}}"] =
                value switch
                {
                    null => "",
                    DateTime dt => dt.ToString("dd-MMM-yyyy"),
                    _ => value.ToString() ?? ""
                };
        }

        AddFullNames(dict, obj, prefix);
    }

    private static void AddFullNames(
        Dictionary<string, string> dict,
        object obj,
        string prefix)
    {
        AddName(dict, obj, prefix,
            "FullName",
            "FirstName",
            "MiddleName",
            "LastName");

        AddName(dict, obj, prefix,
            "FatherFullName",
            "FatherFirstName",
            "FatherMiddleName",
            "FatherLastName");

        AddName(dict, obj, prefix,
            "MotherFullName",
            "MotherFirstName",
            "MotherMiddleName",
            "MotherLastName");

        AddName(dict, obj, prefix,
            "GuardianFullName",
            "GuardianFirstName",
            "GuardianMiddleName",
            "GuardianLastName");
    }

    private static void AddName(
        Dictionary<string, string> dict,
        object obj,
        string prefix,
        string target,
        string first,
        string middle,
        string last)
    {
        string full = string.Join(" ",
            new[]
            {
                GetValue(obj, first),
                GetValue(obj, middle),
                GetValue(obj, last)
            }.Where(x => !string.IsNullOrWhiteSpace(x)));

        if (!string.IsNullOrWhiteSpace(full))
            dict[$"{{{{{prefix}.{target}}}}}"] = full;
    }

    private static string GetValue(object obj, string property)
    {
        return obj.GetType()
            .GetProperty(property)?
            .GetValue(obj)?
            .ToString() ?? "";
    }

    private static bool IsSimpleType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(Guid);
    }

    private static string GetPrefix(object obj)
    {
        string name = obj.GetType().Name;

        if (name.Contains("School"))
            return "School";
            if (name.Contains("Student"))
                return "Student";

            if (name.Contains("Parent"))
            return "Parent";

        if (name.Contains("Staff"))
            return "Staff";

        if (name.Contains("Leave"))
            return "Leave";

        if (name.Contains("Attendance"))
            return "Attendance";

        if (name.Contains("PreviousSchool"))
            return "PreviousSchool";

        return name;
    }
}
}
