using Microsoft.AspNetCore.Mvc;

namespace PRN232.LMS.API.Models.Requests;

public class ListQueryRequest
{
    /// <summary>Free-text search term</summary>
    [FromQuery(Name = "search")]
    public string? Search { get; set; }

    /// <summary>Comma-separated sort fields, prefix with '-' for descending. e.g. "fullName,-dateOfBirth"</summary>
    [FromQuery(Name = "sort")]
    public string? Sort { get; set; }

    [FromQuery(Name = "page")]
    public int Page { get; set; } = 1;

    [FromQuery(Name = "size")]
    public int Size { get; set; } = 10;

    /// <summary>Comma-separated fields to include in the response. e.g. "studentId,fullName,email"</summary>
    [FromQuery(Name = "fields")]
    public string? Fields { get; set; }

    /// <summary>Comma-separated related entities to expand. e.g. "student,course"</summary>
    [FromQuery(Name = "expand")]
    public string? Expand { get; set; }

    /// <summary>Backward-compatible alias for expand.</summary>
    [FromQuery(Name = "expland")]
    public string? Expland { get; set; }
}
