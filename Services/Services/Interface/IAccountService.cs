using Microsoft.AspNetCore.Http;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using VueServer.Domain.Interface;
using VueServer.Models.Account;
using VueServer.Models.Modules;
using VueServer.Models.Response;
using VueServer.Models.User;

namespace VueServer.Services.Interface
{
    public interface IAccountService
    {
        Task<IResult> Register(RegisterRequest model);

        Task<IResult<LoginResponse>> Login (LoginRequest model);

        Task<IResult> Logout (HttpContext context);

        IResult<string> GetCsrfToken (HttpContext context);

        Task<IResult<string>> RefreshJwtToken (string token);

        Task<IResult<IEnumerable<WSUser>>> GetUsers();

        Task<IResult<IEnumerable<WSUserResponse>>> GetAllOtherUsers();

        Task<IResult<string>> UpdateUserAvatar(IFormFile file);

        Task<IResult<bool>> UpdateDisplayName(string name);
    }
}
