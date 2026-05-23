using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Helpers;
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
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> GetList(
        [FromQuery] ListQueryRequest q)
    {
        var (items, total) = await _service.GetPagedAsync(q.Search, q.Sort, q.Page, q.Size, q.Expand);
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

    /// <summary>Get enrollments for a specific course.</summary>
    [HttpGet("{courseId:int}/enrollments")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> GetEnrollmentsByCourseId(
        int courseId,
        [FromQuery] ListQueryRequest q,
        [FromServices] IEnrollmentService enrollmentService)
    {
        var course = await _service.GetByIdAsync(courseId);
        if (course is null)
            return NotFound(ApiResponse<object>.Fail($"Course {courseId} not found."));

        var result = await enrollmentService.GetListByCourseIdAsync(courseId, new EnrollmentListQuery
        {
            Search = q.Search,
            Sort   = q.Sort,
            Page   = q.Page,
            Size   = q.Size,
            Expand = q.Expand
        });

        var responses = result.Items.Select(MapEnrollmentResponse).ToList();

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

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CourseResponse>>> Create([FromBody] CreateCourseRequest r)
    {
        var bm = await _service.CreateAsync(r.CourseName, r.SemesterId);
        return CreatedAtAction(nameof(GetById), new { id = bm.CourseId },
            ApiResponse<CourseResponse>.Ok(MapToResponse(bm), "Course created."));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CourseResponse>>> Update(int id, [FromBody] UpdateCourseRequest r)
    {
        var bm = await _service.UpdateAsync(id, r.CourseName, r.SemesterId);
        if (bm is null)
            return NotFound(ApiResponse<CourseResponse>.Fail($"Course {id} not found."));

        return Ok(ApiResponse<CourseResponse>.Ok(MapToResponse(bm), "Course updated."));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.Fail($"Course {id} not found."));

        return Ok(ApiResponse<object>.Ok(null!, "Course deleted."));
    }

    private static CourseResponse MapToResponse(CourseBM bm) => new()
    {
        CourseId     = bm.CourseId,
        CourseName   = bm.CourseName,
        SemesterId   = bm.SemesterId,
        SemesterName = bm.SemesterName,
        Semester = bm.Semester is null ? null : new SemesterResponse
        {
            SemesterId   = bm.Semester.SemesterId,
            SemesterName = bm.Semester.SemesterName,
            StartDate    = bm.Semester.StartDate,
            EndDate      = bm.Semester.EndDate
        }
    };

    private static EnrollmentResponse MapEnrollmentResponse(EnrollmentBM bm) => new()
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
            SemesterName = bm.Course.SemesterName,
            Semester = bm.Course.Semester is null ? null : new SemesterResponse
            {
                SemesterId   = bm.Course.Semester.SemesterId,
                SemesterName = bm.Course.Semester.SemesterName,
                StartDate    = bm.Course.Semester.StartDate,
                EndDate      = bm.Course.Semester.EndDate
            }
        }
    };
}
