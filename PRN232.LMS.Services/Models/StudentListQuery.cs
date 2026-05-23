namespace PRN232.LMS.Services.Models;

/// <summary>
/// Parameters passed from the controller into StudentService.GetListAsync.
/// Mirrors ListQueryRequest but lives in the Services layer to avoid circular dependency.
/// </summary>
public class StudentListQuery
{
    public string? Search { get; set; }
    /// <summary>Comma-separated sort fields. Prefix '-' = descending. e.g. "fullName,-dateOfBirth"</summary>
    public string? Sort { get; set; }
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
}
