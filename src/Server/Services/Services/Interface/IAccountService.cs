using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

using VueServer.Domain.Interface;
using VueServer.Models;
using VueServer.Models.Account;
using VueServer.Models.Directory;
using VueServer.Models.Response;
using VueServer.Models.User;

namespace VueServer.Services.Interface
{
    public interface IAccountService
    {
        Task<IResult> Register(RegisterRequest model);
        Task<IResult<bool>> ChangePassword(ChangePasswordRequest model, bool isAdmin);
        Task<IResult<LoginResponse>> Login(HttpContext context, LoginRequest model);
        Task<IResult> Logout(HttpContext context, string username);

        IResult<string> GetCsrfToken(HttpContext context);
        IResult<string> ValidateTokenAndGetName(string token);
        Task<IResult<string>> RefreshJwtToken(RefreshTokenRequest token);

        Task<IResult<IEnumerable<WSUser>>> GetUsers();
        Task<IResult<IEnumerable<WSUserResponse>>> GetAllOtherUsers();

        Task<IResult<string>> UpdateUserAvatar(IFormFile file);
        Task<IResult<bool>> UpdateDisplayName(string name);

        Task<IResult<IEnumerable<WSGuestLogin>>> GetGuestLogins();
        Task<IResult<bool>> UnblockGuestIP(string ip);


        Task<IResult<IEnumerable<ServerSettings>>> GetDirectorySettings();
        Task<IResult<bool>> SetUseDefaultUserDirectoryPath(bool should);
        Task<IResult<bool>> SetPathDefaultUserDirectoryPath(string path);
        Task<IResult<IEnumerable<ServerGroupDirectory>>> GetGroupDirectories();
        Task<IResult<IEnumerable<ServerUserDirectory>>> GetUserDirectories();
        Task<IResult<bool>> AddGroupDirectory(ServerGroupDirectory dir);
        Task<IResult<bool>> AddUserDirectory(ServerUserDirectory dir);
        Task<IResult<bool>> DeleteGroupDirectory(int id);
        Task<IResult<bool>> DeleteUserDirectory(long id);
        Task<IResult<IEnumerable<string>>> GetRoles();
    }
}
