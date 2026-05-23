using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

public class CourseRepository : GenericRepository<Course>, ICourseRepository
{
    public CourseRepository(LmsDbContext context) : base(context) { }

    public async Task<Course?> GetWithSemesterAsync(int courseId) =>
        await _dbSet.Include(c => c.Semester)
                    .FirstOrDefaultAsync(c => c.CourseId == courseId);

    public async Task<IEnumerable<Course>> GetBySemesterAsync(int semesterId) =>
        await _dbSet.Include(c => c.Semester)
                    .Where(c => c.SemesterId == semesterId)
                    .ToListAsync();
}
