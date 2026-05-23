using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Implementations;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Implementations;
using PRN232.LMS.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
// Use a fixed ServerVersion instead of AutoDetect to avoid a live DB call
// at startup (which would crash the container before MySQL is ready).
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddDbContext<LmsDbContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 0))
    ));

// ── Repositories ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<
    PRN232.LMS.Repositories.Repositories.IStudentRepository,
    PRN232.LMS.Repositories.Repositories.StudentRepository>();
builder.Services.AddScoped<
    PRN232.LMS.Repositories.Repositories.IEnrollmentRepository,
    PRN232.LMS.Repositories.Repositories.EnrollmentRepository>();

// ── UnitOfWork (Course / Semester / Subject services) ─────────────────────────
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IStudentService,    StudentService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ICourseService,     CourseService>();
builder.Services.AddScoped<ISemesterService,   SemesterService>();
builder.Services.AddScoped<ISubjectService,    SubjectService>();

// ── Controllers + JSON ────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase);

// ── Swagger ───────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title       = "LMS API - PRN232 Lab 1",
        Version     = "v1",
        Description = "Learning Management System REST API"
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// ── Auto-migrate with retry (MySQL may still be initialising inside Docker) ───
using (var scope = app.Services.CreateScope())
{
    var db      = scope.ServiceProvider.GetRequiredService<LmsDbContext>();
    var logger  = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var retries = 5;

    while (retries > 0)
    {
        try
        {
            logger.LogInformation("Applying database migrations...");
            db.Database.Migrate();
            logger.LogInformation("Database migration completed.");
            break;
        }
        catch (Exception ex)
        {
            retries--;
            logger.LogWarning(ex, "Migration failed. Retries left: {Retries}. Waiting 3s...", retries);
            if (retries == 0) throw;
            Thread.Sleep(3000);
        }
    }
}

// ── Global Exception Handler → always returns ApiResponse<T> with HTTP 500 ────
app.UseExceptionHandler(err => err.Run(async ctx =>
{
    ctx.Response.StatusCode  = StatusCodes.Status500InternalServerError;
    ctx.Response.ContentType = "application/json";

    var feature = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
    var message = app.Environment.IsDevelopment()
        ? (feature?.Error?.Message ?? "Internal server error")
        : "Internal server error";

    await ctx.Response.WriteAsJsonAsync(new
    {
        success = false,
        message,
        data   = (object?)null,
        errors = (object?)null
    });
}));

// ── Swagger UI (always on — required inside Docker) ───────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "LMS API v1");
    c.RoutePrefix = string.Empty; // Swagger at root: http://localhost:8080
});

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
