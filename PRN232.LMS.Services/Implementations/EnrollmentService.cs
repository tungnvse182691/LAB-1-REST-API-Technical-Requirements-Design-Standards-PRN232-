using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Repositories;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Implementations;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _repo;

    public EnrollmentService(IEnrollmentRepository repo) => _repo = repo;

    // ── GetListAsync ─────────────────────────────────────────────────────────
    public async Task<PagedResponse<EnrollmentBM>> GetListAsync(EnrollmentListQuery query)
    {
        return await GetPagedAsync(query, null);
    }

    public async Task<PagedResponse<EnrollmentBM>> GetListByCourseIdAsync(int courseId, EnrollmentListQuery query)
    {
        return await GetPagedAsync(query, courseId);
    }

    // ── GetByIdAsync ─────────────────────────────────────────────────────────
    /// <summary>Always includes nested Student and Course objects.</summary>
    public async Task<EnrollmentBM?> GetByIdAsync(int id)
    {
        var enrollment = await _repo.GetByIdAsync(id);
        return enrollment is null ? null : MapToBM(enrollment, includeRelated: true);
    }

    // ── CreateAsync ──────────────────────────────────────────────────────────
    public async Task<EnrollmentBM> CreateAsync(
        int studentId, int courseId, DateTime enrollDate, string status)
    {
        var entity = new Enrollment
        {
            StudentId  = studentId,
            CourseId   = courseId,
            EnrollDate = enrollDate,
            Status     = status
        };
        var created = await _repo.CreateAsync(entity);
        return MapToBM(created, includeRelated: false);
    }

    // ── UpdateAsync ──────────────────────────────────────────────────────────
    public async Task<EnrollmentBM?> UpdateAsync(int id, DateTime enrollDate, string status)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing is null) return null;

        existing.EnrollDate = enrollDate;
        existing.Status     = status;

        var updated = await _repo.UpdateAsync(existing);
        return MapToBM(updated, includeRelated: true);
    }

    // ── DeleteAsync ──────────────────────────────────────────────────────────
    public async Task<bool> DeleteAsync(int id) =>
        await _repo.DeleteAsync(id);

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Supported sort fields: enrollmentId, enrollDate, status.
    /// Prefix '-' = descending.
    /// </summary>
    private static IEnumerable<Enrollment> ApplySort(IEnumerable<Enrollment> source, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return source.OrderBy(e => e.EnrollmentId);

        var fields = sort.Split(',', StringSplitOptions.RemoveEmptyEntries);
        IOrderedEnumerable<Enrollment>? ordered = null;

        foreach (var field in fields)
        {
            var desc = field.TrimStart().StartsWith('-');
            var name = field.TrimStart('-', ' ').ToLower();

            if (ordered is null)
            {
                ordered = (name, desc) switch
                {
                    ("enrollmentid", false) => source.OrderBy(e => e.EnrollmentId),
                    ("enrollmentid", true)  => source.OrderByDescending(e => e.EnrollmentId),
                    ("enrolldate",   false) => source.OrderBy(e => e.EnrollDate),
                    ("enrolldate",   true)  => source.OrderByDescending(e => e.EnrollDate),
                    ("status",       false) => source.OrderBy(e => e.Status),
                    ("status",       true)  => source.OrderByDescending(e => e.Status),
                    _                       => source.OrderBy(e => e.EnrollmentId)
                };
            }
            else
            {
                ordered = (name, desc) switch
                {
                    ("enrollmentid", false) => ordered.ThenBy(e => e.EnrollmentId),
                    ("enrollmentid", true)  => ordered.ThenByDescending(e => e.EnrollmentId),
                    ("enrolldate",   false) => ordered.ThenBy(e => e.EnrollDate),
                    ("enrolldate",   true)  => ordered.ThenByDescending(e => e.EnrollDate),
                    ("status",       false) => ordered.ThenBy(e => e.Status),
                    ("status",       true)  => ordered.ThenByDescending(e => e.Status),
                    _                       => ordered.ThenBy(e => e.EnrollmentId)
                };
            }
        }

        return ordered ?? source.OrderBy(e => e.EnrollmentId);
    }

    private static EnrollmentBM MapToBM(Enrollment e, bool includeRelated) => new()
    {
        EnrollmentId = e.EnrollmentId,
        StudentId    = e.StudentId,
        CourseId     = e.CourseId,
        EnrollDate   = e.EnrollDate,
        Status       = e.Status,
        Student = includeRelated && e.Student is not null ? new StudentBM
        {
            StudentId   = e.Student.StudentId,
            FullName    = e.Student.FullName,
            Email       = e.Student.Email,
            DateOfBirth = e.Student.DateOfBirth
        } : null,
        Course = includeRelated && e.Course is not null ? new CourseBM
        {
            CourseId     = e.Course.CourseId,
            CourseName   = e.Course.CourseName,
            SemesterId   = e.Course.SemesterId,
            SemesterName = e.Course.Semester?.SemesterName ?? string.Empty
        } : null
    };

    private async Task<PagedResponse<EnrollmentBM>> GetPagedAsync(EnrollmentListQuery query, int? courseId)
    {
        var page   = query.Page < 1 ? 1 : query.Page;
        var size   = query.Size < 1 ? 10 : query.Size;
        bool expand = !string.IsNullOrWhiteSpace(query.Expand) &&
                      (query.Expand.Contains("student", StringComparison.OrdinalIgnoreCase) ||
                       query.Expand.Contains("course",  StringComparison.OrdinalIgnoreCase));

        IEnumerable<Enrollment> source;
        int total;

        if (courseId.HasValue)
            (source, total) = await _repo.GetByCourseIdAsync(courseId.Value, 0, int.MaxValue, includeRelated: expand);
        else
            (source, total) = await _repo.GetAllAsync(0, int.MaxValue, includeRelated: expand);

        IEnumerable<Enrollment> filtered = source;
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var lower = query.Search.ToLower();
            filtered = filtered.Where(e => e.Status.ToLower().Contains(lower));
        }

        var filteredList = filtered.ToList();
        total = filteredList.Count;

        filteredList = ApplySort(filteredList, query.Sort).ToList();
        var items = filteredList.Skip((page - 1) * size).Take(size).ToList();

        return new PagedResponse<EnrollmentBM>
        {
            Items = items.Select(e => MapToBM(e, expand)),
            Pagination = new PaginationMeta
            {
                Page       = page,
                PageSize   = size,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling(total / (double)size)
            }
        };
    }
}
