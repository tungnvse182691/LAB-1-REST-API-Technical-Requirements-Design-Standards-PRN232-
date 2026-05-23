using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Helpers;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/students")]
[Produces("application/json")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _service;

    public StudentsController(IStudentService service) => _service = service;

    /// <summary>
    /// Get a paged, searchable, sortable list of students.
    /// Use ?fields=studentId,fullName to return only specific fields.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<StudentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> GetList(
        [FromQuery] ListQueryRequest q)
    {
        var result = await _service.GetListAsync(new StudentListQuery
        {
            Search = q.Search,
            Sort   = q.Sort,
            Page   = q.Page,
            Size   = q.Size
        });

        var responses = result.Items.Select(MapToResponse).ToList();

        // ── Field selection ───────────────────────────────────────────────────
        // If ?fields= is provided, return only those fields per item.
        // Otherwise return full typed response as normal.
        object items = string.IsNullOrWhiteSpace(q.Fields)
            ? (object)responses
            : FieldSelector.ApplyToList(responses, q.Fields);

        var paged = new
        {
            items,
            pagination = new PaginationMeta
            {
                Page       = result.Pagination.Page,
                PageSize   = result.Pagination.PageSize,
                TotalItems = result.Pagination.TotalItems,
                TotalPages = result.Pagination.TotalPages
            }
        };

        return Ok(ApiResponse<object>.Ok(paged));
    }

    /// <summary>Get a single student by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> GetById(int id)
    {
        var bm = await _service.GetByIdAsync(id);
        if (bm is null)
            return NotFound(ApiResponse<StudentResponse>.Fail($"Student {id} not found."));

        return Ok(ApiResponse<StudentResponse>.Ok(MapToResponse(bm)));
    }

    /// <summary>Create a new student.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> Create(
        [FromBody] CreateStudentRequest r)
    {
        var bm = await _service.CreateAsync(r.FullName, r.Email, r.DateOfBirth);

        return CreatedAtAction(nameof(GetById), new { id = bm.StudentId },
            ApiResponse<StudentResponse>.Ok(MapToResponse(bm), "Student created."));
    }

    /// <summary>Update an existing student.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> Update(
        int id, [FromBody] UpdateStudentRequest r)
    {
        var bm = await _service.UpdateAsync(id, r.FullName, r.Email, r.DateOfBirth);
        if (bm is null)
            return NotFound(ApiResponse<StudentResponse>.Fail($"Student {id} not found."));

        return Ok(ApiResponse<StudentResponse>.Ok(MapToResponse(bm), "Student updated."));
    }

    /// <summary>Delete a student by ID.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.Fail($"Student {id} not found."));

        return Ok(ApiResponse<object>.Ok(null!, "Student deleted."));
    }

    // ── Mapper ────────────────────────────────────────────────────────────────

    private static StudentResponse MapToResponse(StudentBM bm) => new()
    {
        StudentId   = bm.StudentId,
        FullName    = bm.FullName,
        Email       = bm.Email,
        DateOfBirth = bm.DateOfBirth
    };
}
