using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Helpers;
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
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> GetList(
        [FromQuery] ListQueryRequest q)
    {
        var (items, total) = await _service.GetPagedAsync(q.Search, q.Sort, q.Page, q.Size);
        var responses = items.Select(MapToResponse).ToList();

        object itemData = string.IsNullOrWhiteSpace(q.Fields)
            ? (object)responses
            : FieldSelector.ApplyToList(responses, q.Fields);

        var paged = new
        {
            items = itemData,
            pagination = new PaginationMeta
            {
                Page = q.Page,
                PageSize = q.Size,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling(total / (double)q.Size)
            }
        };

        return Ok(ApiResponse<object>.Ok(paged));
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

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<SemesterResponse>>> Create([FromBody] CreateSemesterRequest r)
    {
        var bm = await _service.CreateAsync(r.SemesterName, r.StartDate, r.EndDate);
        return CreatedAtAction(nameof(GetById), new { id = bm.SemesterId },
            ApiResponse<SemesterResponse>.Ok(MapToResponse(bm), "Semester created."));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<SemesterResponse>>> Update(int id, [FromBody] UpdateSemesterRequest r)
    {
        var bm = await _service.UpdateAsync(id, r.SemesterName, r.StartDate, r.EndDate);
        if (bm is null)
            return NotFound(ApiResponse<SemesterResponse>.Fail($"Semester {id} not found."));

        return Ok(ApiResponse<SemesterResponse>.Ok(MapToResponse(bm), "Semester updated."));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.Fail($"Semester {id} not found."));

        return Ok(ApiResponse<object>.Ok(null!, "Semester deleted."));
    }

    private static SemesterResponse MapToResponse(SemesterBM bm) => new()
    {
        SemesterId   = bm.SemesterId,
        SemesterName = bm.SemesterName,
        StartDate    = bm.StartDate,
        EndDate      = bm.EndDate
    };
}
