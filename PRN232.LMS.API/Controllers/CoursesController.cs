using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/courses")]
[Produces("application/json")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _service;

    public CoursesController(ICourseService service) => _service = service;

    /// <summary>Get a paged list of courses.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<CourseResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResponse<CourseResponse>>>> GetList(
        [FromQuery] ListQueryRequest q)
    {
        var (items, total) = await _service.GetPagedAsync(q.Search, q.Sort, q.Page, q.Size);

        return Ok(ApiResponse<PagedResponse<CourseResponse>>.Ok(new PagedResponse<CourseResponse>
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

    /// <summary>Get a single course by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CourseResponse>>> GetById(int id)
    {
        var bm = await _service.GetByIdAsync(id);
        if (bm is null)
            return NotFound(ApiResponse<CourseResponse>.Fail($"Course {id} not found."));

        return Ok(ApiResponse<CourseResponse>.Ok(MapToResponse(bm)));
    }

    private static CourseResponse MapToResponse(CourseBM bm) => new()
    {
        CourseId     = bm.CourseId,
        CourseName   = bm.CourseName,
        SemesterId   = bm.SemesterId,
        SemesterName = bm.SemesterName
    };
}
