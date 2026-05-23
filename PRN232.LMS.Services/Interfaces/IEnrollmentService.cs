using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface IEnrollmentService
{
    /// <summary>
    /// Filter by Status → Sort → Page → Map to BM (with nested Student/Course if expand requested).
    /// </summary>
    Task<PagedResponse<EnrollmentBM>> GetListAsync(EnrollmentListQuery query);

    /// <summary>Always returns nested Student and Course objects.</summary>
    Task<EnrollmentBM?> GetByIdAsync(int id);

    Task<EnrollmentBM> CreateAsync(int studentId, int courseId, DateTime enrollDate, string status);

    Task<EnrollmentBM?> UpdateAsync(int id, DateTime enrollDate, string status);

    Task<bool> DeleteAsync(int id);
}
