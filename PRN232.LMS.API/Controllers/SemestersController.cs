using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/semesters")]
[Produces("application/json")]
public class SemestersController : ControllerBase
{
    private readonly ISemesterService _service;

    public SemestersController(ISemesterService service) => _service = service;

    /// <summary>Get a paged list of semesters.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<SemesterResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResponse<SemesterResponse>>>> GetList(
        [FromQuery] ListQueryRequest q)
    {
        var (items, total) = await _service.GetPagedAsync(q.Search, q.Sort, q.Page, q.Size);

        return Ok(ApiResponse<PagedResponse<SemesterResponse>>.Ok(new PagedResponse<SemesterResponse>
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

    /// <summary>Get a single semester by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<SemesterResponse>>> GetById(int id)
    {
        var bm = await _service.GetByIdAsync(id);
        if (bm is null)
            return NotFound(ApiResponse<SemesterResponse>.Fail($"Semester {id} not found."));

        return Ok(ApiResponse<SemesterResponse>.Ok(MapToResponse(bm)));
    }

    private static SemesterResponse MapToResponse(SemesterBM bm) => new()
    {
        SemesterId   = bm.SemesterId,
        SemesterName = bm.SemesterName,
        StartDate    = bm.StartDate,
        EndDate      = bm.EndDate
    };
}
