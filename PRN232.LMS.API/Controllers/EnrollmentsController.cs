using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Helpers;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/enrollments")]
[Produces("application/json")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _service;

    public EnrollmentsController(IEnrollmentService service) => _service = service;

    /// <summary>Get a paged list of enrollments. Use expand=student,course to include nested objects. Use fields=field1,field2 to select specific fields.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<EnrollmentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> GetList(
        [FromQuery] ListQueryRequest q)
    {
        var result = await _service.GetListAsync(new EnrollmentListQuery
        {
            Search = q.Search,
            Sort   = q.Sort,
            Page   = q.Page,
            Size   = q.Size,
            Expand = GetEffectiveExpand(q)
        });

        var responses = result.Items.Select(MapToResponse).ToList();

        // ── Field selection ───────────────────────────────────────────────────
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

    /// <summary>Get a single enrollment by ID (always includes Student and Course).</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<EnrollmentResponse>>> GetById(int id)
    {
        var bm = await _service.GetByIdAsync(id);
        if (bm is null)
            return NotFound(ApiResponse<EnrollmentResponse>.Fail($"Enrollment {id} not found."));

        return Ok(ApiResponse<EnrollmentResponse>.Ok(MapToResponse(bm)));
    }

    /// <summary>Create a new enrollment.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<EnrollmentResponse>>> Create(
        [FromBody] CreateEnrollmentRequest r)
    {
        var bm = await _service.CreateAsync(r.StudentId, r.CourseId, r.EnrollDate, r.Status);

        return CreatedAtAction(nameof(GetById), new { id = bm.EnrollmentId },
            ApiResponse<EnrollmentResponse>.Ok(MapToResponse(bm), "Enrollment created."));
    }

    /// <summary>Update an existing enrollment's date and status.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<EnrollmentResponse>>> Update(
        int id, [FromBody] UpdateEnrollmentRequest r)
    {
        var bm = await _service.UpdateAsync(id, r.EnrollDate, r.Status);
        if (bm is null)
            return NotFound(ApiResponse<EnrollmentResponse>.Fail($"Enrollment {id} not found."));

        return Ok(ApiResponse<EnrollmentResponse>.Ok(MapToResponse(bm), "Enrollment updated."));
    }

    /// <summary>Delete an enrollment by ID.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.Fail($"Enrollment {id} not found."));

        return Ok(ApiResponse<object>.Ok(null!, "Enrollment deleted."));
    }

    // ── Mappers ───────────────────────────────────────────────────────────────

    private static PagedResponse<EnrollmentResponse> MapPagedResponse(PagedResponse<EnrollmentBM> src) => new()
    {
        Items = src.Items.Select(MapToResponse),
        Pagination = new PaginationMeta
        {
            Page       = src.Pagination.Page,
            PageSize   = src.Pagination.PageSize,
            TotalItems = src.Pagination.TotalItems,
            TotalPages = src.Pagination.TotalPages
        }
    };

    private static EnrollmentResponse MapToResponse(EnrollmentBM bm) => new()
    {
        EnrollmentId = bm.EnrollmentId,
        StudentId    = bm.StudentId,
        CourseId     = bm.CourseId,
        EnrollDate   = bm.EnrollDate,
        Status       = bm.Status,
        Student = bm.Student is null ? null : new StudentResponse
        {
            StudentId   = bm.Student.StudentId,
            FullName    = bm.Student.FullName,
            Email       = bm.Student.Email,
            DateOfBirth = bm.Student.DateOfBirth
        },
        Course = bm.Course is null ? null : new CourseResponse
        {
            CourseId     = bm.Course.CourseId,
            CourseName   = bm.Course.CourseName,
            SemesterId   = bm.Course.SemesterId,
            SemesterName = bm.Course.SemesterName
        }
    };

    private static string? GetEffectiveExpand(ListQueryRequest q) =>
        !string.IsNullOrWhiteSpace(q.Expand) ? q.Expand : q.Expland;
}
