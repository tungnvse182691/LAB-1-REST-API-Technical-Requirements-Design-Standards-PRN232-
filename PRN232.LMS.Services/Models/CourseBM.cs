namespace PRN232.LMS.Services.Models;

public class CourseBM
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = null!;
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = null!;
    public SemesterBM? Semester { get; set; }
}
