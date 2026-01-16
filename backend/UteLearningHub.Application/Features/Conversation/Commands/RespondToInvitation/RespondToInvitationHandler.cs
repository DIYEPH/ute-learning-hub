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

public class RespondToInvitationHandler(
    IConversationRepository convRepo,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTime,
    IConversationSystemMessageService sysMsg,
    IVectorMaintenanceService vectorService) : IRequestHandler<RespondToInvitationCommand, bool>
{
    public async Task<bool> Handle(RespondToInvitationCommand req, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
            throw new UnauthorizedException("Must be authenticated");

        var userId = currentUser.UserId ?? throw new UnauthorizedException();

        var invite = await convRepo.GetInvitationByIdAsync(req.InvitationId, ct);
        if (invite == null)
            throw new NotFoundException("Invitation not found");

        if (invite.InvitedUserId != userId)
            throw new ForbiddenException("This invitation is not for you");

        if (invite.Status != ContentStatus.PendingReview)
            throw new BadRequestException("Invitation already responded");

        // Cập nhật status
        invite.Status = req.Accept ? ContentStatus.Approved : ContentStatus.Hidden;
        invite.RespondedAt = dateTime.OffsetNow;
        invite.ResponseNote = req.Note;
        invite.UpdatedById = userId;
        invite.UpdatedAt = dateTime.OffsetNow;

        if (req.Accept)
        {
            var conv = await convRepo.GetByIdWithDetailsAsync(invite.ConversationId, false, ct);
            if (conv == null)
                throw new NotFoundException("Conversation not found");

            // Thêm member nếu chưa có
            if (!conv.Members.Any(m => m.UserId == userId && !m.IsDeleted))
            {
                var newMember = new ConversationMember
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ConversationId = conv.Id,
                    ConversationMemberRoleType = ConversationMemberRoleType.Member,
                    IsMuted = false,
                    CreatedAt = dateTime.OffsetNow
                };

                await convRepo.AddMemberAsync(newMember, ct);
                await sysMsg.CreateAsync(conv.Id, userId, MessageType.MemberJoined, null, ct);

                // Update vector background
                _ = Task.Run(async () =>
                {
                    try { await vectorService.UpdateUserVectorAsync(userId, ct); }
                    catch { }
                }, ct);
            }
        }

        await convRepo.UnitOfWork.SaveChangesAsync(ct);
        return true;
    }
}