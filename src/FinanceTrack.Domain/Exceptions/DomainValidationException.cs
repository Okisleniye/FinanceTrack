namespace FinanceTrack.Domain.Exceptions;

public class DomainValidationException : DomainException
{
    public string? PropertyName { get; }

    public DomainValidationException(string message, string? propertyName = null) 
        : base(message)
    {
        PropertyName = propertyName;
    }
}
