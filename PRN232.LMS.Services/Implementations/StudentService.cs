using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Repositories;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _repo;

    public StudentService(IStudentRepository repo) => _repo = repo;

    // ── GetListAsync ─────────────────────────────────────────────────────────
    public async Task<PagedResponse<StudentBM>> GetListAsync(StudentListQuery query)
    {
        // 1. Fetch all (or search results) from repository
        IEnumerable<Student> source;
        int total;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            // Repository does the DB-level keyword search
            source = await _repo.SearchByNameOrEmailAsync(query.Search);
            total  = source.Count();
        }
        else
        {
            // Repository returns total count alongside items
            (source, total) = await _repo.GetAllAsync(0, int.MaxValue);
        }

        // 2. Sort (in-memory after fetch — keeps repo free of business rules)
        source = ApplySort(source, query.Sort);

        // 3. Page
        var page = query.Page < 1 ? 1 : query.Page;
        var size = query.Size < 1 ? 10 : query.Size;
        var items = source.Skip((page - 1) * size).Take(size).ToList();

        // 4. Map → BM
        return new PagedResponse<StudentBM>
        {
            Items = items.Select(MapToBM),
            Pagination = new PaginationMeta
            {
                Page       = page,
                PageSize   = size,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling(total / (double)size)
            }
        };
    }

    // ── GetByIdAsync ─────────────────────────────────────────────────────────
    public async Task<StudentBM?> GetByIdAsync(int id)
    {
        var student = await _repo.GetByIdAsync(id);
        return student is null ? null : MapToBM(student);
    }

    // ── CreateAsync ──────────────────────────────────────────────────────────
    public async Task<StudentBM> CreateAsync(string fullName, string email, DateTime dateOfBirth)
    {
        var entity = new Student
        {
            FullName    = fullName,
            Email       = email,
            DateOfBirth = dateOfBirth
        };
        var created = await _repo.CreateAsync(entity);
        return MapToBM(created);
    }

    // ── UpdateAsync ──────────────────────────────────────────────────────────
    public async Task<StudentBM?> UpdateAsync(int id, string fullName, string email, DateTime dateOfBirth)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing is null) return null;

        existing.FullName    = fullName;
        existing.Email       = email;
        existing.DateOfBirth = dateOfBirth;

        var updated = await _repo.UpdateAsync(existing);
        return MapToBM(updated);
    }

    // ── DeleteAsync ──────────────────────────────────────────────────────────
    public async Task<bool> DeleteAsync(int id) =>
        await _repo.DeleteAsync(id);

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Parses comma-separated sort string and applies multi-field ordering.
    /// Supported fields: studentId, fullName, email, dateOfBirth
    /// Prefix '-' = descending.
    /// </summary>
    private static IEnumerable<Student> ApplySort(IEnumerable<Student> source, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return source.OrderBy(s => s.StudentId);

        var fields = sort.Split(',', StringSplitOptions.RemoveEmptyEntries);
        IOrderedEnumerable<Student>? ordered = null;

        foreach (var field in fields)
        {
            var desc = field.TrimStart() .StartsWith('-');
            var name = field.TrimStart('-', ' ').ToLower();

            if (ordered is null)
            {
                ordered = (name, desc) switch
                {
                    ("studentid",   false) => source.OrderBy(s => s.StudentId),
                    ("studentid",   true)  => source.OrderByDescending(s => s.StudentId),
                    ("fullname",    false) => source.OrderBy(s => s.FullName),
                    ("fullname",    true)  => source.OrderByDescending(s => s.FullName),
                    ("email",       false) => source.OrderBy(s => s.Email),
                    ("email",       true)  => source.OrderByDescending(s => s.Email),
                    ("dateofbirth", false) => source.OrderBy(s => s.DateOfBirth),
                    ("dateofbirth", true)  => source.OrderByDescending(s => s.DateOfBirth),
                    _                      => source.OrderBy(s => s.StudentId)
                };
            }
            else
            {
                ordered = (name, desc) switch
                {
                    ("studentid",   false) => ordered.ThenBy(s => s.StudentId),
                    ("studentid",   true)  => ordered.ThenByDescending(s => s.StudentId),
                    ("fullname",    false) => ordered.ThenBy(s => s.FullName),
                    ("fullname",    true)  => ordered.ThenByDescending(s => s.FullName),
                    ("email",       false) => ordered.ThenBy(s => s.Email),
                    ("email",       true)  => ordered.ThenByDescending(s => s.Email),
                    ("dateofbirth", false) => ordered.ThenBy(s => s.DateOfBirth),
                    ("dateofbirth", true)  => ordered.ThenByDescending(s => s.DateOfBirth),
                    _                      => ordered.ThenBy(s => s.StudentId)
                };
            }
        }

        return ordered ?? source.OrderBy(s => s.StudentId);
    }

    private static StudentBM MapToBM(Student s) => new()
    {
        StudentId   = s.StudentId,
        FullName    = s.FullName,
        Email       = s.Email,
        DateOfBirth = s.DateOfBirth
    };
}
