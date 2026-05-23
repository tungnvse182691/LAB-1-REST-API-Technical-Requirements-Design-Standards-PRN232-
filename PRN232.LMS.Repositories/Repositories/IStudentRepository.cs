using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Repositories;

public interface IStudentRepository : IRepository<Student>
{
    /// <summary>Returns a page of students (no filtering, no sorting — that's the service's job).</summary>
    Task<(IEnumerable<Student> Items, int Total)> GetAllAsync(int skip, int take);

    /// <summary>Simple keyword search on FullName or Email — pure data access, no business rules.</summary>
    Task<IEnumerable<Student>> SearchByNameOrEmailAsync(string keyword);
}
