using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Implementations;

public class SemesterService : ISemesterService
{
    private readonly IUnitOfWork _uow;

    public SemesterService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<SemesterBM>> GetAllAsync()
    {
        var semesters = await _uow.Semesters.GetAllAsync();
        return semesters.Select(MapToBM);
    }

    public async Task<SemesterBM?> GetByIdAsync(int id)
    {
        var semester = await _uow.Semesters.GetByIdAsync(id);
        return semester is null ? null : MapToBM(semester);
    }

    public async Task<(IEnumerable<SemesterBM> Items, int TotalCount)> GetPagedAsync(
        string? search, string? sort, int page, int size)
    {
        var query = _uow.Semesters.Query();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lower = search.ToLower();
            query = query.Where(s => s.SemesterName.ToLower().Contains(lower));
        }

        query = string.IsNullOrWhiteSpace(sort)
            ? query.OrderBy(s => s.SemesterId)
            : sort.TrimStart('-').ToLower() switch
            {
                "semestername" => sort.StartsWith('-')
                    ? query.OrderByDescending(s => s.SemesterName)
                    : query.OrderBy(s => s.SemesterName),
                "startdate" => sort.StartsWith('-')
                    ? query.OrderByDescending(s => s.StartDate)
                    : query.OrderBy(s => s.StartDate),
                _ => query.OrderBy(s => s.SemesterId)
            };

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        return (items.Select(MapToBM), total);
    }

    private static SemesterBM MapToBM(PRN232.LMS.Repositories.Entities.Semester s) => new()
    {
        SemesterId   = s.SemesterId,
        SemesterName = s.SemesterName,
        StartDate    = s.StartDate,
        EndDate      = s.EndDate
    };
}
