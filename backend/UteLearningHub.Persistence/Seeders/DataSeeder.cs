using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories.UnitOfWork;
using UteLearningHub.Persistence.Identity;
using DomainType = UteLearningHub.Domain.Entities.Type;


namespace UteLearningHub.Persistence.Seeders;

public class DataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public DataSeeder(ApplicationDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }
    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedAdminAsync(); // Tạo admin trước để lấy adminId
        await SeedSystemAsync();
        await SeedFacultiesAsync();
        await SeedMajorsAsync();
        await SeedUsersAsync(); // Tạo các users khác (students)
        await SeedSubjectsAsync();
        await SeedTypesAsync();
        await SeedTagsAsync();
        await SeedAuthorsAsync();


        await _context.SaveChangesAsync();
    }
    private async Task SeedSystemAsync()
    {
        if (await _context.SystemSettings.AnyAsync()) return;

        var name = SystemName.CreateDocument;

        var system = new SystemSetting
        {
            Name = name,
            Value = 0
        };

        await _context.SystemSettings.AddAsync(system);
        await _context.SaveChangesAsync();
    }



    private async Task SeedRolesAsync()
    {
        var roles = new[] { "Admin", "Student" };

        foreach (var roleName in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpper()
                });
            }
        }
    }
    private async Task SeedFacultiesAsync()
    {
        if (await _context.Faculty.AnyAsync()) return;

        var adminUser = await _userManager.FindByEmailAsync("admin@ute.edu.vn");
        if (adminUser == null)
            throw new InvalidOperationException("Admin user must be created before seeding faculties");

        var adminId = adminUser.Id;

        var faculties = new List<Faculty>
        {
            new()
            {
                Id = Guid.NewGuid(),
                FacultyName = "Khoa Cơ Khí",
                FacultyCode = "CK",
                Logo = "/images/faculties/logo_ck.png",
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                FacultyName = "Khoa Điện - Điện tử",
                FacultyCode = "DDT",
                Logo = "/images/faculties/logo_ddt.png",
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                FacultyName = "Khoa Kỹ Thuật Xây Dựng",
                FacultyCode = "KTXD",
                Logo = "/images/faculties/logo_ktxd.png",
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                FacultyName = "Khoa Công Nghệ Hóa Học - Môi Trường",
                FacultyCode = "CNHHMT",
                Logo = "/images/faculties/logo_cnhhmt.png",
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                FacultyName = "Khoa Sư Phạm Công Nghiệp",
                FacultyCode = "SPCN",
                Logo = "/images/faculties/logo_spcn.png",
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                FacultyName = "Khoa Công Nghệ Số",
                FacultyCode = "CNS",
                Logo = "/images/faculties/logo_cns.png",
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        await _context.Faculty.AddRangeAsync(faculties);
        await _context.SaveChangesAsync();
    }
    private async Task SeedMajorsAsync()
    {
        if (await _context.Majors.AnyAsync()) return;

        var ckFaculty = await _context.Faculty.FirstAsync(f => f.FacultyCode == "CK");
        var ddtFaculty = await _context.Faculty.FirstAsync(f => f.FacultyCode == "DDT");
        var ktxdFaculty = await _context.Faculty.FirstAsync(f => f.FacultyCode == "KTXD");
        var cnhhmtFaculty = await _context.Faculty.FirstAsync(f => f.FacultyCode == "CNHHMT");
        var spcnFaculty = await _context.Faculty.FirstAsync(f => f.FacultyCode == "SPCN");
        var cnsFaculty = await _context.Faculty.FirstAsync(f => f.FacultyCode == "CNS");

        var adminUser = await _userManager.FindByEmailAsync("admin@ute.edu.vn");
        if (adminUser == null)
            throw new InvalidOperationException("Admin user must be created before seeding majors");

        var adminId = adminUser.Id;

        var majors = new List<Major>
        {
            new()
            {
                Id = Guid.NewGuid(),
                MajorName = "Sư phạm kỹ thuật công nghiệp",
                MajorCode = "7140214",
                FacultyId = spcnFaculty.Id,
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                MajorName = "Công nghệ thông tin",
                MajorCode = "7480201",
                FacultyId = cnsFaculty.Id,
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                MajorName = "Công nghệ kỹ thuật xây dựng",
                MajorCode = "7510103",
                FacultyId = ktxdFaculty.Id,
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                MajorName = "Công nghệ kỹ thuật giao thông",
                MajorCode = "7510104",
                FacultyId = ktxdFaculty.Id,
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                MajorName = "Công nghệ kỹ thuật cơ khí",
                MajorCode = "7510201",
                FacultyId = ckFaculty.Id,
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                MajorName = "Công nghệ kỹ thuật cơ điện tử",
                MajorCode = "7510203",
                FacultyId = ckFaculty.Id,
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                MajorName = "Công nghệ kỹ thuật ô tô",
                MajorCode = "7510205",
                FacultyId = ckFaculty.Id,
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new(){
                Id = Guid.NewGuid(),
                MajorName = "Công nghệ kỹ thuật nhiệt",
                MajorCode = "7510206",
                FacultyId = ckFaculty.Id,
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                MajorName = "Công nghệ kỹ thuật điện, điện tử",
                MajorCode = "7510301",
                FacultyId = ckFaculty.Id,
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                MajorName = "Công nghệ kỹ thuật điện tử, viễn thông",
                MajorCode = "7510302",
                FacultyId = ddtFaculty.Id,
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                MajorName = "Công nghệ kỹ thuật điều khiển và tự động hóa",
                MajorCode = "7510303",
                FacultyId = ddtFaculty.Id,
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                MajorName = "Kỹ thuật cơ sở hạ tầng",
                MajorCode = "7580210",
                FacultyId = cnhhmtFaculty.Id,
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                MajorName = "Công nghệ kỹ thuật môi trường",
                MajorCode = "7510406",
                FacultyId = cnhhmtFaculty.Id,
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                MajorName = "Kỹ thuật thực phẩm",
                MajorCode = "7540102",
                FacultyId = cnhhmtFaculty.Id,
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                MajorName = "Công nghệ vật liệu",
                MajorCode = "7510402",
                FacultyId = cnhhmtFaculty.Id,
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                MajorName = "Công nghệ kỹ thuật hóa học",
                MajorCode = "7510401",
                FacultyId = cnhhmtFaculty.Id,
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                MajorName = "Công nghệ kỹ thuật kiến trúc",
                MajorCode = "7510101",
                FacultyId = ktxdFaculty.Id,
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        await _context.Majors.AddRangeAsync(majors);
        await _context.SaveChangesAsync();
    }
    private async Task SeedAdminAsync()
    {
        var existingAdmin = await _userManager.FindByEmailAsync("admin@ute.edu.vn");
        if (existingAdmin != null) return;

        var admin = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = "admin",
            Email = "admin@ute.edu.vn",
            EmailConfirmed = true,
            FullName = "Administrator",
            Introduction = "System Administrator",
            AvatarUrl = "https://static.vecteezy.com/system/resources/thumbnails/019/194/935/small_2x/global-admin-icon-color-outline-vector.jpg",
            TrustScore = 1000,
            TrustLever = TrustLever.Master,
            Gender = Gender.Other,
            MajorId = null, 
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _userManager.CreateAsync(admin, "Admin123!");
        await _userManager.AddToRoleAsync(admin, "Admin");
    }
    private async Task SeedTagsAsync()
    {
        if (await _context.Tags.AnyAsync()) return;

        var adminUser = await _userManager.FindByEmailAsync("admin@ute.edu.vn");
        if (adminUser == null)
            throw new InvalidOperationException("Admin user must be created before seeding tags");

        var adminId = adminUser.Id;

        var tags = new List<Tag>
        {
            // Programming Languages
            new() { Id = Guid.NewGuid(), TagName = "C#", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Java", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Python", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "JavaScript", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "TypeScript", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "C/C++", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "PHP", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "SQL", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },

            // Web Development
            new() { Id = Guid.NewGuid(), TagName = "HTML/CSS", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "React", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Angular", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Vue.js", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "ASP.NET", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Node.js", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Spring Boot", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },

            // Database & Backend
            new() { Id = Guid.NewGuid(), TagName = "SQL Server", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "MySQL", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "PostgreSQL", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "MongoDB", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Redis", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },

            // Mobile Development
            new() { Id = Guid.NewGuid(), TagName = "Android", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "iOS", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Flutter", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "React Native", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },

            // DevOps & Tools
            new() { Id = Guid.NewGuid(), TagName = "Docker", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Kubernetes", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Git", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "CI/CD", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Linux", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },

            // AI & Data Science
            new() { Id = Guid.NewGuid(), TagName = "Machine Learning", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Deep Learning", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Trí tuệ nhân tạo", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Data Science", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Computer Vision", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },

            // Computer Science Fundamentals
            new() { Id = Guid.NewGuid(), TagName = "Cấu trúc dữ liệu", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Giải thuật", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "OOP", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Design Patterns", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Mạng máy tính", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Hệ điều hành", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Cơ sở dữ liệu", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },

            // Software Engineering
            new() { Id = Guid.NewGuid(), TagName = "Công nghệ phần mềm", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Kiểm thử phần mềm", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Agile/Scrum", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Clean Code", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Microservices", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "API", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },

            // Mathematics
            new() { Id = Guid.NewGuid(), TagName = "Toán cao cấp", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Đại số tuyến tính", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Giải tích", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Xác suất thống kê", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Toán rời rạc", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },

            // General Education
            new() { Id = Guid.NewGuid(), TagName = "Tiếng Anh", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Kỹ năng mềm", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Đồ án", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Luận văn", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Tài liệu ôn thi", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },

            // Engineering Fields
            new() { Id = Guid.NewGuid(), TagName = "Cơ khí", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Điện - Điện tử", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Xây dựng", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Hóa học", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Môi trường", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Nhiệt - Lạnh", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Ô tô", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), TagName = "Tự động hóa", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved }
        };

        await _context.Tags.AddRangeAsync(tags);
        await _context.SaveChangesAsync();
    }
    private async Task SeedUsersAsync()
    {
        // Check if students already exist
        var existingStudents = await _userManager.Users
            .Where(u => u.Email != null && u.Email.Contains("student"))
            .AnyAsync();
        if (existingStudents) return;

        var cnsMajor = await _context.Majors.FirstAsync(m => m.MajorCode == "7480201");

        // Student Users
        var students = new[]
        {
            new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = "student1",
                Email = "student1@student.ute.edu.vn",
                EmailConfirmed = true,
                FullName = "Sinh viên 1",
                Introduction = "Sinh viên năm 3 CNS",
                AvatarUrl = "https://img.icons8.com/office40/1200/administrator-male.jpg",
                TrustScore = 3,
                TrustLever = TrustLever.None,
                Gender = Gender.Male,
                MajorId = cnsMajor.Id,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = "student2",
                Email = "student2@student.ute.edu.vn",
                EmailConfirmed = true,
                FullName = "Sinh viên 2",
                Introduction = "Sinh viên năm 2 CNS",
                AvatarUrl = "https://img.icons8.com/office40/1200/administrator-male.jpg",
                TrustScore = 6,
                TrustLever = TrustLever.Contributor,
                Gender = Gender.Female,
                MajorId = cnsMajor.Id,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = "student3",
                Email = "student3@student.ute.edu.vn",
                EmailConfirmed = true,
                FullName = "Sinh viên 3",
                Introduction = "Sinh viên năm 2 CNS",
                AvatarUrl = "https://img.icons8.com/office40/1200/administrator-male.jpg",
                TrustScore = 31,
                TrustLever = TrustLever.TrustedMember,
                Gender = Gender.Female,
                MajorId = cnsMajor.Id,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = "student4",
                Email = "student4@student.ute.edu.vn",
                EmailConfirmed = true,
                FullName = "Sinh viên 4",
                Introduction = "Sinh viên năm 2 CNS",
                AvatarUrl = "https://img.icons8.com/office40/1200/administrator-male.jpg",
                TrustScore = 61,
                TrustLever = TrustLever.Moderator,
                Gender = Gender.Female,
                MajorId = cnsMajor.Id,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        foreach (var student in students)
        {
            await _userManager.CreateAsync(student, "Student123!");
            await _userManager.AddToRoleAsync(student, "Student");
        }
    }
    private async Task SeedSubjectsAsync()
    {
        if (await _context.Subjects.AnyAsync()) return;

        var majors = await _context.Majors.ToListAsync();
        var majorDict = majors.ToDictionary(m => m.MajorCode, m => m.Id);

        var adminUser = await _userManager.FindByEmailAsync("admin@ute.edu.vn");
        if (adminUser == null) return;

        var subjects = new List<Subject>();
        var subjectMajors = new List<SubjectMajor>();

        Subject CreateSubject(string name, string code, params string[] majorCodes)
        {
            var subject = new Subject
            {
                Id = Guid.NewGuid(),
                SubjectName = name,
                SubjectCode = code,
                CreatedById = adminUser.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                Status = ContentStatus.Approved
            };
            subjects.Add(subject);

            // Create SubjectMajor relationships
            foreach (var majorCode in majorCodes)
            {
                if (majorDict.TryGetValue(majorCode, out var majorId))
                {
                    subjectMajors.Add(new SubjectMajor
                    {
                        SubjectId = subject.Id,
                        MajorId = majorId
                    });
                }
            }

            return subject;
        }

        // ===== MÔN HỌC CHUNG (Tất cả ngành đều học) =====
        var allMajorCodes = majorDict.Keys.ToArray();
        CreateSubject("Đại số tuyến tính", "5319001", allMajorCodes);
        CreateSubject("Giải tích 1", "5319002", allMajorCodes);
        CreateSubject("Giải tích 2", "5319003", allMajorCodes);
        CreateSubject("Vẽ kỹ thuật", "5504026", allMajorCodes);
        CreateSubject("Ngoại ngữ 1", "5413002", allMajorCodes);
        CreateSubject("Ngoại ngữ 2", "5413003", allMajorCodes);
        CreateSubject("Ngoại ngữ 3", "5413004", allMajorCodes);
        CreateSubject("Pháp luật đại cương", "5211005", allMajorCodes);
        CreateSubject("Tin học cơ bản", "5505251", allMajorCodes);
        CreateSubject("Lịch sử Đảng Cộng sản Việt Nam", "5209008", allMajorCodes);
        CreateSubject("Triết học Mác-Lênin", "5209005", allMajorCodes);
        CreateSubject("Kinh tế chính trị", "5209006", allMajorCodes);
        CreateSubject("Chủ nghĩa xã hội khoa học", "5209007", allMajorCodes);
        CreateSubject("Tư tưởng Hồ Chí Minh", "5209004", allMajorCodes);
        CreateSubject("Kỹ năng giao tiếp", "5502003", allMajorCodes);
        CreateSubject("Kỹ năng làm việc nhóm", "5502004", allMajorCodes);
        CreateSubject("Ngoại Ngữ cơ bản", "5413001", allMajorCodes);
        CreateSubject("Ngoại Ngữ IV", "5413005", allMajorCodes);
        CreateSubject("Ngoại Ngữ V", "5413006", allMajorCodes);
        CreateSubject("Đổi mới sáng tạo, khởi nghiệp", "5502010", allMajorCodes);
        CreateSubject("Kỹ năng lãnh đạo, quản lý", "5502009", allMajorCodes);

        // ===== MÔN HỌC CHUYÊN NGÀNH CNTT =====
        CreateSubject("Cấu trúc máy tính", "5505003", "7480201", "7140214"); // Công nghệ thông tin
        CreateSubject("Cơ sở dữ liệu I", "5505127", "7480201", "7140214");
        CreateSubject("Lập trình cơ bản với C", "5505166", "7480201", "7140214");
        CreateSubject("Lý thuyết đồ thị ", "5505177", "7480201", "7140214");
        CreateSubject("Mạng máy tính", "5505181", "7480201", "7140214");
        CreateSubject("Nhập môn ngành CNTT", "5505186", "7480201", "7140214");
        CreateSubject("TH Cơ sở dữ liệu I", "5505194", "7480201", "7140214");
        CreateSubject("TH Lập trình cơ bản với C", "5505201", "7480201", "7140214");
        CreateSubject("TH Mạng Máy tính", "5505209", "7480201", "7140214");
        CreateSubject("TH Thiết kế Web", "5505213", "7480201", "7140214");
        CreateSubject("Thiết kế Web", "5505222", "7480201", "7140214");
        CreateSubject("Cấu trúc dữ liệu và giải thuật", "5505121", "7480201", "7140214");
        CreateSubject("Chuyên đề ngôn ngữ lập trình", "5505320", "7480201", "7140214");
        CreateSubject("Cơ sở dữ liêu II", "5505128", "7480201", "7140214");
        CreateSubject("Công nghệ mạng không dây ", "5505321", "7480201", "7140214");
        CreateSubject("Công nghệ phần mềm", "5505132", "7480201", "7140214");
        CreateSubject("Công nghệ XML", "5505135", "7480201", "7140214");
        CreateSubject("Đồ Án Kiểm thử phần mềm", "5505322", "7480201", "7140214");
        CreateSubject("Đồ án phần mềm", "5505143", "7480201", "7140214");
        CreateSubject("Đồ án Tốt nghiệp CNTT", "5505323", "7480201", "7140214");
        CreateSubject("Học kỳ doanh nghiệp CNTT", "5505333", "7480201", "7140214");
        CreateSubject("Lập trình hướng đối tượng", "5505168", "7480201", "7140214");
        CreateSubject("Lập trình Java nâng cao", "5505169", "7480201", "7140214");
        CreateSubject("Lập trình trên ĐTDĐ", "5505172", "7480201", "7140214");
        CreateSubject("Lập trình trực quan", "5505173", "7480201", "7140214");
        CreateSubject("Lập trình web nâng cao", "5505175", "7480201", "7140214");
        CreateSubject("Ngoại ngữ chuyên ngành CNTT", "5505183", "7480201", "7140214");
        CreateSubject("Phân tích thiết kế hướng đối tượng", "5505188", "7480201", "7140214");
        CreateSubject("Quản trị dự án CNTT", "5505325", "7480201", "7140214");
        CreateSubject("Quản trị Mạng", "5505192", "7480201", "7140214");
        CreateSubject("TH Cơ sở dữ liệu II", "5505195", "7480201", "7140214");
        CreateSubject("TH CTDL & giải thuật", "5505198", "7480201", "7140214");
        CreateSubject("TH Lập trình hướng đối tượng", "5505202", "7480201", "7140214");
        CreateSubject("TH Lập trình Java nâng cao", "5505203", "7480201", "7140214");
        CreateSubject("TH Lập trình trên ĐTDÐ", "5505206", "7480201", "7140214");
        CreateSubject("TH Lập trình web nâng cao", "5505208", "7480201", "7140214");
        CreateSubject("Thị giác máy tính", "5505326", "7480201", "7140214");
        CreateSubject("Trí tuệ nhân tạo", "5505226", "7480201", "7140214");
        CreateSubject("TH Quản trị Mạng", "5505212", "7480201", "7140214");
        CreateSubject("TTCM Công nghệ mới", "5505328", "7480201", "7140214");
        CreateSubject("TTCM Thiết kế Cơ sở dữ liệu", "5505231", "7480201", "7140214");
        CreateSubject("Thực tập Tốt nghiệp CNTT", "5505327", "7480201", "7140214");
        CreateSubject("An ninh hệ thống", "5505119", "7480201", "7140214");
        CreateSubject("Bảo trì máy tính", "5505120", "7480201", "7140214");
        CreateSubject("Chuyên đề Cơ sở dữ liệu", "5505122", "7480201", "7140214");
        CreateSubject("Chuyên đề đồ hoạ", "5505123", "7480201", "7140214");
        CreateSubject("Chuyên đề mạng", "5505124", "7480201", "7140214");
        CreateSubject("Chuyên đề phần mềm", "5505126", "7480201", "7140214");
        CreateSubject("Công cụ và môi trường mã nguồn mở", "5505130", "7480201", "7140214");
        CreateSubject("CSDL phi quan hệ", "5505136", "7480201", "7140214");
        CreateSubject("Điện toán đám mây", "5505138", "7480201", "7140214");
        CreateSubject("Đồ hoạ đa truyền thông", "5505147", "7480201", "7140214");
        CreateSubject("Đồ họa máy tính", "5505148", "7480201", "7140214");
        CreateSubject("Đồ họa ứng dụng", "5505149", "7480201", "7140214");
        CreateSubject("Hệ điều hành", "5505155", "7480201", "7140214");
        CreateSubject("Hệ thống thông tin quản lý", "5505157", "7480201", "7140214");
        CreateSubject("Kế toán máy", "5505160", "7480201", "7140214");
        CreateSubject("Kho dữ liệu-Khai phá dữ liêu", "5505161", "7480201", "7140214");
        CreateSubject("Kinh tế hoc vi mô", "5502007", "7480201", "7140214");
        CreateSubject("Kỹ Thuật Điện tử", "5505038", "7480201", "7140214");
        CreateSubject("Lập trình C#", "5505165", "7480201", "7140214");
        CreateSubject("Lập trình mạng", "5505171", "7480201", "7140214");
        CreateSubject("Mạng diện rộng", "5505180", "7480201", "7140214");
        CreateSubject("Nguyên lý kế toán", "5505185", "7480201", "7140214");
        CreateSubject(" Phát triển Hệ thống thông tin", "5505324", "7480201", "7140214");
        CreateSubject("Quản trị học", "5505191", "7480201", "7140214");
        CreateSubject("TH Công cụ và môi trường mã nguồn mở", "5505196", "7480201", "7140214");
        CreateSubject("TH Công Nghệ XML", "5505197", "7480201", "7140214");
        CreateSubject("TH Đồ họa máy tính", "5505200", "7480201", "7140214");
        CreateSubject("TH Lập trình mạng", "5505204", "7480201", "7140214");
        CreateSubject("TH Lập trình trực quan", "5505207", "7480201", "7140214");
        CreateSubject("TH Lập trình C#", "5505193", "7480201", "7140214");
        CreateSubject("Thiết kế giao diện người dùng", "5505219", "7480201", "7140214");
        CreateSubject("Thiết kế Mạng", "5505220", "7480201", "7140214");
        CreateSubject("Thương mại điện tử", "5505223", "7480201", "7140214");
        CreateSubject("TTCM Mạng diện rộng", "5505329", "7480201", "7140214");

        // ===== MÔN HỌC CHUYÊN NGÀNH SPKTCN =====
        CreateSubject("Vật Lý Quang - Nguyên tử", "5305005", "7140214"); 
        CreateSubject("Giáo dục học", "5514004", "7140214");
        CreateSubject("Lý luận dạy học", "5514005", "7140214");
        CreateSubject("Phương pháp giảng dạy môn kỹ thuật", "5514010", "7140214");
        CreateSubject("Thực tập Sư phạm", "5514011", "7140214");

        // ===== MÔN HỌC CHUYÊN NGÀNH SPkT KIẾN TRÚC =====
        CreateSubject("Vật Lý Quang - Nguyên tử", "5305005", "7510101");
        CreateSubject("Hóa vô cơ", "5507320", "7510101");
        CreateSubject("TN Hóa vô cơ", "5507322", "7510101");
        CreateSubject("Hóa hữu cơ 1", "5507321", "7510101");
        CreateSubject("TN Hóa hữu cơ 1", "5507323", "7510101");
        CreateSubject("Hóa hữu cơ 2", "5507324", "7510101");
        CreateSubject("TN Hóa hữu cơ 2", "5507329", "7510101");
        CreateSubject("ATLĐ và vệ sinh công nghiệp ", "5507001", "7510101");
        CreateSubject("Ứng dụng CNTT trong Hóa học ", "5507194", "7510101");
        CreateSubject("Nhập môn ngành Vật liệu", "5507123", "7510101");
        CreateSubject("Quá trình và thiết bị thủy lực", "5507129", "7510101");
        CreateSubject("Quá trình và thiết bị truyền chất ", "5507130", "7510101");
        CreateSubject("Quá trình và thiết bị truyền nhiệt", "5507131", "7510101");
        CreateSubject("TN Quá trình và thiết bị thủy lực", "5507325", "7510101");
        CreateSubject("TN Quá trình và thiết bị truyền chất", "5507336", "7510101");
        CreateSubject("TN Quá trình và thiết bị truyền nhiệt", "5507332", "7510101");
        CreateSubject("Hóa lý", "5507326", "7510101");
        CreateSubject("TN Hóa lý", "5507330", "7510101");
        CreateSubject("Hóa phân tích", "5507327", "7510101");
        CreateSubject("TN Hóa phân tích", "5507331", "7510101");
        CreateSubject("Hóa tính toán", "5507333", "7510101");
        CreateSubject("Hóa học các hợp chất cao phân tử", "5507328", "7510101");
        CreateSubject("Hóa lý polymer", "5507334", "7510101");
        CreateSubject("Ăn mòn và bảo vệ kim loại", "5507335", "7510101");
        CreateSubject("TN ăn mòn và bảo vệ kim loại", "5507337", "7510101");
        CreateSubject("Các phương pháp phân tích vật lý và hóa lý", "5507048", "7510101");
        CreateSubject("TN Các PP phân tích Vật lý & Hóa lý", "5507146", "7510101");
        CreateSubject("Đồ án QT & TB", "5507090", "7510101");
        CreateSubject("Cơ sở thiết kế nhà máy Hóa", "5507252", "7510101");
        CreateSubject("Tiếng Anh chuyên ngành VL", "5507143", "7510101");
        CreateSubject("Chuyên đề ngành VL", "5507051", "7510101");
        CreateSubject("Thực tập nhận thức ", "5507261", "7510101");
        CreateSubject("Thực tập kỹ thuật", "5507260", "7510101");
        CreateSubject("Học kỳ doanh nghiệp VL", "5507246", "7510101");
        CreateSubject("Gia công polymer", "5507345", "7510101");
        CreateSubject("KTSX chất đẻo", "5507338", "7510101");
        CreateSubject("TN KTSX chất dẻo", "5507340", "7510101");
        CreateSubject("Kỹ thuật gia công cao su", "5507339", "7510101");
        CreateSubject("Vật liệu composite", "5507195", "7510101");
        CreateSubject("TN gia công composite", "5507169", "7510101");
        CreateSubject("CNSX vật liệu tiên tiến", "5507346", "7510101");
        CreateSubject("Vật liệu cấu trúc nano", "5507347", "7510101");
        CreateSubject("Đồ án chuyên ngành", "5507348", "7510101");
        CreateSubject("Ứng dụng hóa tính toán trong nghiên cứu vật liệu", "5507341", "7510101");
        CreateSubject("Quản lý dự án chuyên ngành", "5507257", "7510101");
        CreateSubject("Thực tập tốt nghiệp", "5507263", "7510101");
        CreateSubject("CNSX sợi hóa học", "5507350", "7510101");
        CreateSubject("CNSX cellulose và giấy", "5507351", "7510101");
        CreateSubject("TN CNSX cellulose và giấy", "5507353", "7510101");
        CreateSubject("Vật liệu nano ứng dụng", "5507352", "7510101");
        CreateSubject("CNSX Son-Vecni", "5507249", "7510101");
        CreateSubject("TN CNSX Son-Vecni", "5507354", "7510101");
        CreateSubject("Đồ án tốt nghiệp Kỹ sư VL", "5507262", "7510101");
        CreateSubject("Hóa hương liệu và mỹ phẩm", "5507106", "7510101");
        CreateSubject("TN Hóa hương liệu và mỹ phẩm", "5507173", "7510101");
        CreateSubject("CNSX Phân bón", "5507342", "7510101");
        CreateSubject("TN CNSX Phân bón", "5507343", "7510101");
        CreateSubject("Công nghệ các sản phẩm tấy rửa ", "5507344", "7510101");

        // ===== MÔN HỌC CHUYÊN NGÀNH CÔNG NGHỆ KỸ THUẬT XÂY DỰNG =====
        CreateSubject("Cơ học kết cấu - Hệ tĩnh định", "5506142", "7510103");
        CreateSubject("Cơ lý thuyết", "5504088", "7510103");
        CreateSubject("Địa chất công trình", "5506014", "7510103");
        CreateSubject("Đồ án Kết cấu BTCТ", "5506017", "7510103");
        CreateSubject("Đồ án nền móng", "5506021", "7510103");
        CreateSubject("Kết cấu bê tông cốt thép", "5506029", "7510103");
        CreateSubject("Kết cấu thép", "5506033", "7510103");
        CreateSubject("Nền móng", "5506040", "7510103");
        CreateSubject("Nhập môn ngành XD", "Nhập môn ngành XD", "7510103");
        CreateSubject("Thí nghiệm cơ học", "5506046", "7510103");
        CreateSubject("Thực hành trắc địa", "5506250", "7510103");
        CreateSubject("TN cơ hoc đất", "5506054", "7510103");
        CreateSubject("TN Vật liệu xây dựng", "5506056", "7510103");
        CreateSubject("Trắc địa xây dựng", "5506059", "7510103");
        CreateSubject("Vẽ kỹ thuật xây dựng", "5506061", "7510103");
        CreateSubject("Vẽ xây dựng trên máy tính", "5506062", "7510103");
        CreateSubject("Cấu tạo KT nhà dân dụng", "5506004", "7510103");
        CreateSubject("Cơ học kết cấu - Hệ siêu tĩnh", "5506184", "7510103");
        CreateSubject("Đồ án Kết cấu Công trình BTCT", "5506260", "7510103");
        CreateSubject("Đồ án kết cấu thép", "5506257", "7510103");
        CreateSubject("Đồ án Kiến trúc XD", "5506019", "7510103");
        CreateSubject("Đồ án thi công đất và BTCT toàn khối", "5506258", "7510103");
        CreateSubject("Đồ án Tổ chức thi công", "5506259", "7510103");
        CreateSubject("Dự toán xây dựng", "5506025", "7510103");
        CreateSubject("Học kỳ Doanh nghiệp XD", "5506191", "7510103");
        CreateSubject("Kết cấu công trình BTСТ", "5506198", "7510103");
        CreateSubject("Kết cấu công trình thép ", "5506031", "7510103");
        CreateSubject("Kiến trúc xây dựng ", "5506034", "7510103");
        CreateSubject("Máy xây dựng", "5506039", "7510103");
        CreateSubject("Phương pháp Phần tử hữu hạn", "5506043", "7510103");
        CreateSubject("Quản lý dự án xây dựng", "5506044", "7510103");
        CreateSubject("Thi công đất và BTCT toàn khối", "5506187", "7510103");
        CreateSubject("Thi công lắp ghép, xây và hoàn thiện", "5506189", "7510103");
        CreateSubject("Thực tập Kỹ thuật XD", "5506256", "7510103");
        CreateSubject("Thực tập Nhận thức XD", "5506049", "7510103");
        CreateSubject("Tin học xây đựng", "5506053", "7510103");
        CreateSubject("TN kết cấu công trình", "5506055", "7510103");
        CreateSubject("Tổ chức thi công", "5506057", "7510103");
        CreateSubject("Cấp thoát nước", "5506003", "7510103");
        CreateSubject("Chuyên đề đấu thầu XD", "5506005", "7510103");
        CreateSubject("Chuyên đề kết cấu CT", "5506006", "7510103");
        CreateSubject("Chuyên đề nền móng СТ", "5506008", "7510103");
        CreateSubject("Chuyên đề thi công CT", "5506009", "7510103");
        CreateSubject("Chuyên đề ứng dụng BIM trong xây dựng", "5506152", "7510103");
        CreateSubject("Thanh quyết toán công trình XD", "5506045", "7510103");
        CreateSubject("Thiết bị kỹ thuật trong nhà", "5506047", "7510103");
        CreateSubject("Chuyên đề kiến trúc bền vững", "5506007", "7510103");
        CreateSubject("Đồ án Kỹ thuật thi công lắp ghép", "5506192", "7510103");
        CreateSubject("Đồ án tốt nghiệp kỹ sư XD", "5506194", "7510103");
        CreateSubject("Kết cấu nhà nhiều tầng", "5506032", "7510103");
        CreateSubject("Thi công nhà nhiều tầng", "5506193", "7510103");
        CreateSubject("Bảo dưỡng SC & nâng cấp CT", "5506002", "7510103");
        CreateSubject("Giám sát thi công xây dựng", "5506026", "7510103");
        CreateSubject("Kết cấu Bê tông ứng lực trước", "5506263", "7510103");
        CreateSubject("Quy hoạch đô thị", "5506196", "7510103");
        CreateSubject("Thiết kế công trình chịu động đất và gió bão", "5506197", "7510103");
        CreateSubject("Tin học đồ hoạ kiến trúc", "5506052", "7510103");
        CreateSubject("Vật lý kiến trúc", "5506195", "7510103");

        // ===== MÔN HỌC CHUYÊN NGÀNH CÔNG NGHỆ KỸ THUẬT GIAO THÔNG =====
        CreateSubject("Thủy văn", "5506121", "7510104");
        CreateSubject("Thiết kế cầu bê tông cốt thép", "5506251", "7510104");
        CreateSubject("Đồ án thiết kế cầu BTСT", "5506076", "7510104");
        CreateSubject("Tin học ứng dụng cầu", "5506122", "7510104");
        CreateSubject("Thiết kế hình học đường ô tô", "5506144", "7510104");
        CreateSubject("Đồ án thiết kế hình học đường ô tô", "5506079", "7510104");
        CreateSubject("Tin học ứng dụng đường", "5506123", "7510104");
        CreateSubject("Thiết kế cầu thép", "5506145", "7510104");
        CreateSubject("Thiết kế nền mặt đường", "5506146", "7510104");
        CreateSubject("Thi công đường", "5506147", "7510104");
        CreateSubject("Đồ án thi công đường", "5506148", "7510104");
        CreateSubject("Thi công cầu", "5506101", "7510104");
        CreateSubject("Đồ án thi công cầu", "5506149", "7510104");
        CreateSubject("Tổ chức và Quản lí Thi công", "5506129", "7510104");
        CreateSubject("Khai thác và thí nghiệm đường", "5506090", "7510104");
        CreateSubject("Khai thác và kiểm định cầu", "5506150", "7510104");
        CreateSubject("Giao thông đô thị và thiết kế đường phố", "5506151", "7510104");
        CreateSubject("Chuyên đề ứng dụng BIM trong xây dựng", "5506152", "7510104");
        CreateSubject("Dự toán công trình xây dựng", "5506153", "7510104");
        CreateSubject("Thực tập kỹ thuật XС", "5506253", "7510104");
        CreateSubject("Thực tập nhận thức XC", "5506252", "7510104");
        CreateSubject("Học kỳ doanh nghiệp XC", "5506154", "7510104");
        CreateSubject("Thí nghiệm hiện trường công trình đường", "5506262", "7510104");
        CreateSubject("Phân tích kết cấu công trình cầu", "5506155", "7510104");
        CreateSubject("Thiết kế và thi công cống trên đường ô tô", "5506156", "7510104");
        CreateSubject("Mố trụ cầu", "5506157", "7510104");
        CreateSubject("Thiết kế và thi công cầu nhịp lớn", "5506158", "7510104");
        CreateSubject("Đồ án Thiết kế và thi công cầu nhịp lớn", "5506159", "7510104");
        CreateSubject("Thực tập khảo sát và thiết kế đường", "5506160", "7510104");
        CreateSubject("Công trình đường ô tô trong vùng điều kiện địa chất đặc biệt", "5506161", "7510104");
        CreateSubject("Đồ án Công trình đường ô tô trong vùng điều kiện địa chất đặc biệt", "5506162", "7510104");
        CreateSubject("Quản lý dự án công trình giao thông", "5506163", "7510104");
        CreateSubject("Chuyên đề Thiết kế và thi công cọc khoan nhồi", "5506164", "7510104");
        CreateSubject("Ngoại ngữ chuyên ngành nâng cao", "5506165", "7510104");
        CreateSubject("Đồ án tốt nghiệp kỹ sư XC", "5506166", "7510104");
        CreateSubject("Quản lý giao thông đô thị bền vững", "5502010", "7510104");
        CreateSubject("Môi trường và phát triển bền vững", "5506168", "7510104");
        CreateSubject("Sức khỏe công trình", "5506169", "7510104");

        // ===== MÔN HỌC CHUYÊN NGÀNH KỸ THUẬT NHIỆT =====
        CreateSubject("Kỹ thuật điện", "5505037", "7510206");
        CreateSubject("Nhiệt động học kỹ thuật", "5504132", "7510206");
        CreateSubject("Thủy khí & Máy thủy khí", "5504171", "7510206");
        CreateSubject("Truyền nhiệt", "5504142", "7510206");
        CreateSubject("Thiết bị trao đổi nhiệt", "5504135", "7510206");
        CreateSubject("Tiếng Anh chuyên ngành Nhiệt", "5504136", "7510206");
        CreateSubject("Nhập môn ngành Nhiệt lạnh", "5504166", "7510206");
        CreateSubject("Kỹ thuật lạnh cơ sở", "5504124", "7510206");
        CreateSubject("Kỹ thuật lạnh ứng dụng", "5504125", "7510206");
        CreateSubject("Lò hơi", "5504128", "7510206");
        CreateSubject("Công nghệ sây", "5504174", "7510206");
        CreateSubject("Kỹ thuật an toàn", "5504121", "7510206");
        CreateSubject("Nhà máy nhiệt điện", "5504131", "7510206");
        CreateSubject("Điều hòa không khí", "5504102", "7510206");
        CreateSubject("Kỹ thuật vận hành thiết bị áp lực", "5504127", "7510206");
        CreateSubject("Đo lường nhiệt", "5504172", "7510206");
        CreateSubject("Tiết kiệm năng lượng", "5504137", "7510206");
        CreateSubject("Năng lượng tái tạo", "5504130", "7510206");
        CreateSubject("Đồ án Lò hơi", "5504107", "7510206");
        CreateSubject("Đồ án Kỹ thuật lạnh", "5504106", "7510206");
        CreateSubject("THCM Gò - Hàn", "5504071", "7510206");
        CreateSubject("THCM Nóng", "5504153", "7510206");
        CreateSubject("THCM Lạnh cơ bản", "5504287", "7510206");
        CreateSubject("THCM Lạnh nâng cao", "5504152", "7510206");
        CreateSubject("THCM Điện lạnh công nghiệp", "5504173", "7510206");
        CreateSubject("THCM Điều hòa không khí", "5504148", "7510206");
        CreateSubject("THCM Điện", "5505229", "7510206");
        CreateSubject("THCM Công nghệ mới NL", "5504289", "7510206");
        CreateSubject("Thí nghiệm Kỹ thuật nhiệt", "5504139", "7510206");
        CreateSubject("HKDN ngành Nhiệt lạnh", "5504176", "7510206");
        CreateSubject("CN làm lạnh bền vững", "5504099", "7510206");
        CreateSubject("Bơm nhiệt ứng dụng", "5504179", "7510206");
        CreateSubject("Lò hơi công nghiệp", "5504182", "7510206");
        CreateSubject("Hệ thống cấp nhiệt lạnh", "5504180", "7510206");
        CreateSubject("Kỹ thuật xử lý khí phát thải", "5504181", "7510206");
        CreateSubject("Quản lý dự án ngành Nhiệt lạnh", "5504184", "7510206");
        CreateSubject("Thực tập tốt nghiệp Nhiệt lạnh", "5504186", "7510206");
        CreateSubject("Đồ án tốt nghiệp Nhiệt lạnh Kỹ sư", "5504188", "7510206");
        CreateSubject("CĐ Điều hòa không khí", "5504089", "7510206");
        CreateSubject("Chuyên đề Sấy", "5504098", "7510206");
        CreateSubject("Chuyên đề Lạnh", "5504099", "7510206");
        CreateSubject("Vật liệu chuyên ngành Nhiệt lạnh", "5504175", "7510206");
        CreateSubject("Thông gió công nghiệp", "5504178", "7510206");
        CreateSubject("Chuyên để ống nhiệt", "5504177", "7510206");
        CreateSubject("Điều hòa trên Ôtô", "5504103", "7510206");
        CreateSubject("Trang bị điện công nghiệp", "5505109", "7510206");






        // ===== MÔN HỌC CHO NHIỀU NGÀNH KỸ THUẬT =====
        var kyThuatMajorCodes = new[] { "7480201", "7510302", "7510303", "7510104" }; // Cơ khí, Điện tử viễn thông, Điều khiển tự động
        CreateSubject("Vật liệu xây dựng", "5506060", kyThuatMajorCodes);

        // ===== MÔN HỌC CHO NGÀNH XÂY DỰNG =====
        CreateSubject("Sức bền vật liệu", "5504040", "7510103", "7510104"); // Xây dựng, Giao thông
        CreateSubject("Cơ học đất", "5506011", "7510103", "7510104");
        CreateSubject("An toàn lao động", "5506001", "7510103", "7510104");
        CreateSubject("Kinh tế xây dựng", "5506035", "7510103", "7510104");
        CreateSubject("Ngoại ngữ chuyên ngành XD ", "5506121", "7510103","7510104");
        CreateSubject("Luật xây dựng", "5506038", "7510103", "7510104");
        CreateSubject("Chuyên đề vật liệu xây dựng", "5506010", "7510103", "7510104");

        // Save subjects first
        await _context.Subjects.AddRangeAsync(subjects);
        await _context.SaveChangesAsync();

        // Then save SubjectMajor relationships
        await _context.Set<SubjectMajor>().AddRangeAsync(subjectMajors);
        await _context.SaveChangesAsync();
    }

    private async Task SeedTypesAsync()
    {
        if (await _context.Types.AnyAsync()) return;

        var adminUser = await _userManager.FindByEmailAsync("admin@ute.edu.vn");
        if (adminUser == null)
            throw new InvalidOperationException("Admin user must be created before seeding types");

        var adminId = adminUser.Id;

        var types = new List<DomainType>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TypeName = "Giáo trình",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TypeName = "Đồ án tốt nghiệp",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TypeName = "Tài liệu",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TypeName = "Bài tập",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TypeName = "Đề thi",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TypeName = "Bài giảng",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TypeName = "Tài liệu tham khảo",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TypeName = "Báo cáo",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TypeName = "Luận văn",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TypeName = "Khóa luận",
                CreatedAt = DateTimeOffset.UtcNow
            }
            ,
            new()
            {
                Id = Guid.NewGuid(),
                TypeName = "Khác",
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        await _context.Types.AddRangeAsync(types);
        await _context.SaveChangesAsync();
    }

    private async Task SeedAuthorsAsync()
    {
        if (await _context.Authors.AnyAsync()) return;

        var adminUser = await _userManager.FindByEmailAsync("admin@ute.edu.vn");
        if (adminUser == null)
            throw new InvalidOperationException("Admin user must be created before seeding authors");

        var adminId = adminUser.Id;

        var authors = new List<Author>
        {
            new() { Id = Guid.NewGuid(), FullName = "TS. Hoàng Thị Mỹ Lệ", Description = "Giảng viên Công nghệ thông tin, ĐH Sư phạm Kỹ thuật Đà Nẵng", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), FullName = "TS. Phạm Tuấn", Description = "Giảng viên Công nghệ thông tin, ĐH Sư phạm Kỹ thuật Đà Nẵng", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), FullName = "ThS. Nguyễn Thị Hà Quyên", Description = "Giảng viên Công nghệ thông tin, ĐH Sư phạm Kỹ thuật Đà Nẵng", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), FullName = "ThS. Nguyễn Bửu Dung", Description = "Giảng viên Công nghệ thông tin, ĐH Sư phạm Kỹ thuật Đà Nẵng", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            // Tác giả Việt Nam - Giáo dục & Công nghệ
            new() { Id = Guid.NewGuid(), FullName = "GS.TS Nguyễn Thanh Thủy", Description = "Chuyên gia Trí tuệ nhân tạo, ĐH Bách Khoa Hà Nội", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), FullName = "PGS.TS Trần Văn Hùng", Description = "Giảng viên Công nghệ thông tin, ĐH Sư phạm Kỹ thuật TP.HCM", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },

            // Tác giả Quốc tế - Công nghệ
            new() { Id = Guid.NewGuid(), FullName = "Robert C. Martin", Description = "Tác giả 'Clean Code', Chuyên gia Software Craftsmanship", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), FullName = "Martin Fowler", Description = "Tác giả 'Refactoring', Chuyên gia Architecture", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), FullName = "Andrew S. Tanenbaum", Description = "Tác giả sách giáo khoa về Mạng máy tính và Hệ điều hành", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), FullName = "Thomas H. Cormen", Description = "Đồng tác giả 'Introduction to Algorithms'", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), FullName = "Eric Evans", Description = "Tác giả 'Domain-Driven Design'", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), FullName = "Gang of Four (GoF)", Description = "Design Patterns: Elements of Reusable Object-Oriented Software", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), FullName = "Andrew Ng", Description = "Chuyên gia AI, Founder Coursera & DeepLearning.AI", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), FullName = "Linus Torvalds", Description = "Người sáng lập Linux và Git", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), FullName = "Joshua Bloch", Description = "Tác giả 'Effective Java'", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), FullName = "Jon Skeet", Description = "Chuyên gia C# và .NET", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },

            // Tác giả Việt Nam - Các ngành kỹ thuật khác
            new() { Id = Guid.NewGuid(), FullName = "GS.TS Nguyễn Đình Đức", Description = "Chuyên gia Cơ học vật liệu và kết cấu", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), FullName = "PGS.TS Trần Minh Tú", Description = "Giảng viên Kỹ thuật Xây dựng", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), FullName = "TS. Lê Quang Vinh", Description = "Chuyên gia Kỹ thuật Nhiệt - Lạnh", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), FullName = "PGS.TS Phạm Văn Chính", Description = "Giảng viên Công nghệ Hóa học", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
            new() { Id = Guid.NewGuid(), FullName = "ThS. Nguyễn Thanh Long", Description = "Chuyên gia Kỹ thuật Ô tô", CreatedById = adminId, CreatedAt = DateTimeOffset.UtcNow, Status = ContentStatus.Approved },
        };

        await _context.Authors.AddRangeAsync(authors);
        await _context.SaveChangesAsync();
    }

}
