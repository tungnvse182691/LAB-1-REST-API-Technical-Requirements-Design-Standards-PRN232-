using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface ICourseRepository : IGenericRepository<Course>
{
    Task<Course?> GetWithSemesterAsync(int courseId);
    Task<IEnumerable<Course>> GetBySemesterAsync(int semesterId);
}
