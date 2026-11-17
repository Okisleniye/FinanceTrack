namespace FinanceTrack.Application.Common.Exceptions;

public class ApplicationValidationException : Exception
{
    public string? PropertyName { get; }

    public ApplicationValidationException(string message, string? propertyName = null)
        : base(message)
    {
        PropertyName = propertyName;
    }
}
