namespace PRN232.LMS.API.Models.Requests;

public class ListQueryRequest
{
    /// <summary>Free-text search term</summary>
    public string? Search { get; set; }

    /// <summary>Comma-separated sort fields, prefix with '-' for descending. e.g. "fullName,-dateOfBirth"</summary>
    public string? Sort { get; set; }

    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;

    /// <summary>Comma-separated fields to include in the response. e.g. "studentId,fullName,email"</summary>
    public string? Fields { get; set; }

    /// <summary>Comma-separated related entities to expand. e.g. "student,course"</summary>
    public string? Expand { get; set; }
}
