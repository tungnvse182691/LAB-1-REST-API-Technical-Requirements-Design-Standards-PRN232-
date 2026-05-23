using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public class UpdateCourseRequest
{
    [Required]
    [MaxLength(100)]
    public string CourseName { get; set; } = null!;

    [Required]
    public int SemesterId { get; set; }
}