using WebApp.Infrastructure.Domain.Entities;

namespace WebApp.Test;

public class UserTest
{
    [Fact]
    public void User_ShouldStoreNameCorrectly()
    {
        // Arrange
        var user = new UserProfile()
        {
            UserId = Guid.NewGuid().ToString(),
            FirstName = "Alice",
            LastName = "Smith"
        };

        // Act
        var result = user.FirstName;

        // Assert
        Assert.Equal("Alice", result);
    }
}
