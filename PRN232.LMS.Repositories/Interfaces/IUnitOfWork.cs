namespace PRN232.LMS.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IStudentRepository Students { get; }
    ICourseRepository Courses { get; }
    ISemesterRepository Semesters { get; }
    ISubjectRepository Subjects { get; }
    IEnrollmentRepository Enrollments { get; }
    Task<int> SaveChangesAsync();
}
