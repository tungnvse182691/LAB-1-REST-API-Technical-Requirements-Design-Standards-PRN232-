using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly LmsDbContext _context;

    public IStudentRepository    Students    { get; }
    public ICourseRepository     Courses     { get; }
    public ISemesterRepository   Semesters   { get; }
    public ISubjectRepository    Subjects    { get; }
    public IEnrollmentRepository Enrollments { get; }

    public UnitOfWork(LmsDbContext context)
    {
        _context    = context;
        Students    = new StudentRepository(context);
        Courses     = new CourseRepository(context);
        Semesters   = new SemesterRepository(context);
        Subjects    = new SubjectRepository(context);
        Enrollments = new EnrollmentRepository(context);
    }

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();

    public void Dispose() =>
        _context.Dispose();
}
