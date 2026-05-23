using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/subjects")]
[Produces("application/json")]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _service;

    public SubjectsController(ISubjectService service) => _service = service;

    /// <summary>Get a paged list of subjects.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<SubjectResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResponse<SubjectResponse>>>> GetList(
        [FromQuery] ListQueryRequest q)
    {
        var (items, total) = await _service.GetPagedAsync(q.Search, q.Sort, q.Page, q.Size);

        return Ok(ApiResponse<PagedResponse<SubjectResponse>>.Ok(new PagedResponse<SubjectResponse>
        {
            Items = items.Select(MapToResponse),
            Pagination = new PaginationMeta
            {
                Page       = q.Page,
                PageSize   = q.Size,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling(total / (double)q.Size)
            }
        }));
    }

    /// <summary>Get a single subject by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<SubjectResponse>>> GetById(int id)
    {
        var bm = await _service.GetByIdAsync(id);
        if (bm is null)
            return NotFound(ApiResponse<SubjectResponse>.Fail($"Subject {id} not found."));

        return Ok(ApiResponse<SubjectResponse>.Ok(MapToResponse(bm)));
    }

    private static SubjectResponse MapToResponse(SubjectBM bm) => new()
    {
        SubjectId   = bm.SubjectId,
        SubjectCode = bm.SubjectCode,
        SubjectName = bm.SubjectName,
        Credit      = bm.Credit
    };
}
