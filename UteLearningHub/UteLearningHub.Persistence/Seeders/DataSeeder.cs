using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Persistence.Identity;

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
        await SeedFacultiesAsync();
        await SeedMajorsAsync();
        await SeedUsersAsync();
        await SeedSubjectsAsync();
        //await SeedDocumentTypesAsync();
        //await SeedTagsAsync();
        //await SeedDocumentsAsync();

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

        var adminId = Guid.NewGuid(); // Temporary admin ID
        var faculties = new List<Faculty>
        {
            new()
            {
                Id = Guid.NewGuid(),
                FacultyName = "Khoa Công nghệ Thông tin",
                FacultyCode = "CNTT",
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                FacultyName = "Khoa Điện - Điện tử",
                FacultyCode = "DDT",
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                FacultyName = "Khoa Cơ khí",
                FacultyCode = "CK",
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                FacultyName = "Khoa Kinh tế",
                FacultyCode = "KT",
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

        var cnttFaculty = await _context.Faculty.FirstAsync(f => f.FacultyCode == "CNTT");
        var ddtFaculty = await _context.Faculty.FirstAsync(f => f.FacultyCode == "DDT");
        var adminId = Guid.NewGuid();

        var majors = new List<Major>
        {
            new()
            {
                Id = Guid.NewGuid(),
                MajorName = "Công nghệ Phần mềm",
                MajorCode = "CNPM",
                FacultyId = cnttFaculty.Id,
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                MajorName = "Hệ thống Thông tin",
                MajorCode = "HTTT",
                FacultyId = cnttFaculty.Id,
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new(){
                Id = Guid.NewGuid(),
                MajorName = "Khoa học Máy tính",
                MajorCode = "KHMT",
                FacultyId = cnttFaculty.Id,
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                MajorName = "Điện tử Viễn thông",
                MajorCode = "DTVT",
                FacultyId = ddtFaculty.Id,
                CreatedById = adminId,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        await _context.Majors.AddRangeAsync(majors);
        await _context.SaveChangesAsync();
    }
    private async Task SeedUsersAsync()
    {
        if (await _userManager.Users.AnyAsync()) return;

        var cnpmMajor = await _context.Majors.FirstAsync(m => m.MajorCode == "CNPM");
        var htttMajor = await _context.Majors.FirstAsync(m => m.MajorCode == "HTTT");

        // Admin User
        var admin = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = "admin",
            Email = "admin@ute.edu.vn",
            EmailConfirmed = true,
            FullName = "Administrator",
            Introduction = "System Administrator",
            AvatarUrl = "https://via.placeholder.com/150",
            TrustScore = 100,
            TrustLever = TrustLever.Master,
            Gender = Gender.Other,
            MajorId = cnpmMajor.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _userManager.CreateAsync(admin, "Admin123!");
        await _userManager.AddToRoleAsync(admin, "Admin");

        // Student Users
        var students = new[]
        {
            new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = "student1",
                Email = "student1@student.ute.edu.vn",
                EmailConfirmed = true,
                FullName = "Trần Văn Sinh viên",
                Introduction = "Sinh viên năm 3 CNPM",
                AvatarUrl = "https://via.placeholder.com/150",
                TrustScore = 50,
                TrustLever = TrustLever.Contributor,
                Gender = Gender.Male,
                MajorId = cnpmMajor.Id,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = "student2",
                Email = "student2@student.ute.edu.vn",
                EmailConfirmed = true,
                FullName = "Lê Thị Học sinh",
                Introduction = "Sinh viên năm 2 HTTT",
                AvatarUrl = "https://via.placeholder.com/150",
                TrustScore = 30,
                TrustLever = TrustLever.Newbie,
                Gender = Gender.Female,
                MajorId = htttMajor.Id,
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

        var cnpmMajor = await _context.Majors.FirstAsync(m => m.MajorCode == "CNPM");
        var htttMajor = await _context.Majors.FirstAsync(m => m.MajorCode == "HTTT");
        var adminUser = await _userManager.FindByEmailAsync("admin@ute.edu.vn");

        var subjects = new List<Subject>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SubjectName = "Lập trình Web",
                SubjectCode = "LTW",
                MajorId = cnpmMajor.Id,
                CreatedById = adminUser!.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                ReviewStatus = ReviewStatus.Approved
            },
            new()
            {
                Id = Guid.NewGuid(),
                SubjectName = "Cơ sở dữ liệu",
                SubjectCode = "CSDL",
                MajorId = cnpmMajor.Id,
                CreatedById = adminUser.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                ReviewStatus = ReviewStatus.Approved
            },
            new()
            {
                Id = Guid.NewGuid(),
                SubjectName = "Phân tích thiết kế hệ thống",
                SubjectCode = "PTTKHT",
                MajorId = htttMajor.Id,
                CreatedById = adminUser.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                ReviewStatus = ReviewStatus.Approved
            },
            new()
            {
                Id = Guid.NewGuid(),
                SubjectName = "Cấu trúc dữ liệu và giải thuật",
                SubjectCode = "CTDL",
                MajorId = cnpmMajor.Id,
                CreatedById = adminUser.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                ReviewStatus = ReviewStatus.Approved
            }
        };

        await _context.Subjects.AddRangeAsync(subjects);
        await _context.SaveChangesAsync();
    }

}
