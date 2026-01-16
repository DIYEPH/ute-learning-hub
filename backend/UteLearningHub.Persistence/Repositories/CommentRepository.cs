using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence.Repositories.Common;

namespace UteLearningHub.Persistence.Repositories;

public class CommentRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
    : Repository<Comment, Guid>(dbContext, dateTimeProvider), ICommentRepository;
