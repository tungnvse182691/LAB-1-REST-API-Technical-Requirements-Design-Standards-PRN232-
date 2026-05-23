using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface ICourseService
{
    Task<IEnumerable<CourseBM>> GetAllAsync();
    Task<CourseBM?> GetByIdAsync(int id);
    Task<(IEnumerable<CourseBM> Items, int TotalCount)> GetPagedAsync(
        string? search, string? sort, int page, int size);
}
