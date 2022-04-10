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
        Task<IResult<string>> Register(RegisterRequest model);
        Task<IResult<bool>> ChangePassword(ChangePasswordRequest model, bool isAdmin);
        Task<IResult<LoginResponse>> Login(HttpContext context, LoginRequest model);
        Task<IResult> Logout(HttpContext context, string username);

        IResult<string> GetCsrfToken(HttpContext context);
        IResult<string> ValidateTokenAndGetName(string token);
        Task<IResult<TokenValidation>> ValidateToken(string token, IRequestCookieCollection cookies);
        Task<IResult<string>> RefreshJwtToken(string token, IRequestCookieCollection cookies);

        Task<IResult<IEnumerable<WSUserResponse>>> GetUsers();
        Task<IResult<WSUserProfile>> GetUserProfile(string userId);
        Task<IResult<IEnumerable<WSUserResponse>>> FuzzyUserSearch(string query);

        Task<IResult<string>> UpdateUserAvatar(IFormFile file);
        Task<IResult<bool>> UpdateDisplayName(string name);

        Task<IResult<IEnumerable<WSGuestLogin>>> GetGuestLogins();
        Task<IResult<bool>> UnblockGuestIP(string ip);

        Task<IResult<IEnumerable<WSRole>>> GetRoles();
    }
}
