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
        //await SeedTagsAsync();
        //await SeedDocumentsAsync();

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
        // Check if admin already exists
        var existingAdmin = await _userManager.FindByEmailAsync("admin@ute.edu.vn");
        if (existingAdmin != null) return;

        // Create Admin User (không cần Major)
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
            MajorId = null, // Admin không thuộc ngành nào
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _userManager.CreateAsync(admin, "Admin123!");
        await _userManager.AddToRoleAsync(admin, "Admin");
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
    private async Task SeedSubjectsAsync()
    {
        if (await _context.Subjects.AnyAsync()) return;

        // Load all majors into dictionary for easy lookup
        var majors = await _context.Majors.ToListAsync();
        var majorDict = majors.ToDictionary(m => m.MajorCode, m => m.Id);

        var adminUser = await _userManager.FindByEmailAsync("admin@ute.edu.vn");
        if (adminUser == null) return;

        var subjects = new List<Subject>();
        var subjectMajors = new List<SubjectMajor>();

        // Helper method to create subject with multiple majors
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
        CreateSubject("Toán cao cấp A1", "TOAN1", allMajorCodes);
        CreateSubject("Toán cao cấp A2", "TOAN2", allMajorCodes);
        CreateSubject("Tiếng Anh 1", "ENG1", allMajorCodes);
        CreateSubject("Tiếng Anh 2", "ENG2", allMajorCodes);
        CreateSubject("Giáo dục thể chất 1", "PE1", allMajorCodes);
        CreateSubject("Giáo dục thể chất 2", "PE2", allMajorCodes);
        CreateSubject("Giáo dục quốc phòng", "GDQP", allMajorCodes);

        // ===== MÔN HỌC CHUYÊN NGÀNH CNTT =====
        CreateSubject("Lập trình Web", "LTW", "7480201"); // Công nghệ thông tin
        CreateSubject("Cơ sở dữ liệu", "CSDL", "7480201");
        CreateSubject("Cấu trúc dữ liệu và giải thuật", "CTDL", "7480201");
        CreateSubject("Lập trình hướng đối tượng", "LTHDT", "7480201");
        CreateSubject("Mạng máy tính", "MMT", "7480201");

        // ===== MÔN HỌC CHO NHIỀU NGÀNH KỸ THUẬT =====
        var kyThuatMajorCodes = new[] { "7480201", "7510201", "7510302", "7510303" }; // CNTT, Cơ khí, Điện tử viễn thông, Điều khiển tự động
        CreateSubject("Vật lý đại cương", "VLDC", kyThuatMajorCodes);
        CreateSubject("Toán kỹ thuật", "TOANKT", kyThuatMajorCodes);

        // ===== MÔN HỌC CHO NGÀNH XÂY DỰNG =====
        CreateSubject("Sức bền vật liệu", "SBVL", "7510103", "7510104"); // Xây dựng, Giao thông
        CreateSubject("Cơ học đất", "CHD", "7510103");

        // ===== MÔN HỌC CHO NGÀNH CƠ KHÍ =====
        CreateSubject("Cơ học kỹ thuật", "CHKT", "7510201", "7510203", "7510205"); // Cơ khí, Cơ điện tử, Ô tô
        CreateSubject("Nguyên lý máy", "NLM", "7510201");

        // ===== MÔN HỌC CHO NGÀNH HÓA HỌC =====
        CreateSubject("Hóa học đại cương", "HHDC", "7510401", "7510406", "7540102"); // Hóa học, Môi trường, Thực phẩm
        CreateSubject("Hóa phân tích", "HPT", "7510401");

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

}
