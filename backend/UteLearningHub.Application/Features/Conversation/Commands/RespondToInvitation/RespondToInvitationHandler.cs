using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Message;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Conversation.Commands.RespondToInvitation;

public class RespondToInvitationHandler : IRequestHandler<RespondToInvitationCommand, bool>
{
    private readonly IConversationRepository _convRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;
    private readonly IConversationSystemMessageService _sysMsg;
    private readonly IVectorMaintenanceService _vectorService;

    public RespondToInvitationHandler(
        IConversationRepository convRepo,
        ICurrentUserService currentUser,
        IDateTimeProvider dateTime,
        IConversationSystemMessageService sysMsg,
        IVectorMaintenanceService vectorService)
    {
        _convRepo = convRepo;
        _currentUser = currentUser;
        _dateTime = dateTime;
        _sysMsg = sysMsg;
        _vectorService = vectorService;
    }

    public async Task<bool> Handle(RespondToInvitationCommand req, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedException("Must be authenticated");

        var userId = _currentUser.UserId ?? throw new UnauthorizedException();

        // Tìm lời mời
        var invite = await _convRepo.GetInvitationByIdAsync(req.InvitationId, ct);
        if (invite == null)
            throw new NotFoundException("Invitation not found");

        if (invite.InvitedUserId != userId)
            throw new ForbiddenException("This invitation is not for you");

        if (invite.Status != ContentStatus.PendingReview)
            throw new BadRequestException("Invitation already responded");

        // Cập nhật trạng thái
        invite.Status = req.Accept ? ContentStatus.Approved : ContentStatus.Hidden;
        invite.RespondedAt = _dateTime.OffsetNow;
        invite.ResponseNote = req.Note;
        invite.UpdatedById = userId;
        invite.UpdatedAt = _dateTime.OffsetNow;

        if (req.Accept)
        {
            // Tự động thêm vào nhóm
            var conv = await _convRepo.GetByIdWithDetailsAsync(invite.ConversationId, false, ct);
            if (conv == null || conv.IsDeleted)
                throw new NotFoundException("Conversation not found");

            var isAlreadyMember = conv.Members.Any(m => m.UserId == userId && !m.IsDeleted);
            if (!isAlreadyMember)
            {
                var newMember = new ConversationMember
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ConversationId = conv.Id,
                    ConversationMemberRoleType = ConversationMemberRoleType.Member,
                    IsMuted = false
                };

                await _convRepo.AddMemberAsync(newMember, ct);
                await _sysMsg.CreateAsync(conv.Id, userId, MessageType.MemberJoined, null, ct);

                // Update user vector (async)
                _ = Task.Run(async () =>
                {
                    try { await _vectorService.UpdateUserVectorAsync(userId, ct); }
                    catch { }
                }, ct);
            }
        }

        await _convRepo.UnitOfWork.SaveChangesAsync(ct);
        return true;
    }
}
