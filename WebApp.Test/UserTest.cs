using WebApp.Infrastructure.Domain.Entities;

namespace WebApp.Test;

public class UserTest
{
    [Fact]
    public void User_ShouldStoreNameCorrectly()
    {
        // Arrange
        var user = new User()
        {
           Name = "Alice",
           Email = "alice@example.com"
        };

        // Act
        var result = user.Name;

        // Assert
        Assert.Equal("Alice", result);
    }
}
