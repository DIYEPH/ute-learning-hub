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
        await SeedUsersAsync();
        await SeedFacultiesAsync();
        await SeedMajorsAsync();
        // await SeedSubjectsAsync();
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

        var adminUser = (await _userManager.GetUsersInRoleAsync("Admin")).FirstOrDefault();
        var adminId = adminUser?.Id ?? Guid.NewGuid();

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

        var adminUser = (await _userManager.GetUsersInRoleAsync("Admin")).FirstOrDefault();
        var adminId = adminUser?.Id ?? Guid.NewGuid();

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
    private async Task SeedUsersAsync()
    {
        if (await _userManager.Users.AnyAsync()) return;

        var cnsMajor = await _context.Majors.FirstAsync(m => m.MajorCode == "7480201");

        // Admin User
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
                Introduction = "Sinh viên năm 3 CNS",
                AvatarUrl = "https://img.icons8.com/office40/1200/administrator-male.jpg",
                TrustScore = 50,
                TrustLever = TrustLever.Contributor,
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
                FullName = "Lê Thị Học sinh",
                Introduction = "Sinh viên năm 2 CNS",
                AvatarUrl = "https://img.icons8.com/office40/1200/administrator-male.jpg",
                TrustScore = 30,
                TrustLever = TrustLever.Newbie,
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
    // private async Task SeedSubjectsAsync()
    // {
    //     if (await _context.Subjects.AnyAsync()) return;

    //     var spktcnMajor = await _context.Majors.FirstAsync(m => m.MajorCode == "7140214");

    //     var cnttMajor = await _context.Majors.FirstAsync(m => m.MajorCode == "7480201");

    //     var cnktxdMajor = await _context.Majors.FirstAsync(m => m.MajorCode == "7510103");
    //     var cnktgtMajor = await _context.Majors.FirstAsync(m => m.MajorCode == "7510104 ");
    //     var cnktktMajor = await _context.Majors.FirstAsync(m => m.MajorCode == "7510101 ");

    //     var cnktckMajor = await _context.Majors.FirstAsync(m => m.MajorCode == "7510201 ");
    //     var cnktcdtMajor = await _context.Majors.FirstAsync(m => m.MajorCode == "7510203 ");
    //     var cnktotMajor = await _context.Majors.FirstAsync(m => m.MajorCode == "7510205 ");
    //     var cnktnMajor = await _context.Majors.FirstAsync(m => m.MajorCode == "7510206 ");
    //     var cnktddtMajor = await _context.Majors.FirstAsync(m => m.MajorCode == "7510301 ");

    //     var cnktdtvtMajor = await _context.Majors.FirstAsync(m => m.MajorCode == "7510302 ");
    //     var cnktdkvtdhMajor = await _context.Majors.FirstAsync(m => m.MajorCode == "7510303 ");

    //     var ktcshtMajor = await _context.Majors.FirstAsync(m => m.MajorCode == "7580210");
    //     var cnktmtMajor = await _context.Majors.FirstAsync(m => m.MajorCode == "7510406 ");
    //     var kttpMajor = await _context.Majors.FirstAsync(m => m.MajorCode == "7540102 ");
    //     var cnvlMajor = await _context.Majors.FirstAsync(m => m.MajorCode == "7510402 ");
    //     var cnkthhMajor = await _context.Majors.FirstAsync(m => m.MajorCode == "7510401 ");

    //     var adminUser = await _userManager.FindByEmailAsync("admin@ute.edu.vn");

    //     var subjects = new List<Subject>
    //     {
    //         // Sư phạm kỹ thuật công nghiệp
    //         new()
    //         {
    //             Id = Guid.NewGuid(),
    //             SubjectName = "Lập trình Web",
    //             SubjectCode = "LTW",
    //             MajorId = spktcnMajor.Id,
    //             CreatedById = adminUser!.Id,
    //             CreatedAt = DateTimeOffset.UtcNow,
    //             ReviewStatus = ReviewStatus.Approved
    //         },
    //         new()
    //         {
    //             Id = Guid.NewGuid(),
    //             SubjectName = "Cơ sở dữ liệu",
    //             SubjectCode = "CSDL",
    //             MajorId = spktcnMajor.Id,
    //             CreatedById = adminUser.Id,
    //             CreatedAt = DateTimeOffset.UtcNow,
    //             ReviewStatus = ReviewStatus.Approved
    //         },
    //         new()
    //         {
    //             Id = Guid.NewGuid(),
    //             SubjectName = "Phân tích thiết kế hệ thống",
    //             SubjectCode = "PTTKHT",
    //             MajorId = spktcnMajor.Id,
    //             CreatedById = adminUser.Id,
    //             CreatedAt = DateTimeOffset.UtcNow,
    //             ReviewStatus = ReviewStatus.Approved
    //         },
    //         new()
    //         {
    //             Id = Guid.NewGuid(),
    //             SubjectName = "Cấu trúc dữ liệu và giải thuật",
    //             SubjectCode = "CTDL",
    //             MajorId = spktcnMajor.Id,
    //             CreatedById = adminUser.Id,
    //             CreatedAt = DateTimeOffset.UtcNow,
    //             ReviewStatus = ReviewStatus.Approved
    //         }
    //     };

    //     await _context.Subjects.AddRangeAsync(subjects);
    //     await _context.SaveChangesAsync();
    // }

}
