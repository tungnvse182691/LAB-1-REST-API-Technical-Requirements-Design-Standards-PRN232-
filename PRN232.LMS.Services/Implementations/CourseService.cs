using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Implementations;

public class CourseService : ICourseService
{
    private readonly IUnitOfWork _uow;

    public CourseService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<CourseBM>> GetAllAsync()
    {
        var courses = await _uow.Courses.Query()
            .Include(c => c.Semester)
            .ToListAsync();
        return courses.Select(c => MapToBM(c, includeSemester: true));
    }

    public async Task<CourseBM?> GetByIdAsync(int id)
    {
        var course = await _uow.Courses.GetWithSemesterAsync(id);
        return course is null ? null : MapToBM(course, includeSemester: true);
    }

    public async Task<(IEnumerable<CourseBM> Items, int TotalCount)> GetPagedAsync(
        string? search, string? sort, int page, int size, string? expand = null)
    {
        IQueryable<Course> query = _uow.Courses.Query().Include(c => c.Semester);
        var includeSemester = !string.IsNullOrWhiteSpace(expand) &&
                              expand.Contains("semester", StringComparison.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lower = search.ToLower();
            query = query.Where(c =>
                c.CourseName.ToLower().Contains(lower) ||
                c.Semester.SemesterName.ToLower().Contains(lower));
        }

        query = string.IsNullOrWhiteSpace(sort)
            ? query.OrderBy(c => c.CourseId)
            : sort.TrimStart('-').ToLower() switch
            {
                "coursename"   => sort.StartsWith('-') ? query.OrderByDescending(c => c.CourseName)   : query.OrderBy(c => c.CourseName),
                "semestername" => sort.StartsWith('-') ? query.OrderByDescending(c => c.Semester.SemesterName) : query.OrderBy(c => c.Semester.SemesterName),
                _              => query.OrderBy(c => c.CourseId)
            };

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        return (items.Select(c => MapToBM(c, includeSemester)), total);
    }

    public async Task<CourseBM> CreateAsync(string courseName, int semesterId)
    {
        var entity = new Course
        {
            CourseName = courseName,
            SemesterId = semesterId
        };

        await _uow.Courses.AddAsync(entity);
        await _uow.SaveChangesAsync();

        var created = await _uow.Courses.GetWithSemesterAsync(entity.CourseId);
        return created is null ? MapToBM(entity, includeSemester: false) : MapToBM(created, includeSemester: true);
    }

    public async Task<CourseBM?> UpdateAsync(int id, string courseName, int semesterId)
    {
        var existing = await _uow.Courses.GetByIdAsync(id);
        if (existing is null) return null;

        existing.CourseName = courseName;
        existing.SemesterId = semesterId;

        _uow.Courses.Update(existing);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Courses.GetWithSemesterAsync(id);
        return updated is null ? MapToBM(existing, includeSemester: false) : MapToBM(updated, includeSemester: true);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _uow.Courses.GetByIdAsync(id);
        if (existing is null) return false;

        _uow.Courses.Remove(existing);
        await _uow.SaveChangesAsync();
        return true;
    }

    private static CourseBM MapToBM(Course c, bool includeSemester) => new()
    {
        CourseId     = c.CourseId,
        CourseName   = c.CourseName,
        SemesterId   = c.SemesterId,
        SemesterName = c.Semester?.SemesterName ?? string.Empty,
        Semester = includeSemester && c.Semester is not null ? new SemesterBM
        {
            SemesterId   = c.Semester.SemesterId,
            SemesterName = c.Semester.SemesterName,
            StartDate    = c.Semester.StartDate,
            EndDate      = c.Semester.EndDate
        } : null
    };
}
