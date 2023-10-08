using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using VueServer.Domain.Interface;
using VueServer.Modules.Core.Models.Account;
using VueServer.Modules.Core.Models.Response;
using VueServer.Modules.Core.Models.User;

namespace VueServer.Modules.Core.Services.Account
{
    public interface IAccountService
    {
        Task<IServerResult<string>> Register(RegisterRequest model);
        Task<IServerResult<bool>> ChangePassword(ChangePasswordRequest model, bool isAdmin);
        Task<IServerResult<LoginResponse>> Login(HttpContext context, LoginRequest model);
        Task<IServerResult> Logout(HttpContext context, string username);

        IServerResult<string> GetCsrfToken(HttpContext context);
        IServerResult<string> ValidateTokenAndGetName(string token);
        Task<IServerResult<TokenValidation>> ValidateToken(string token, IRequestCookieCollection cookies);
        Task<IServerResult<string>> RefreshJwtToken(string token, IRequestCookieCollection cookies);

        Task<IServerResult<IEnumerable<WSUserResponse>>> GetUsers();
        Task<IServerResult<WSUserProfile>> GetUserProfile(string userId);
        Task<IServerResult<IEnumerable<WSUserResponse>>> FuzzyUserSearch(string query);

        Task<IServerResult<string>> UpdateUserAvatar(IFormFile file);
        Task<IServerResult<bool>> UpdateDisplayName(string name);

        Task<IServerResult<IEnumerable<WSGuestLogin>>> GetGuestLogins();
        Task<IServerResult<bool>> UnblockGuestIP(string ip);

        Task<IServerResult<IEnumerable<WSRole>>> GetRoles();
    }
}
