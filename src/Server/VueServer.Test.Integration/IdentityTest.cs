using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace VueServer.Test.Integration
{
    public static class IdentityTest
    {
        public static Mock<UserManager<TUser>> MockUserManager<TUser>(IUserStore<TUser> store) where TUser : class
        {
            var userManager = new Mock<UserManager<TUser>>(store, null, null, null, null, null, null, null, null);
            userManager.Object.UserValidators.Add(new UserValidator<TUser>());
            userManager.Object.PasswordValidators.Add(new PasswordValidator<TUser>());
            return userManager;
        }

        public static Mock<SignInManager<TUser>> MockSignInManager<TUser>(UserManager<TUser> userManager) where TUser : class
        {
            var signinManager = new Mock<SignInManager<TUser>>(
                userManager,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<TUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<ILogger<SignInManager<TUser>>>().Object,
                new Mock<IAuthenticationSchemeProvider>().Object
            );
            return signinManager;
        }
    }
}
