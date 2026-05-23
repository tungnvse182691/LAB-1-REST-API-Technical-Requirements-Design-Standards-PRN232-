using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface ISemesterService
{
    Task<IEnumerable<SemesterBM>> GetAllAsync();
    Task<SemesterBM?> GetByIdAsync(int id);
    Task<(IEnumerable<SemesterBM> Items, int TotalCount)> GetPagedAsync(
        string? search, string? sort, int page, int size);

    Task<SemesterBM> CreateAsync(string semesterName, DateTime startDate, DateTime endDate);

    Task<SemesterBM?> UpdateAsync(int id, string semesterName, DateTime startDate, DateTime endDate);

    Task<bool> DeleteAsync(int id);
}
