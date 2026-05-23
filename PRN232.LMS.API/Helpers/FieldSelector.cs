using System.Text.Json;

namespace PRN232.LMS.API.Helpers;

/// <summary>
/// Filters an object's properties to only the requested fields.
/// Used to implement ?fields=field1,field2 query parameter.
/// </summary>
public static class FieldSelector
{
    private static readonly JsonSerializerOptions _opts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// If <paramref name="fields"/> is null/empty, returns the original object unchanged.
    /// Otherwise returns a Dictionary containing only the requested fields (case-insensitive).
    /// </summary>
    public static object Apply<T>(T obj, string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
            return obj!;

        var requested = fields
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(f => f.ToLower())
            .ToHashSet();

        // Serialize → parse as JsonElement → pick only requested keys
        var json    = JsonSerializer.Serialize(obj, _opts);
        var element = JsonSerializer.Deserialize<JsonElement>(json);

        var result = new Dictionary<string, object?>();
        foreach (var prop in element.EnumerateObject())
        {
            if (requested.Contains(prop.Name.ToLower()))
                result[prop.Name] = prop.Value.Deserialize<object>(_opts);
        }

        return result;
    }

    /// <summary>
    /// Applies field selection to every item in a collection.
    /// </summary>
    public static IEnumerable<object> ApplyToList<T>(IEnumerable<T> items, string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
            return items.Cast<object>();

        return items.Select(item => Apply(item, fields));
    }
}
