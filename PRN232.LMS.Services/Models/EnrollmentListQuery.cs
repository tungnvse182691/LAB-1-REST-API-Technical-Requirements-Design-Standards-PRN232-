namespace PRN232.LMS.Services.Models;

/// <summary>
/// Parameters passed from the controller into EnrollmentService.GetListAsync.
/// </summary>
public class EnrollmentListQuery
{
    public string? Search { get; set; }
    /// <summary>Comma-separated sort fields. Prefix '-' = descending. e.g. "enrollDate,-status"</summary>
    public string? Sort { get; set; }
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    /// <summary>Comma-separated expand hints. e.g. "student,course"</summary>
    public string? Expand { get; set; }
}
