using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

public class StudentRepository : GenericRepository<Student>, IStudentRepository
{
    public StudentRepository(LmsDbContext context) : base(context) { }

    public async Task<Student?> GetByEmailAsync(string email) =>
        await _dbSet.FirstOrDefaultAsync(s => s.Email == email);
}
