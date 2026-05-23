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

    public async Task<SemesterBM> CreateAsync(string semesterName, DateTime startDate, DateTime endDate)
    {
        var entity = new PRN232.LMS.Repositories.Entities.Semester
        {
            SemesterName = semesterName,
            StartDate = startDate,
            EndDate = endDate
        };

        await _uow.Semesters.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return MapToBM(entity);
    }

    public async Task<SemesterBM?> UpdateAsync(int id, string semesterName, DateTime startDate, DateTime endDate)
    {
        var existing = await _uow.Semesters.GetByIdAsync(id);
        if (existing is null) return null;

        existing.SemesterName = semesterName;
        existing.StartDate = startDate;
        existing.EndDate = endDate;

        _uow.Semesters.Update(existing);
        await _uow.SaveChangesAsync();
        return MapToBM(existing);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _uow.Semesters.GetByIdAsync(id);
        if (existing is null) return false;

        _uow.Semesters.Remove(existing);
        await _uow.SaveChangesAsync();
        return true;
    }

    private static SemesterBM MapToBM(PRN232.LMS.Repositories.Entities.Semester s) => new()
    {
        SemesterId   = s.SemesterId,
        SemesterName = s.SemesterName,
        StartDate    = s.StartDate,
        EndDate      = s.EndDate
    };
}
