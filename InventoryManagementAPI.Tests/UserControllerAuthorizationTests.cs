using InventoryManagementAPI.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace InventoryManagementAPI.Tests;

public class UserControllerAuthorizationTests
{
    [Fact]
    public void UserController_RequiresAuthenticationByDefault()
    {
        var authorizeAttributes = typeof(UserController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true);

        Assert.NotEmpty(authorizeAttributes);
    }

    [Fact]
    public void CreateNewUser_IsExplicitlyAnonymous()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.CreateNewUser));

        Assert.NotNull(method);
        Assert.NotEmpty(method.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true));
    }

    [Theory]
    [InlineData(nameof(UserController.UpdateUserPassword))]
    [InlineData(nameof(UserController.UpdateUserEmail))]
    [InlineData(nameof(UserController.UpdateUserUsername))]
    public void SensitiveProfileEndpoint_DoesNotAllowAnonymousAccess(string methodName)
    {
        var method = typeof(UserController).GetMethod(methodName);

        Assert.NotNull(method);
        Assert.Empty(method.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true));
    }
}
