using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface IStudentService
{
    /// <summary>
    /// Filter → Sort → Page → Map to BM.
    /// </summary>
    Task<PagedResponse<StudentBM>> GetListAsync(StudentListQuery query);

    Task<StudentBM?> GetByIdAsync(int id);

    Task<StudentBM> CreateAsync(string fullName, string email, DateTime dateOfBirth);

    Task<StudentBM?> UpdateAsync(int id, string fullName, string email, DateTime dateOfBirth);

    Task<bool> DeleteAsync(int id);
}
