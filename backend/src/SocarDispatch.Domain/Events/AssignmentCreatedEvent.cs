using MediatR;

namespace SocarDispatch.Domain.Events;

public record AssignmentCreatedEvent(
    Guid AssignmentId,
    Guid IncidentId,
    Guid TeamId,
    Guid OperatorId,
    DateTime AssignedAt
) : INotification;
