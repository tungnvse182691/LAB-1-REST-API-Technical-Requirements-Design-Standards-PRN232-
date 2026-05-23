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
        return courses.Select(MapToBM);
    }

    public async Task<CourseBM?> GetByIdAsync(int id)
    {
        var course = await _uow.Courses.GetWithSemesterAsync(id);
        return course is null ? null : MapToBM(course);
    }

    public async Task<(IEnumerable<CourseBM> Items, int TotalCount)> GetPagedAsync(
        string? search, string? sort, int page, int size)
    {
        IQueryable<Course> query = _uow.Courses.Query().Include(c => c.Semester);

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
        return (items.Select(MapToBM), total);
    }

    private static CourseBM MapToBM(Course c) => new()
    {
        CourseId     = c.CourseId,
        CourseName   = c.CourseName,
        SemesterId   = c.SemesterId,
        SemesterName = c.Semester?.SemesterName ?? string.Empty
    };
}
