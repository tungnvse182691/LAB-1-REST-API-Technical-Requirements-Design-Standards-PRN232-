using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Implementations;

public class SubjectService : ISubjectService
{
    private readonly IUnitOfWork _uow;

    public SubjectService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<SubjectBM>> GetAllAsync()
    {
        var subjects = await _uow.Subjects.GetAllAsync();
        return subjects.Select(MapToBM);
    }

    public async Task<SubjectBM?> GetByIdAsync(int id)
    {
        var subject = await _uow.Subjects.GetByIdAsync(id);
        return subject is null ? null : MapToBM(subject);
    }

    public async Task<(IEnumerable<SubjectBM> Items, int TotalCount)> GetPagedAsync(
        string? search, string? sort, int page, int size)
    {
        var query = _uow.Subjects.Query();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lower = search.ToLower();
            query = query.Where(s =>
                s.SubjectCode.ToLower().Contains(lower) ||
                s.SubjectName.ToLower().Contains(lower));
        }

        query = string.IsNullOrWhiteSpace(sort)
            ? query.OrderBy(s => s.SubjectId)
            : sort.TrimStart('-').ToLower() switch
            {
                "subjectname" => sort.StartsWith('-')
                    ? query.OrderByDescending(s => s.SubjectName)
                    : query.OrderBy(s => s.SubjectName),
                "subjectcode" => sort.StartsWith('-')
                    ? query.OrderByDescending(s => s.SubjectCode)
                    : query.OrderBy(s => s.SubjectCode),
                "credit" => sort.StartsWith('-')
                    ? query.OrderByDescending(s => s.Credit)
                    : query.OrderBy(s => s.Credit),
                _ => query.OrderBy(s => s.SubjectId)
            };

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        return (items.Select(MapToBM), total);
    }

    private static SubjectBM MapToBM(PRN232.LMS.Repositories.Entities.Subject s) => new()
    {
        SubjectId   = s.SubjectId,
        SubjectCode = s.SubjectCode,
        SubjectName = s.SubjectName,
        Credit      = s.Credit
    };
}
