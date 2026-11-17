using FinanceTrack.Domain.Exceptions;
using Xunit;

namespace FinanceTrack.UnitTests.Domain;

public class DomainExceptionTests
{
    [Fact]
    public void DomainValidationException_ShouldStoreMessageAndPropertyName()
    {
        // Act
        var ex = new DomainValidationException("Amount is invalid", "Amount");

        // Assert
        Assert.Equal("Amount is invalid", ex.Message);
        Assert.Equal("Amount", ex.PropertyName);
    }
}
