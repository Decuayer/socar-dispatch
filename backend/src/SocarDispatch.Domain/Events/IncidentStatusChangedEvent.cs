using MediatR;
using SocarDispatch.Domain.Enums;


namespace SocarDispatch.Domain.Events;

/// A Domain Event triggered when the Incident status changes (e.g., Assigned -> Resolved, Open -> Canceled).
/// By implementing the MediatR INotification interface, it enables Notification Handlers in the system to listen for it.

public record IncidentStatusChangedEvent(
    Guid IncidentId,
    IncidentStatus PreviousStatus,
    IncidentStatus NewStatus,
    Guid ChangedById,
    DateTime ChangedAt
) : INotification;
