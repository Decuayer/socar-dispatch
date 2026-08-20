using MediatR;
using SocarDispatch.Domain.Enums;

namespace SocarDispatch.Domain.Events;

public record TeamMemberStatusChangedEvent(
    Guid TeamId,
    Guid UserId,
    TeamMemberStatus PreviousStatus,
    TeamMemberStatus NewStatus,
    Guid ChangedById,
    DateTime ChangedAt
) : INotification;
