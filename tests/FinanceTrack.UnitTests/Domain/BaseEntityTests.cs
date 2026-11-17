using FinanceTrack.Domain.Common;
using Xunit;

namespace FinanceTrack.UnitTests.Domain;

public class TestEntity : BaseEntity
{
}

public class BaseEntityTests
{
    [Fact]
    public void BaseEntity_ShouldInitializeCreatedAtWithUtcNow()
    {
        // Arrange & Act
        var before = DateTime.UtcNow;
        var entity = new TestEntity();
        var after = DateTime.UtcNow;

        // Assert
        Assert.True(entity.CreatedAt >= before && entity.CreatedAt <= after);
        Assert.Null(entity.UpdatedAt);
    }
}
