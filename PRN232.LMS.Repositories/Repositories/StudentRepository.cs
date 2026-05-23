using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly LmsDbContext _context;

    public StudentRepository(LmsDbContext context) => _context = context;

    // ── IRepository<Student> ─────────────────────────────────────────────────

    public async Task<Student?> GetByIdAsync(int id) =>
        await _context.Students.FindAsync(id);

    public async Task<Student> CreateAsync(Student student)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        return student;
    }

    public async Task<Student> UpdateAsync(Student student)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync();
        return student;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student is null) return false;

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id) =>
        await _context.Students.AnyAsync(s => s.StudentId == id);

    // ── IStudentRepository ───────────────────────────────────────────────────

    public async Task<(IEnumerable<Student> Items, int Total)> GetAllAsync(int skip, int take)
    {
        var query = _context.Students.AsNoTracking();
        var total = await query.CountAsync();
        var items = await query.Skip(skip).Take(take).ToListAsync();
        return (items, total);
    }

    public async Task<IEnumerable<Student>> SearchByNameOrEmailAsync(string keyword)
    {
        var lower = keyword.ToLower();
        return await _context.Students
            .AsNoTracking()
            .Where(s => s.FullName.ToLower().Contains(lower) ||
                        s.Email.ToLower().Contains(lower))
            .ToListAsync();
    }
}
