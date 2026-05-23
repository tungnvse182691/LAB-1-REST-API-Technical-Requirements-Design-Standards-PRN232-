using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly LmsDbContext _context;

    public EnrollmentRepository(LmsDbContext context) => _context = context;

    // ── IRepository<Enrollment> ──────────────────────────────────────────────

    /// <summary>Always includes Student and Course (with Semester) for full context.</summary>
    public async Task<Enrollment?> GetByIdAsync(int id) =>
        await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course).ThenInclude(c => c.Semester)
            .FirstOrDefaultAsync(e => e.EnrollmentId == id);

    public async Task<Enrollment> CreateAsync(Enrollment enrollment)
    {
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();
        return enrollment;
    }

    public async Task<Enrollment> UpdateAsync(Enrollment enrollment)
    {
        _context.Enrollments.Update(enrollment);
        await _context.SaveChangesAsync();
        return enrollment;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var enrollment = await _context.Enrollments.FindAsync(id);
        if (enrollment is null) return false;

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id) =>
        await _context.Enrollments.AnyAsync(e => e.EnrollmentId == id);

    // ── IEnrollmentRepository ────────────────────────────────────────────────

    public async Task<(IEnumerable<Enrollment> Items, int Total)> GetAllAsync(
        int skip, int take, bool includeRelated = false)
    {
        IQueryable<Enrollment> query = _context.Enrollments.AsNoTracking();

        if (includeRelated)
            query = query
                .Include(e => e.Student)
                .Include(e => e.Course).ThenInclude(c => c.Semester);

        var total = await query.CountAsync();
        var items = await query.Skip(skip).Take(take).ToListAsync();
        return (items, total);
    }
}
