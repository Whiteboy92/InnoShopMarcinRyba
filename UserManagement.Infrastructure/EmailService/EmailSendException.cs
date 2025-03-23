namespace UserManagement.Infrastructure.EmailService;

public class EmailSendException : Exception
{
    public EmailSendException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}