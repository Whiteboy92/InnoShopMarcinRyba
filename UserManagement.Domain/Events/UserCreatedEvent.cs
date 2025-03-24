using MediatR;

namespace UserManagement.Domain.Events;

public record UserCreatedEvent(Guid UserId, string Email, DateTime CreatedAt) : INotification;