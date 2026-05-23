using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Helpers;
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

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<SubjectResponse>>> Create([FromBody] CreateSubjectRequest r)
    {
        var bm = await _service.CreateAsync(r.SubjectCode, r.SubjectName, r.Credit);
        return CreatedAtAction(nameof(GetById), new { id = bm.SubjectId },
            ApiResponse<SubjectResponse>.Ok(MapToResponse(bm), "Subject created."));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<SubjectResponse>>> Update(int id, [FromBody] UpdateSubjectRequest r)
    {
        var bm = await _service.UpdateAsync(id, r.SubjectCode, r.SubjectName, r.Credit);
        if (bm is null)
            return NotFound(ApiResponse<SubjectResponse>.Fail($"Subject {id} not found."));

        return Ok(ApiResponse<SubjectResponse>.Ok(MapToResponse(bm), "Subject updated."));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.Fail($"Subject {id} not found."));

        return Ok(ApiResponse<object>.Ok(null!, "Subject deleted."));
    }

    private static SubjectResponse MapToResponse(SubjectBM bm) => new()
    {
        SubjectId   = bm.SubjectId,
        SubjectCode = bm.SubjectCode,
        SubjectName = bm.SubjectName,
        Credit      = bm.Credit
    };
}
