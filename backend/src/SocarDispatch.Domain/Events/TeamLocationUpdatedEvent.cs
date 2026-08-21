using MediatR;

namespace SocarDispatch.Domain.Events;

public record TeamLocationUpdatedEvent(
    Guid TeamId,
    double Latitude,
    double Longitude,
    DateTime UpdatedAt
) : INotification;
