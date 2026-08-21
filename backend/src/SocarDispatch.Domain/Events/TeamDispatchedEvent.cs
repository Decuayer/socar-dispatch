using MediatR;

namespace SocarDispatch.Domain.Events;

public record TeamDispatchedEvent(
    Guid AssignmentId,
    Guid IncidentId,
    Guid TeamId,
    string TeamName,
    Guid OperatorId,
    DateTime DispatchedAt
) : INotification;
