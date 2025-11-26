using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence.Identity;
using UteLearningHub.Persistence.Repositories;
using UteLearningHub.Persistence.Seeders;

namespace UteLearningHub.Persistence;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddDbContextPool<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

        services.AddIdentityCore<AppUser>(options => {
            //Password
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;

            //UserSettings
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._+";
            options.User.RequireUniqueEmail = true;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddSignInManager<SignInManager<AppUser>>()
        .AddDefaultTokenProviders();

        //Register Repositories
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IDocumentReviewRepository, DocumentReviewRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IFacultyRepository, FacultyRepository>();
        services.AddScoped<IMajorRepository, MajorRepository>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ITypeRepository, TypeRepository>();
        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        
        services.AddScoped<IProfileVectorStore, ProfileVectorStore>();
        services.AddScoped<IConversationVectorStore, ConversationVectorStore>();

        services.AddScoped<DataSeeder>();

        return services;
    }
}
