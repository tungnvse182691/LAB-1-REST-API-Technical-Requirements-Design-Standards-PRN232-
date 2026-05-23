using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface ISubjectService
{
    Task<IEnumerable<SubjectBM>> GetAllAsync();
    Task<SubjectBM?> GetByIdAsync(int id);
    Task<(IEnumerable<SubjectBM> Items, int TotalCount)> GetPagedAsync(
        string? search, string? sort, int page, int size);

    Task<SubjectBM> CreateAsync(string subjectCode, string subjectName, int credit);

    Task<SubjectBM?> UpdateAsync(int id, string subjectCode, string subjectName, int credit);

    Task<bool> DeleteAsync(int id);
}
