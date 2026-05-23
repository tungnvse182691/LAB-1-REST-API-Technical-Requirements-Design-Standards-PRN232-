using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface ISubjectService
{
    Task<IEnumerable<SubjectBM>> GetAllAsync();
    Task<SubjectBM?> GetByIdAsync(int id);
    Task<(IEnumerable<SubjectBM> Items, int TotalCount)> GetPagedAsync(
        string? search, string? sort, int page, int size);
}
