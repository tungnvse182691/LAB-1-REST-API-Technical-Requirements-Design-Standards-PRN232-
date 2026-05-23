using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public class UpdateEnrollmentRequest
{
    [Required]
    public DateTime EnrollDate { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = null!;
}
