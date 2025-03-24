namespace UserManagement.Domain.Events
{
    public class UserDeletedEvent
    {
        public Guid UserId { get; }

        public UserDeletedEvent(Guid userId)
        {
            UserId = userId;
        }
    }
}