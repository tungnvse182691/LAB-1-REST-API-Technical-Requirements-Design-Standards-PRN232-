using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Data;

public class LmsDbContext : DbContext
{
    public LmsDbContext(DbContextOptions<LmsDbContext> options) : base(options) { }

    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Semester ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Semester>(e =>
        {
            e.ToTable("Semesters");
            e.HasKey(s => s.SemesterId);
            e.Property(s => s.SemesterName).HasColumnType("varchar(100)").IsRequired();
            e.Property(s => s.StartDate).HasColumnType("datetime");
            e.Property(s => s.EndDate).HasColumnType("datetime");
        });

        // ── Course ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Course>(e =>
        {
            e.ToTable("Courses");
            e.HasKey(c => c.CourseId);
            e.Property(c => c.CourseName).HasColumnType("varchar(100)").IsRequired();
            e.HasOne(c => c.Semester)
             .WithMany(s => s.Courses)
             .HasForeignKey(c => c.SemesterId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Subject ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Subject>(e =>
        {
            e.ToTable("Subjects");
            e.HasKey(s => s.SubjectId);
            e.Property(s => s.SubjectCode).HasColumnType("varchar(20)").IsRequired();
            e.Property(s => s.SubjectName).HasColumnType("varchar(100)");
            e.Property(s => s.Credit).HasColumnType("int");
        });

        // ── Student ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Student>(e =>
        {
            e.ToTable("Students");
            e.HasKey(s => s.StudentId);
            e.Property(s => s.FullName).HasColumnType("varchar(100)");
            e.Property(s => s.Email).HasColumnType("varchar(100)");
            e.Property(s => s.DateOfBirth).HasColumnType("datetime");
        });

        // ── Enrollment ────────────────────────────────────────────────────────
        modelBuilder.Entity<Enrollment>(e =>
        {
            e.ToTable("Enrollments");
            e.HasKey(en => en.EnrollmentId);
            e.Property(en => en.Status).HasColumnType("varchar(20)");
            e.Property(en => en.EnrollDate).HasColumnType("datetime");
            e.HasOne(en => en.Student)
             .WithMany(s => s.Enrollments)
             .HasForeignKey(en => en.StudentId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(en => en.Course)
             .WithMany(c => c.Enrollments)
             .HasForeignKey(en => en.CourseId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Seed Data ─────────────────────────────────────────────────────────
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // 5 Semesters
        modelBuilder.Entity<Semester>().HasData(
            new Semester { SemesterId = 1, SemesterName = "SP2023", StartDate = new DateTime(2023, 1, 9),  EndDate = new DateTime(2023, 5, 19) },
            new Semester { SemesterId = 2, SemesterName = "SU2023", StartDate = new DateTime(2023, 5, 22), EndDate = new DateTime(2023, 9, 15) },
            new Semester { SemesterId = 3, SemesterName = "FA2023", StartDate = new DateTime(2023, 9, 18), EndDate = new DateTime(2024, 1, 19) },
            new Semester { SemesterId = 4, SemesterName = "SP2024", StartDate = new DateTime(2024, 1, 22), EndDate = new DateTime(2024, 5, 24) },
            new Semester { SemesterId = 5, SemesterName = "SU2024", StartDate = new DateTime(2024, 5, 27), EndDate = new DateTime(2024, 9, 20) }
        );

        // 10 Subjects
        modelBuilder.Entity<Subject>().HasData(
            new Subject { SubjectId = 1,  SubjectCode = "MATH101",  SubjectName = "Mathematics",          Credit = 3 },
            new Subject { SubjectId = 2,  SubjectCode = "PHYS101",  SubjectName = "Physics",              Credit = 3 },
            new Subject { SubjectId = 3,  SubjectCode = "OOP201",   SubjectName = "Object-Oriented Programming", Credit = 3 },
            new Subject { SubjectId = 4,  SubjectCode = "DB301",    SubjectName = "Database Systems",     Credit = 3 },
            new Subject { SubjectId = 5,  SubjectCode = "WEB401",   SubjectName = "Web Development",      Credit = 3 },
            new Subject { SubjectId = 6,  SubjectCode = "AI501",    SubjectName = "Artificial Intelligence", Credit = 3 },
            new Subject { SubjectId = 7,  SubjectCode = "NET301",   SubjectName = "Computer Networks",    Credit = 3 },
            new Subject { SubjectId = 8,  SubjectCode = "OS201",    SubjectName = "Operating Systems",    Credit = 3 },
            new Subject { SubjectId = 9,  SubjectCode = "SEC401",   SubjectName = "Cybersecurity",        Credit = 3 },
            new Subject { SubjectId = 10, SubjectCode = "MOB501",   SubjectName = "Mobile Development",   Credit = 3 }
        );

        // 20 Courses (4 per semester)
        modelBuilder.Entity<Course>().HasData(
            new Course { CourseId = 1,  CourseName = "Mathematics SP2023",          SemesterId = 1 },
            new Course { CourseId = 2,  CourseName = "Physics SP2023",              SemesterId = 1 },
            new Course { CourseId = 3,  CourseName = "OOP SP2023",                  SemesterId = 1 },
            new Course { CourseId = 4,  CourseName = "Database SP2023",             SemesterId = 1 },
            new Course { CourseId = 5,  CourseName = "Web Development SU2023",      SemesterId = 2 },
            new Course { CourseId = 6,  CourseName = "AI SU2023",                   SemesterId = 2 },
            new Course { CourseId = 7,  CourseName = "Networks SU2023",             SemesterId = 2 },
            new Course { CourseId = 8,  CourseName = "Operating Systems SU2023",    SemesterId = 2 },
            new Course { CourseId = 9,  CourseName = "Cybersecurity FA2023",        SemesterId = 3 },
            new Course { CourseId = 10, CourseName = "Mobile Dev FA2023",           SemesterId = 3 },
            new Course { CourseId = 11, CourseName = "Mathematics FA2023",          SemesterId = 3 },
            new Course { CourseId = 12, CourseName = "Physics FA2023",              SemesterId = 3 },
            new Course { CourseId = 13, CourseName = "OOP SP2024",                  SemesterId = 4 },
            new Course { CourseId = 14, CourseName = "Database SP2024",             SemesterId = 4 },
            new Course { CourseId = 15, CourseName = "Web Development SP2024",      SemesterId = 4 },
            new Course { CourseId = 16, CourseName = "AI SP2024",                   SemesterId = 4 },
            new Course { CourseId = 17, CourseName = "Networks SU2024",             SemesterId = 5 },
            new Course { CourseId = 18, CourseName = "Operating Systems SU2024",    SemesterId = 5 },
            new Course { CourseId = 19, CourseName = "Cybersecurity SU2024",        SemesterId = 5 },
            new Course { CourseId = 20, CourseName = "Mobile Dev SU2024",           SemesterId = 5 }
        );

        // 50 Students (Vietnamese names, FPT-style emails)
        var students = new Student[]
        {
            new Student { StudentId =  1, FullName = "Nguyễn Văn An",       Email = "s001@fpt.edu.vn", DateOfBirth = new DateTime(2002, 3, 15) },
            new Student { StudentId =  2, FullName = "Trần Thị Bình",       Email = "s002@fpt.edu.vn", DateOfBirth = new DateTime(2001, 7, 22) },
            new Student { StudentId =  3, FullName = "Lê Văn Cường",        Email = "s003@fpt.edu.vn", DateOfBirth = new DateTime(2003, 1, 10) },
            new Student { StudentId =  4, FullName = "Phạm Thị Dung",       Email = "s004@fpt.edu.vn", DateOfBirth = new DateTime(2002, 11, 5) },
            new Student { StudentId =  5, FullName = "Hoàng Văn Em",        Email = "s005@fpt.edu.vn", DateOfBirth = new DateTime(2000, 6, 18) },
            new Student { StudentId =  6, FullName = "Vũ Thị Phương",       Email = "s006@fpt.edu.vn", DateOfBirth = new DateTime(2001, 9, 30) },
            new Student { StudentId =  7, FullName = "Đặng Văn Giang",      Email = "s007@fpt.edu.vn", DateOfBirth = new DateTime(2003, 4, 25) },
            new Student { StudentId =  8, FullName = "Bùi Thị Hoa",         Email = "s008@fpt.edu.vn", DateOfBirth = new DateTime(2002, 8, 12) },
            new Student { StudentId =  9, FullName = "Ngô Văn Hùng",        Email = "s009@fpt.edu.vn", DateOfBirth = new DateTime(2000, 12, 3) },
            new Student { StudentId = 10, FullName = "Đinh Thị Lan",        Email = "s010@fpt.edu.vn", DateOfBirth = new DateTime(2001, 2, 28) },
            new Student { StudentId = 11, FullName = "Trịnh Văn Minh",      Email = "s011@fpt.edu.vn", DateOfBirth = new DateTime(2002, 5, 17) },
            new Student { StudentId = 12, FullName = "Lý Thị Ngọc",         Email = "s012@fpt.edu.vn", DateOfBirth = new DateTime(2003, 10, 8) },
            new Student { StudentId = 13, FullName = "Phan Văn Oanh",       Email = "s013@fpt.edu.vn", DateOfBirth = new DateTime(2001, 1, 14) },
            new Student { StudentId = 14, FullName = "Dương Thị Phúc",      Email = "s014@fpt.edu.vn", DateOfBirth = new DateTime(2000, 7, 19) },
            new Student { StudentId = 15, FullName = "Tô Văn Quân",         Email = "s015@fpt.edu.vn", DateOfBirth = new DateTime(2002, 3, 6) },
            new Student { StudentId = 16, FullName = "Hồ Thị Ry",           Email = "s016@fpt.edu.vn", DateOfBirth = new DateTime(2003, 6, 23) },
            new Student { StudentId = 17, FullName = "Lưu Văn Sơn",         Email = "s017@fpt.edu.vn", DateOfBirth = new DateTime(2001, 11, 11) },
            new Student { StudentId = 18, FullName = "Mai Thị Tâm",         Email = "s018@fpt.edu.vn", DateOfBirth = new DateTime(2000, 4, 29) },
            new Student { StudentId = 19, FullName = "Cao Văn Tuấn",        Email = "s019@fpt.edu.vn", DateOfBirth = new DateTime(2002, 9, 2) },
            new Student { StudentId = 20, FullName = "Đỗ Thị Uyên",         Email = "s020@fpt.edu.vn", DateOfBirth = new DateTime(2001, 12, 16) },
            new Student { StudentId = 21, FullName = "Nguyễn Thị Vân",      Email = "s021@fpt.edu.vn", DateOfBirth = new DateTime(2003, 2, 7) },
            new Student { StudentId = 22, FullName = "Trần Văn Xuân",       Email = "s022@fpt.edu.vn", DateOfBirth = new DateTime(2000, 8, 21) },
            new Student { StudentId = 23, FullName = "Lê Thị Yến",          Email = "s023@fpt.edu.vn", DateOfBirth = new DateTime(2002, 1, 31) },
            new Student { StudentId = 24, FullName = "Phạm Văn Zung",       Email = "s024@fpt.edu.vn", DateOfBirth = new DateTime(2001, 5, 9) },
            new Student { StudentId = 25, FullName = "Hoàng Thị Ánh",       Email = "s025@fpt.edu.vn", DateOfBirth = new DateTime(2003, 7, 14) },
            new Student { StudentId = 26, FullName = "Vũ Văn Bảo",          Email = "s026@fpt.edu.vn", DateOfBirth = new DateTime(2000, 10, 26) },
            new Student { StudentId = 27, FullName = "Đặng Thị Châu",       Email = "s027@fpt.edu.vn", DateOfBirth = new DateTime(2002, 6, 3) },
            new Student { StudentId = 28, FullName = "Bùi Văn Dũng",        Email = "s028@fpt.edu.vn", DateOfBirth = new DateTime(2001, 3, 18) },
            new Student { StudentId = 29, FullName = "Ngô Thị Ếch",         Email = "s029@fpt.edu.vn", DateOfBirth = new DateTime(2003, 9, 27) },
            new Student { StudentId = 30, FullName = "Đinh Văn Phong",      Email = "s030@fpt.edu.vn", DateOfBirth = new DateTime(2000, 1, 5) },
            new Student { StudentId = 31, FullName = "Trịnh Thị Gấm",       Email = "s031@fpt.edu.vn", DateOfBirth = new DateTime(2002, 4, 13) },
            new Student { StudentId = 32, FullName = "Lý Văn Hải",          Email = "s032@fpt.edu.vn", DateOfBirth = new DateTime(2001, 8, 24) },
            new Student { StudentId = 33, FullName = "Phan Thị Iris",        Email = "s033@fpt.edu.vn", DateOfBirth = new DateTime(2003, 12, 1) },
            new Student { StudentId = 34, FullName = "Dương Văn Khoa",      Email = "s034@fpt.edu.vn", DateOfBirth = new DateTime(2000, 5, 20) },
            new Student { StudentId = 35, FullName = "Tô Thị Linh",         Email = "s035@fpt.edu.vn", DateOfBirth = new DateTime(2002, 2, 8) },
            new Student { StudentId = 36, FullName = "Hồ Văn Mạnh",         Email = "s036@fpt.edu.vn", DateOfBirth = new DateTime(2001, 6, 15) },
            new Student { StudentId = 37, FullName = "Lưu Thị Nga",         Email = "s037@fpt.edu.vn", DateOfBirth = new DateTime(2003, 11, 4) },
            new Student { StudentId = 38, FullName = "Mai Văn Oanh",        Email = "s038@fpt.edu.vn", DateOfBirth = new DateTime(2000, 3, 22) },
            new Student { StudentId = 39, FullName = "Cao Thị Phượng",      Email = "s039@fpt.edu.vn", DateOfBirth = new DateTime(2002, 7, 11) },
            new Student { StudentId = 40, FullName = "Đỗ Văn Quý",          Email = "s040@fpt.edu.vn", DateOfBirth = new DateTime(2001, 10, 17) },
            new Student { StudentId = 41, FullName = "Nguyễn Văn Rồng",     Email = "s041@fpt.edu.vn", DateOfBirth = new DateTime(2003, 1, 28) },
            new Student { StudentId = 42, FullName = "Trần Thị Sương",      Email = "s042@fpt.edu.vn", DateOfBirth = new DateTime(2000, 9, 6) },
            new Student { StudentId = 43, FullName = "Lê Văn Thắng",        Email = "s043@fpt.edu.vn", DateOfBirth = new DateTime(2002, 12, 19) },
            new Student { StudentId = 44, FullName = "Phạm Thị Uyên",       Email = "s044@fpt.edu.vn", DateOfBirth = new DateTime(2001, 4, 7) },
            new Student { StudentId = 45, FullName = "Hoàng Văn Vinh",      Email = "s045@fpt.edu.vn", DateOfBirth = new DateTime(2003, 8, 16) },
            new Student { StudentId = 46, FullName = "Vũ Thị Xuân",         Email = "s046@fpt.edu.vn", DateOfBirth = new DateTime(2000, 2, 14) },
            new Student { StudentId = 47, FullName = "Đặng Văn Yên",        Email = "s047@fpt.edu.vn", DateOfBirth = new DateTime(2002, 10, 3) },
            new Student { StudentId = 48, FullName = "Bùi Thị Zara",        Email = "s048@fpt.edu.vn", DateOfBirth = new DateTime(2001, 7, 25) },
            new Student { StudentId = 49, FullName = "Ngô Văn Anh",         Email = "s049@fpt.edu.vn", DateOfBirth = new DateTime(2003, 5, 12) },
            new Student { StudentId = 50, FullName = "Đinh Thị Bích",       Email = "s050@fpt.edu.vn", DateOfBirth = new DateTime(2000, 11, 30) }
        };
        modelBuilder.Entity<Student>().HasData(students);

        // 500 Enrollments – deterministic distribution across 50 students × 20 courses
        // Each student gets 10 enrollments (one per course slot), cycling through courses
        var statuses = new[] { "Active", "Inactive", "Completed" };
        var enrollments = new List<Enrollment>();
        int enrollId = 1;
        var baseDate = new DateTime(2023, 1, 15);

        for (int s = 1; s <= 50; s++)
        {
            for (int c = 0; c < 10; c++)
            {
                int courseId = ((s - 1 + c) % 20) + 1;
                string status = statuses[(s + c) % 3];
                enrollments.Add(new Enrollment
                {
                    EnrollmentId = enrollId++,
                    StudentId    = s,
                    CourseId     = courseId,
                    EnrollDate   = baseDate.AddDays((s - 1) * 3 + c * 7),
                    Status       = status
                });
            }
        }

        modelBuilder.Entity<Enrollment>().HasData(enrollments);
    }
}
