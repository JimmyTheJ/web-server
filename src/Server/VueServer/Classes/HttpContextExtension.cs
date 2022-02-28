using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using VueServer.Modules.Core.Models.User;

namespace VueServer.Classes
{
    public static class HttpContextExtension
    {
        public static async Task RefreshLoginAsync(this HttpContext context)
        {
            if (context.User == null)
            {
                return;
            }

            var userManager = context.RequestServices
                .GetRequiredService<UserManager<WSUser>>();
            var signInManager = context.RequestServices
                .GetRequiredService<SignInManager<WSUser>>();

            WSUser user = await userManager.GetUserAsync(context.User);

            if (signInManager.IsSignedIn(context.User))
            {
                await signInManager.RefreshSignInAsync(user);
            }
        }
    }
}
