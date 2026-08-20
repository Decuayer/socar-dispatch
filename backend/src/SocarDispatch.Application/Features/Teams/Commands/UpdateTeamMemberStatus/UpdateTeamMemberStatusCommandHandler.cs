using MediatR;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Teams.DTOs;
using SocarDispatch.Domain.Enums;
using SocarDispatch.Domain.Events;
using SocarDispatch.Domain.Exceptions;

namespace SocarDispatch.Application.Features.Teams.Commands.UpdateTeamMemberStatus;

public class UpdateTeamMemberStatusCommandHandler : IRequestHandler<UpdateTeamMemberStatusCommand, ApiResponse<TeamMemberDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublisher _publisher;

    public UpdateTeamMemberStatusCommandHandler(IApplicationDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<ApiResponse<TeamMemberDto>> Handle(UpdateTeamMemberStatusCommand request, CancellationToken cancellationToken)
    {
        // 1. İsteği yapan kullanıcının doğrulanması
        var requester = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.RequesterId, cancellationToken);
        if (requester == null)
        {
            throw new DomainException("Invalid user session. User not found.");
        }

        // 2. RBAC Yetki Kontrolü: Team rolü yalnızca KENDİ durumunu güncelleyebilir
        if (requester.RoleType == RoleType.Team && request.TargetUserId != request.RequesterId)
        {
            throw new ForbiddenAccessException("Yalnızca kendi durumunu güncelleyebilirsin.");
        }

        // 3. Ekip üyesinin veritabanından getirilmesi
        var member = await _context.TeamMembers
            .Include(tm => tm.User)
            .FirstOrDefaultAsync(tm => tm.TeamId == request.TeamId && tm.UserId == request.TargetUserId, cancellationToken);

        if (member == null)
        {
            throw new EntityNotFoundException("TeamMember", $"TeamId: {request.TeamId}, UserId: {request.TargetUserId}");
        }

        // 4. Durum ve zaman damgası güncellemesi
        var newStatus = Enum.Parse<TeamMemberStatus>(request.Status, true);
        var previousStatus = member.MemberStatus;

        member.MemberStatus = newStatus;
        member.StatusUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // 5. Domain Event fırlatma (SignalR / Canlı Takip Yayını için)
        await _publisher.Publish(new TeamMemberStatusChangedEvent(
            request.TeamId,
            request.TargetUserId,
            previousStatus,
            newStatus,
            request.RequesterId,
            DateTime.UtcNow
        ), cancellationToken);

        // 6. DTO Dönüşü
        var dto = new TeamMemberDto
        {
            UserId = member.UserId,
            FullName = $"{member.User.FirstName} {member.User.LastName}".Trim(),
            Email = member.User.Email,
            Phone = member.User.Phone,
            Department = member.User.Department,
            SubRole = member.User.SubRole,
            MemberStatus = member.MemberStatus.ToString(),
            StatusUpdatedAt = member.StatusUpdatedAt,
            JoinedAt = member.JoinedAt
        };

        return ApiResponse<TeamMemberDto>.SuccessResult(dto, "Team member status updated successfully.");
    }
}
