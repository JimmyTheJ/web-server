using Microsoft.AspNetCore.Http;

using System;
using System.Threading.Tasks;

using VueServer.Common.Interface;
using VueServer.Models.Account;
using VueServer.Models.Response;

namespace VueServer.Services.Interface
{
    public interface IAccountService
    {
        Task<IResult> Register(RegisterRequest model);

        Task<IResult<LoginResponse>> Login (LoginRequest model);

        Task<IResult> Logout (HttpContext context);

        IResult<string> GetCsrfToken (HttpContext context);

        Task<IResult<RefreshTokenResponse>> RefreshJwtToken (RefreshTokenRequest model);
    }
}
