using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

public class EnrollmentRepository : GenericRepository<Enrollment>, IEnrollmentRepository
{
    public EnrollmentRepository(LmsDbContext context) : base(context) { }

    public async Task<Enrollment?> GetWithDetailsAsync(int enrollmentId) =>
        await _dbSet.Include(e => e.Student)
                    .Include(e => e.Course).ThenInclude(c => c.Semester)
                    .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);

    public async Task<IEnumerable<Enrollment>> GetByStudentAsync(int studentId) =>
        await _dbSet.Include(e => e.Course).ThenInclude(c => c.Semester)
                    .Where(e => e.StudentId == studentId)
                    .ToListAsync();

    public async Task<IEnumerable<Enrollment>> GetByCourseAsync(int courseId) =>
        await _dbSet.Include(e => e.Student)
                    .Where(e => e.CourseId == courseId)
                    .ToListAsync();

    public async Task<bool> ExistsAsync(int studentId, int courseId) =>
        await _dbSet.AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);
}
