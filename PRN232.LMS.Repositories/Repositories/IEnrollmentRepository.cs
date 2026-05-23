using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Repositories;

public interface IEnrollmentRepository : IRepository<Enrollment>
{
    /// <summary>Returns a page of enrollments. Pass includeRelated=true to eager-load Student and Course.</summary>
    Task<(IEnumerable<Enrollment> Items, int Total)> GetAllAsync(int skip, int take, bool includeRelated = false);

    /// <summary>Returns enrollments for a specific course. Pass includeRelated=true to eager-load Student and Course.</summary>
    Task<(IEnumerable<Enrollment> Items, int Total)> GetByCourseIdAsync(int courseId, int skip, int take, bool includeRelated = false);

    /// <summary>Always includes Student and Course navigation properties.</summary>
    new Task<Enrollment?> GetByIdAsync(int id);
}
