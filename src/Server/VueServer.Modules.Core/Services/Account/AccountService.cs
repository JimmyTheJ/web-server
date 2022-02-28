using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VueServer.Core;
using VueServer.Core.Helper;
using VueServer.Core.Objects;
using VueServer.Domain;
using VueServer.Domain.Interface;
using VueServer.Modules.Core.Cache;
using VueServer.Modules.Core.Context;
using VueServer.Modules.Core.Models.Account;
using VueServer.Modules.Core.Models.Response;
using VueServer.Modules.Core.Models.User;
using VueServer.Modules.Core.Services.User;

namespace VueServer.Modules.Core.Services.Account
{
    public class AccountService : IAccountService
    {
        /// <summary>Number of failed login attempts allowed before the requestor's IP is blocked</summary>
        private const int MAX_FAILED_LOGINS = 6;

        /// <summary>Duration in minutes between JWT refresh requests necessary from Client</summary>
        private const double JWT_REFRESH_TIME = 30;

        /// <summary>Duration in days that a client secret is valid on the Server</summary>
        private const double REFRESH_EXPIRE_TIME = 30;

        /// <summary>  Used to manage the user sign in process, which is all part of the Identity Framework </summary>
        private readonly SignInManager<WSUser> _signInManager;
        /// <summary>Hosting environment</summary>
        private readonly IWebHostEnvironment _env;
        /// <summary> Configuration file. </summary>
        private readonly IConfiguration _config;
        /// <summary> AntiForgery service </summary>
        private readonly IAntiforgery _antiForgery;
        /// <summary>Logger</summary>
        private readonly ILogger _logger;
        /// <summary>User Manager</summary>
        private readonly UserManager<WSUser> _userManager;
        /// <summary>User Context (Database)</summary>
        private readonly IWSContext _context;
        /// <summary>User service to manipulate the context using the user manager</summary>
        private readonly IUserService _user;
        /// <summary>Custom Caching service</summary>
        private readonly IVueServerCache _serverCache;

        public AccountService(
            UserManager<WSUser> userManager,
            SignInManager<WSUser> signInManager,
            IWSContext context,
            IWebHostEnvironment env,
            ILoggerFactory logger,
            IConfigurationRoot config,
            IAntiforgery forgery,
            IUserService user,
            IVueServerCache serverCache
        )
        {
            _antiForgery = forgery ?? throw new ArgumentNullException("Anti-Forgery service is null");
            _config = config ?? throw new ArgumentNullException("Configuration is null");
            _env = env ?? throw new ArgumentNullException("Hosting environment is null");
            _signInManager = signInManager ?? throw new ArgumentNullException("Signin manager is null");
            _user = user ?? throw new ArgumentNullException("User service is null");
            _context = context ?? throw new ArgumentNullException("User context is null");
            _userManager = userManager ?? throw new ArgumentNullException("User manager is null");
            _serverCache = serverCache ?? throw new ArgumentNullException("Server cache is null");
            _logger = logger?.CreateLogger<AccountService>() ?? throw new ArgumentNullException("Logger factory is null");
        }

        #region -> Public Functions 

        public async Task<IResult> Register(RegisterRequest model)
        {
            // Create a new identity user to pass to the registration method
            var newUser = new WSUser
            {
                Id = model.Username.ToLower(),
                NormalizedUserName = model.Username.ToUpper(),
                DisplayName = model.Username,
                Active = true,
                PasswordExpired = false
            };

            // Try and register the new user
            var result = await _userManager.CreateAsync(newUser, model.Password);

            // If it does not succeed then fill error list and redirect back to view
            if (!result.Succeeded)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(Register)}: Failed to create user.");
                return new Result<IResult>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            // Succeeded in making the new user apply the role to that user
            var resultRole = await _userManager.AddToRoleAsync(newUser, model.Role);

            if (!resultRole.Succeeded)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(Register)}: Failed to add user to the role.");
                return new Result<IResult>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            // Role applied successfully, update user
            var resultUpdate = await _userManager.UpdateAsync(newUser);
            if (!resultUpdate.Succeeded)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(Register)}: Failed to update user with new role.");
                return new Result<IResult>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            // Create profile for user
            await CreateUserProfile(newUser.Id);

            // Create default folder if setup
            await CreateDefaultFolder(newUser);

            // Creation of user was successful
            return new Result<IResult>(null, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<bool>> ChangePassword(ChangePasswordRequest model, bool isAdmin)
        {
            if (!isAdmin)
            {
                return await ChangeUserPassword(model);
            }
            else
            {
                return await ChangeAdminPassword(model);
            }
        }

        public async Task<IResult<LoginResponse>> Login(HttpContext context, LoginRequest model)
        {
            // Cached security check for password sniffing bots
            if (_serverCache.GetSubDictionaryValue(CacheMap.BlockedIP, _user.IP, out bool isBlocked) && isBlocked)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(Login)}: Login attempt from {_user.IP}, which is a blocked IP address");
                return new Result<LoginResponse>(null, Domain.Enums.StatusCode.FORBIDDEN);
            }

            // If server has been reset, we wouldn't have a cached value yet in the BlockedIP list
            var guestLogin = _context.GuestLogin.Where(x => x.IPAddress == _user.IP).FirstOrDefault();
            if (guestLogin != null && guestLogin.Blocked)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(Login)}: Login attempt from {_user.IP}, which is a blocked IP address");
                _serverCache.SetSubDictionaryValue(CacheMap.BlockedIP, _user.IP, true);
                return new Result<LoginResponse>(null, Domain.Enums.StatusCode.FORBIDDEN);
            }

            // Get WSUser object
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(Login)}: User not found");

                await Signout(context, model.Username);
                return new Result<LoginResponse>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            // See if sign in credentials are valid with the username and password passed in
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            // If login was unsuccessful redirect back to view
            if (!result.Succeeded)
            {
                // TODO: We probably shouldn't log the plaintext password here
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(Login)}: Failed login from {_user.IP} with credentials: username={model.Username}, password={model.Password}");

                await CreateUserLoginLog(model.Username);
                await IncrementGuestLoginFailure(guestLogin);

                await Signout(context, model.Username);
                return new Result<LoginResponse>(null, Domain.Enums.StatusCode.UNAUTHORIZED);
            }
            _logger.LogInformation($"[{this.GetType().Name}] {nameof(Login)}: Successful login of " + model.Username + " @ " + _user.IP);

            //set user role to session

            var roles = await _userManager.GetRolesAsync(user);
            if (roles == null || roles.Count == 0)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(Login)}: Roles not found");

                await Signout(context, model.Username);
                return new Result<LoginResponse>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            DateTime now = DateTime.UtcNow;
            // Claims to register the JWT
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, GenerateBase64RandomNumber()),
                new Claim(JwtRegisteredClaimNames.Iat, now.ToUniversalTime().ToString(), ClaimValueTypes.Integer64),
                new Claim(ClaimTypes.Role, roles[0])
            };
            //ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Token");
            //claimsIdentity.AddClaims(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var token = GenerateJwtToken(claims);

            var resp = new LoginResponse()
            {
                Token = token,
                Roles = roles
            };

            // Ensure user has a ClientId. If not, create one and send it back so a cookie can be created
            if (!context.Request.Cookies.ContainsKey(DomainConstants.Authentication.CLIENT_COOKIE_KEY))
            {
                model.ClientId = Guid.NewGuid().ToString();
                resp.ClientIdCookie = new VueCookie()
                {
                    Key = DomainConstants.Authentication.CLIENT_COOKIE_KEY,
                    Value = model.ClientId,
                    Options = new CookieOptions()
                    {
                        HttpOnly = true,
                        Path = "/",
                        SameSite = SameSiteMode.Lax,
                        Secure = false,
                        Expires = DateTime.Now + TimeSpan.FromDays(365 * 10)
                    }
                };
            }
            else
            {
                model.ClientId = context.Request.Cookies.Where(x => x.Key == DomainConstants.Authentication.CLIENT_COOKIE_KEY).Select(x => x.Value).FirstOrDefault();
            }

            // Generate refresh token
            var sha256 = CalculateSHA256(model.CodeChallenge);
            var salt = GenerateBase64RandomBytes();
            var saltNHash = new byte[sha256.Length + salt.Length];
            sha256.CopyTo(saltNHash, 0);
            salt.CopyTo(saltNHash, sha256.Length);
            var refreshToken = Convert.ToBase64String(saltNHash);

            if (!(await SaveRefreshToken(user.Id, refreshToken, model.ClientId, _user.IP)))
            {
                await Signout(context, model.Username);
                return new Result<LoginResponse>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            // Try to get user profile. If we don't find one, that is okay it won't have any real impact on the app
            var userProfile = await _context.UserProfile.Where(x => x.UserId == user.Id).SingleOrDefaultAsync();
            if (userProfile == null)
            {
                if (await CreateUserProfile(user.Id))
                {
                    user.UserProfile = await _context.UserProfile.Where(x => x.UserId == user.Id).SingleOrDefaultAsync();
                }
            }
            else
            {
                user.UserProfile = userProfile;
            }

            await ResetGuestLoginFailure(guestLogin);
            await CreateUserLoginLog(model.Username, true);

            var userResponse = new WSUserResponse(user);
            if (roles.Contains(DomainConstants.Authentication.ADMINISTRATOR_STRING) && IsAdminFirstTimeLogin(user, model.Password))
            {
                userResponse.ChangePassword = true;
            }
            resp.User = userResponse;

            // Build Secure cookie
            resp.RefreshCookie.Key = DomainConstants.Authentication.REFRESH_TOKEN_COOKIE_KEY;
            resp.RefreshCookie.Value = refreshToken;
            resp.RefreshCookie.Options.Expires = DateTime.Now + TimeSpan.FromDays(REFRESH_EXPIRE_TIME);
            resp.RefreshCookie.Options.HttpOnly = true;
            resp.RefreshCookie.Options.SameSite = SameSiteMode.Strict;
            resp.RefreshCookie.Options.Path = "/";
            resp.RefreshCookie.Options.Secure = true;

            return new Result<LoginResponse>(resp, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult> Logout(HttpContext context, string username)
        {
            await Signout(context, username);

            return new Result<IResult>(null, Domain.Enums.StatusCode.OK);
        }

        public IResult<string> GetCsrfToken(HttpContext context)
        {
            return new Result<string>(GenerateCsrfToken(context), Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IEnumerable<WSGuestLogin>>> GetGuestLogins()
        {
            var guestLogins = await _context.GuestLogin.ToListAsync();

            return new Result<IEnumerable<WSGuestLogin>>(guestLogins, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<bool>> UnblockGuestIP(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(UnblockGuestIP)}: IP is null or empty");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var blockedUser = _context.GuestLogin.Where(x => x.IPAddress == ip).FirstOrDefault();
            if (blockedUser == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(UnblockGuestIP)}: This IP address is not blocked");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            blockedUser.Blocked = false;
            blockedUser.FailedLogins = 0;
            try
            {
                await _context.SaveChangesAsync();
                _serverCache.SetSubDictionaryValue(CacheMap.BlockedIP, ip, false);
                return new Result<bool>(true, Domain.Enums.StatusCode.OK);
            }
            catch (Exception)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(UnblockGuestIP)}: Error saving after unblocking IP address '{_user.IP}'");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }
        }

        public async Task<IResult<TokenValidation>> ValidateToken(string token, IRequestCookieCollection cookies)
        {
            var isJwtValid = ValidateTokenAndGetName(token);
            if (isJwtValid.Obj == null)
            {
                var cookie = cookies.Where(x => x.Key == DomainConstants.Authentication.REFRESH_TOKEN_COOKIE_KEY).Select(x => x.Value).FirstOrDefault();
                if (cookie == null)
                {
                    return new Result<TokenValidation>(TokenValidation.MissingRefreshToken, Domain.Enums.StatusCode.OK);
                }
                else
                {
                    var principal = GetPrincipalFromExpiredToken(token);
                    var username = principal.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Sub).Select(x => x.Value).Single();

                    var clientId = cookies.Where(x => x.Key == DomainConstants.Authentication.CLIENT_COOKIE_KEY).Select(x => x.Value).FirstOrDefault();
                    var refreshToken = await CheckRefreshToken(username, token, clientId);
                    if (refreshToken == null)
                    {
                        return new Result<TokenValidation>(TokenValidation.InvalidRefreshToken, Domain.Enums.StatusCode.OK);
                    }
                    else
                    {
                        return new Result<TokenValidation>(TokenValidation.RequiresNewJwt, Domain.Enums.StatusCode.OK);
                    }
                }
            }
            else
            {
                return new Result<TokenValidation>(TokenValidation.Valid, Domain.Enums.StatusCode.OK);
            }
        }

        public IResult<string> ValidateTokenAndGetName(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, // Might want to validate the audience and issuer
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SigningKey"])),
                ValidateLifetime = true //here we are saying that we do care about the token's expiration date
            };

            try
            {
                var principal = new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }

                return new Result<string>(principal.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Sub).Select(x => x.Value).SingleOrDefault(), Domain.Enums.StatusCode.OK);
            }
            catch (Exception)
            {
                return new Result<string>(null, Domain.Enums.StatusCode.UNAUTHORIZED);
            }
        }

        public async Task<IResult<string>> RefreshJwtToken(string token, IRequestCookieCollection cookies)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(RefreshJwtToken)}: Token is null or empty");
                return new Result<string>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var principal = GetPrincipalFromExpiredToken(token);
            var username = principal.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Sub).Select(x => x.Value).Single();

            var refreshToken = cookies.Where(x => x.Key == DomainConstants.Authentication.REFRESH_TOKEN_COOKIE_KEY).Select(x => x.Value).FirstOrDefault();
            var clientId = cookies.Where(x => x.Key == DomainConstants.Authentication.CLIENT_COOKIE_KEY).Select(x => x.Value).FirstOrDefault();
            var validToken = await CheckRefreshToken(username, refreshToken, clientId);
            if (validToken == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(RefreshJwtToken)}: No valid token");
                return new Result<string>(null, Domain.Enums.StatusCode.UNAUTHORIZED);
            }

            // Update unique value
            var jti = principal.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Jti).Single();
            jti = new Claim(JwtRegisteredClaimNames.Jti, GenerateBase64RandomNumber());

            // Generate new token to return to client
            var newJwtToken = GenerateJwtToken(principal.Claims);

            return new Result<string>(newJwtToken, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IEnumerable<WSUser>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();

            return new Result<IEnumerable<WSUser>>(users, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<WSUserProfile>> GetUserProfile(string userId)
        {
            var userProfile = await _context.UserProfile.Where(x => x.UserId == userId).FirstOrDefaultAsync();
            if (userProfile == null)
            {
                return new Result<WSUserProfile>(null, Domain.Enums.StatusCode.NO_CONTENT);
            }

            return new Result<WSUserProfile>(userProfile, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IDictionary<string, OtherUsersResponse>>> GetAllOtherUsers()
        {
            var users = await _context.Users.Include(x => x.UserProfile).Where(x => x.Id != _user.Id).ToListAsync();

            var dic = new Dictionary<string, OtherUsersResponse>();
            foreach (var usr in users)
            {
                dic[usr.Id] = new OtherUsersResponse() { DisplayName = usr.DisplayName, Avatar = usr.UserProfile?.AvatarPath };
            }

            return new Result<IDictionary<string, OtherUsersResponse>>(dic, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IEnumerable<WSUserResponse>>> FuzzyUserSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new Result<IEnumerable<WSUserResponse>>(new List<WSUserResponse>(), Domain.Enums.StatusCode.NO_CONTENT);
            }

            var users = await _context.Users.Include(x => x.UserProfile)
                .Where(x => x.Id != _user.Id && x.Id.Contains(query))
                .Select(x => new WSUserResponse(x))
                .ToListAsync();

            return new Result<IEnumerable<WSUserResponse>>(users, Domain.Enums.StatusCode.OK);
        }


        public async Task<IResult<string>> UpdateUserAvatar(IFormFile file)
        {
            if (file == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(UpdateUserAvatar)}: File is null");
                return new Result<string>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var fileType = MimeTypeHelper.GetFileType(file.ContentType);
            if (fileType != Domain.Enums.MimeFileType.Photo)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(UpdateUserAvatar)}: File {file.FileName} uploaded is not an image");
                return new Result<string>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            var userProfile = await _context.UserProfile.Where(x => x.UserId == _user.Id).SingleOrDefaultAsync();
            if (userProfile == null)
            {
                if (await CreateUserProfile(_user.Id))
                {
                    userProfile = await _context.UserProfile.Where(x => x.UserId == _user.Id).SingleOrDefaultAsync();
                }
                else
                {
                    _logger.LogInformation($"[{this.GetType().Name}] {nameof(UpdateUserAvatar)}: Can't create user profile for {_user.Id}");
                    return new Result<string>(null, Domain.Enums.StatusCode.SERVER_ERROR);
                }
            }

            var filename = $"{_user.Id}-{file.FileName}";
            var path = Path.Combine(_env.WebRootPath, "public");

            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch
                {
                    _logger.LogError($"[{this.GetType().Name}] {nameof(UpdateUserAvatar)}: Cannot create directory {path}");
                    return new Result<string>(null, Domain.Enums.StatusCode.SERVER_ERROR);
                }
            }

            try
            {
                using (FileStream fs = File.Create(Path.Combine(path, filename)))
                {
                    await file.CopyToAsync(fs);
                    fs.Flush();
                    _logger.LogInformation($"[{this.GetType().Name}] {nameof(UpdateUserAvatar)}: Upload success {filename}");
                }
            }
            catch
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(UpdateUserAvatar)}: Upload FAILED {filename}");
                return new Result<string>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            userProfile.AvatarPath = filename;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(UpdateUserAvatar)}: Error saving after updating profile user '{_user.Id}' avatar");
                return new Result<string>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            return new Result<string>(filename, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<bool>> UpdateDisplayName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(UpdateDisplayName)}: Must provide a new name, null or empty is not valid");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var wsUser = await _context.Users.Where(x => x.Id == _user.Id).SingleOrDefaultAsync();
            if (wsUser == null)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(UpdateDisplayName)}: User with user id ({_user.Id}) doesn't exist. This shouldn't be possible though.");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            // TODO: Add some kind of validation for what is acceptable for a user name
            wsUser.DisplayName = name;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(UpdateDisplayName)}: Error saving after updating profile user '{_user.Id}' avatar");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            return new Result<bool>(true, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IEnumerable<WSRole>>> GetRoles()
        {
            var roles = await _context.Roles.ToListAsync();
            return new Result<IEnumerable<WSRole>>(roles, Domain.Enums.StatusCode.OK);
        }


        #endregion

        #region -> Private Functions

        private bool IsAdminFirstTimeLogin(WSUser user, string pw)
        {
            return user.PasswordExpired && !user.Active && (string.IsNullOrWhiteSpace(pw) || pw == DomainConstants.Authentication.DEFAULT_PASSWORD);
        }

        private async Task<IResult<bool>> ChangeUserPassword(ChangePasswordRequest model)
        {
            var user = await _userManager.FindByIdAsync(_user.Id);
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains(DomainConstants.Authentication.ADMINISTRATOR_STRING))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(ChangeUserPassword)}: Administrators should use the /api/account/admin to change their password");
                return new Result<bool>(false, Domain.Enums.StatusCode.FORBIDDEN);
            }

            // User is not an administrator

            if (string.IsNullOrWhiteSpace(model.OldPassword))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(ChangeUserPassword)}: Request requires an old password");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var passwordChange = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (passwordChange.Succeeded)
            {
                _logger.LogDebug($"[{this.GetType().Name}] {nameof(ChangeUserPassword)}: User password for user '{user.DisplayName}' changed successfully");

                if (user.PasswordExpired)
                {
                    try
                    {
                        _context.Users.Update(user);
                        user.PasswordExpired = false;
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        _logger.LogWarning($"[{this.GetType().Name}] {nameof(ChangeUserPassword)}: Error saving after changing password for user '{_user.IP}'");
                        return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
                    }
                }

                return new Result<bool>(true, Domain.Enums.StatusCode.OK);
            }
            else
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(ChangeUserPassword)}: User password for user '{user.DisplayName}' failed to change password");
                return new Result<bool>(false, Domain.Enums.StatusCode.UNAUTHORIZED);
            }
        }

        private async Task<IResult<bool>> ChangeAdminPassword(ChangePasswordRequest model)
        {
            var user = await _userManager.FindByIdAsync(_user.Id);
            var roles = await _userManager.GetRolesAsync(user);

            if (!roles.Contains(DomainConstants.Authentication.ADMINISTRATOR_STRING))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(ChangeAdminPassword)}: Administrators should use the /api/account/admin to change their password");
                return new Result<bool>(false, Domain.Enums.StatusCode.FORBIDDEN);
            }
            else
            {
                // User is an administrator

                // If user is an administrator, has an expired password, has an innactive account and the model 
                // doesn't contain an old password then this is our first login password change activation
                if (IsAdminFirstTimeLogin(user, model.OldPassword))
                {
                    model.OldPassword = DomainConstants.Authentication.DEFAULT_PASSWORD;
                }
                // Model doesn't have an old password, but we aren't the on a first time login, so the request in invalid
                else if (string.IsNullOrWhiteSpace(model.OldPassword))
                {
                    _logger.LogInformation($"[{this.GetType().Name}] {nameof(ChangeAdminPassword)}: Request requires an old password");
                    return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
                }

                if (_userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, model.OldPassword) != PasswordVerificationResult.Failed)
                {
                    var passwordChange = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                    if (passwordChange.Succeeded)
                    {
                        _logger.LogDebug($"[{this.GetType().Name}] {nameof(ChangeAdminPassword)}: Administrator password for user '{user.DisplayName}' changed successfully");

                        if (user.PasswordExpired)
                        {
                            _context.Users.Update(user);

                            if (IsAdminFirstTimeLogin(user, model.OldPassword))
                            {
                                user.Active = true;
                            }

                            user.PasswordExpired = false;
                            try
                            {
                                await _context.SaveChangesAsync();
                            }
                            catch (Exception)
                            {
                                _logger.LogWarning($"[{this.GetType().Name}] {nameof(ChangeAdminPassword)}: Error saving after changing password for user '{_user.IP}'");
                                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
                            }
                        }

                        return new Result<bool>(true, Domain.Enums.StatusCode.OK);
                    }
                    else
                    {
                        _logger.LogInformation($"[{this.GetType().Name}] {nameof(ChangeAdminPassword)}: Administrator password for user '{user.DisplayName}' failed to change password");
                        return new Result<bool>(false, Domain.Enums.StatusCode.UNAUTHORIZED);
                    }
                }
                else
                {
                    _logger.LogInformation($"[{this.GetType().Name}] {nameof(ChangeAdminPassword)}: Administrator hashed password for user '{user.DisplayName}' doesn't match old password");
                    return new Result<bool>(false, Domain.Enums.StatusCode.UNAUTHORIZED);
                }
            }
        }

        private async Task Signout(HttpContext context, string username)
        {
            var token = await GetRefreshTokenForUser(username, _user.IP);
            if (token != null)
            {
                await InvalidateRefreshToken(token);
            }

            // Sign me out (This shouldn't be required, as we never really "sign in"
            //await _signInManager.SignOutAsync();
            context.Session.Clear();
        }

        private async Task<WSUserTokens> GetRefreshTokenForUser(string id, string ip)
        {
            return await _context.UserTokens.Where(x => x.UserId == id && x.IPAddress == ip && x.Valid).SingleOrDefaultAsync();
        }

        /// <summary>
        /// Check the validity of the passed in token based on the user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<WSUserTokens> CheckRefreshToken(string id, string token, string clientId, string ip = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogDebug($"[{this.GetType().Name}] {nameof(CheckRefreshToken)}: No user id present");
                return null;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(CheckRefreshToken)}: User ({id}) passed a null token. This is probably a client code bug of some sort.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(CheckRefreshToken)}: User ({id}) has a potentially valid token, but no associated clientId");
                return null;
            }

            var returnedToken = _context.UserTokens.Where(x => x.UserId == id && x.ClientId == clientId).FirstOrDefault();
            if (returnedToken == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(CheckRefreshToken)}: User ({id}) does not have any refresh tokens in the data store for this client Id ({clientId})");
                return null;
            }

            if (returnedToken.Token != token)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(CheckRefreshToken)}: User ({id}) does not have a refresh token that matches the passed in token value in the data store. This could potentially mean someone is trying to impersonate the user. Invalidating all refresh tokens for user.");
                return null;
            }

            if (!returnedToken.Valid)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(CheckRefreshToken)}: User ({id}) does have a refresh token that matches the passed in token value in the data store, but it has been previously invalidated. This could potentially mean someone has gained access to a user's device and is trying to pass iligitimate information to the server.");
                return null;
            }

            if (IsRefreshTokenExpired(returnedToken.Issued))
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(CheckRefreshToken)}: User ({id}) does have a refresh token that matches the passed in token value in the data store, but it has expired.");
                return null;
            }

            return returnedToken;
        }

        private bool IsRefreshTokenExpired(DateTime value)
        {
            return DateTime.UtcNow > value.AddMonths(1);
        }

        /// <summary>
        /// Invalidate the passed in token
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<bool> InvalidateAllRefreshTokensForUser(string id, string source = null)
        {
            List<WSUserTokens> tokens = null;
            if (source == null)
            {
                tokens = await _context.UserTokens.Where(x => x.UserId == id).ToListAsync();
            }
            else
            {
                tokens = await _context.UserTokens.Where(x => x.UserId == id && x.IPAddress == source).ToListAsync();
            }

            if (tokens == null || tokens.Count == 0)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(InvalidateAllRefreshTokensForUser)}: User '{id}' has no refresh tokens in the data store");
                return false;
            }

            foreach (var token in tokens)
            {
                token.Valid = false;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                if (source == null)
                {
                    _logger.LogWarning($"[{this.GetType().Name}] {nameof(InvalidateAllRefreshTokensForUser)}: Error saving after invalidating all of user: '{id}' refresh tokens");
                }
                else
                {
                    _logger.LogWarning($"[{this.GetType().Name}] {nameof(InvalidateAllRefreshTokensForUser)}: Error saving after invalidating all of user: '{id}' refresh tokens from the current source: {source}");
                }

                return false;
            }
            return true;
        }

        /// <summary>
        /// Invalidate the passed in token
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<bool> InvalidateRefreshToken(WSUserTokens token)
        {
            token.Valid = false;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(InvalidateRefreshToken)}: Error saving after invalidating the user: '{token.UserId}' token from IP: {token.IPAddress}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Save the newly created and validated refresh token to the data store
        /// </summary>
        /// <param name="id"></param>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        private async Task<bool> SaveRefreshToken(string id, string refreshToken, string clientId, string ip)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(SaveRefreshToken)}: Token is null or empty");
                return false;
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(SaveRefreshToken)}: Client Id is null or empty");
                return false;
            }

            // For some reason sometimes localhost comes through as ::1 and sometimes its 127.0.0.1, this normalizes it
            if (ip == "::1")
            {
                ip = "127.0.0.1";
            }

            var now = DateTime.UtcNow;
            var tokenIP = await _context.UserTokens.Where(x => x.UserId == id && x.ClientId == clientId).SingleOrDefaultAsync();
            if (tokenIP == null)
            {
                var newTok = new WSUserTokens
                {
                    Issued = now,
                    Token = refreshToken,
                    UserId = id,
                    Valid = true,
                    IPAddress = ip,
                    ClientId = clientId
                };
                _context.UserTokens.Add(newTok);
            }
            else
            {
                tokenIP.Issued = now;
                tokenIP.Token = refreshToken;
                tokenIP.Valid = true;
                tokenIP.IPAddress = ip;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(SaveRefreshToken)}: Error saving after creating the refresh token: '{refreshToken}' token from IP: {ip}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Reverse engineer a token and get the user principal claims from it
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, // Might want to validate the audience and issuer
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SigningKey"])),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };

            var principal = new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }

        /// <summary>
        /// Generate a Jwt Token to authenticate the client
        /// </summary>
        /// <param name="username"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private string GenerateJwtToken(IEnumerable<Claim> claims)
        {
            var token = new JwtSecurityToken
            (
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(JWT_REFRESH_TIME),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SigningKey"])), SecurityAlgorithms.HmacSha256)
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Build a csrf token from the user's information and context
        /// </summary>
        /// <returns></returns>
        private string GenerateCsrfToken(HttpContext context)
        {
            var tokens = _antiForgery.GetAndStoreTokens(context);
            if (tokens != null && !string.IsNullOrWhiteSpace(tokens.RequestToken))
            {
                return tokens.RequestToken;
            }

            return Guid.Empty.ToString();
        }

        /// <summary>
        /// Secure way to build a random 256 bit key
        /// </summary>
        /// <returns></returns>
        private string GenerateBase64RandomNumber()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private byte[] GenerateBase64RandomBytes()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return randomNumber;
            }
        }

        private byte[] CalculateSHA256(string str)
        {
            SHA256 sha256 = SHA256Managed.Create();
            byte[] hashValue;
            UTF8Encoding objUtf8 = new UTF8Encoding();
            hashValue = sha256.ComputeHash(objUtf8.GetBytes(str));

            return hashValue;
        }


        private async Task<bool> CreateUserProfile(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(CreateUserProfile)}: User id is null or empty");
                return false;
            }

            WSUserProfile profile = new WSUserProfile()
            {
                UserId = userId,
            };

            _context.UserProfile.Add(profile);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(CreateUserProfile)}: Error saving after creating a profile for user '{userId}'");
                return false;
            }

            return true;
        }

        private async Task<bool> CreateDefaultFolder(WSUser user)
        {
            var directorySettings = await _context.ServerSettings.Where(x => x.Key.StartsWith(DomainConstants.ServerSettings.BaseKeys.Directory)).ToListAsync();
            if (directorySettings == null || directorySettings.Count == 0)
            {
                return false;
            }

            var shouldCreate = directorySettings.Where(x => x.Key == DomainConstants.ServerSettings.BaseKeys.Directory + DomainConstants.ServerSettings.Directory.ShouldUseDefaultPath).Select(x => x.Value?.StringToBool() ?? null).FirstOrDefault();
            if (!shouldCreate.HasValue || (shouldCreate.HasValue && shouldCreate.Value == false))
            {
                return false;
            }

            var createPath = directorySettings.Where(x => x.Key == DomainConstants.ServerSettings.BaseKeys.Directory + DomainConstants.ServerSettings.Directory.DefaultPathValue).Select(x => x.Value).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(createPath))
            {
                return false;
            }

            var argError = false;
            string arg = null;

            // TODO: Support multiple parameters
            for (int i = 0; i < createPath.Length; i++)
            {
                if (createPath[i] == '{')
                {
                    int start = i;
                    while (true)
                    {
                        if (createPath[i] == '}')
                        {
                            arg = createPath.Substring(start, i - start + 1).ToLower();
                            break;
                        }

                        if (i == createPath.Length)
                        {
                            argError = true;
                            break;
                        }

                        i++;
                    }
                }

                if (argError)
                {
                    break;
                }
            }

            if (argError)
            {
                return false;
            }

            if (arg != null)
            {
                if (arg.Contains(nameof(WSUser.Id).ToLower()))
                {
                    createPath = createPath.Replace(arg, user.Id, StringComparison.OrdinalIgnoreCase);
                }
                else if (arg.Contains(nameof(WSUser.Id).ToLower()))
                {
                    createPath = createPath.Replace(arg, user.Id);
                }
            }

            if (!FolderBuilder.CreateFolder(createPath))
            {
                return false;
            }

            // TODO: Add back in
            //var userDirectory = new ServerUserDirectory()
            //{
            //    Name = "User Files",
            //    Path = createPath,
            //    Default = true,
            //    UserId = user.Id,
            //    AccessFlags = DirectoryAccessFlags.CreateFolder | DirectoryAccessFlags.DeleteFile | DirectoryAccessFlags.DeleteFolder
            //        | DirectoryAccessFlags.ReadFile | DirectoryAccessFlags.ReadFolder | DirectoryAccessFlags.UploadFile
            //};
            //_context.ServerUserDirectory.Add(userDirectory);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(CreateDefaultFolder)}: Error saving when creating default user directory");
                return false;
            }

            return true;
        }

        private async Task<WSUserLogin> CreateUserLoginLog(string username, bool success = false)
        {
            var loginAttempt = new WSUserLogin()
            {
                IPAddress = _user.IP,
                Username = username,
                Timestamp = DateTime.UtcNow.Ticks,
                Success = success
            };

            _context.UserLogin.Add(loginAttempt);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(CreateUserLoginLog)}: Error saving when creating login attempt for account: '{username}' from IP '{_user.IP}' with success status '{success}'");
                return null;
            }

            return loginAttempt;
        }

        private async Task<bool> IncrementGuestLoginFailure(WSGuestLogin guestLogin)
        {
            if (guestLogin == null)
            {
                guestLogin = await CreateGuestLogin();
                if (guestLogin == null)
                {
                    return false;
                }
            }

            guestLogin.FailedLogins++;
            if (!guestLogin.Blocked && guestLogin.FailedLogins > MAX_FAILED_LOGINS)
            {
                guestLogin.Blocked = true;
            }

            try
            {
                await _context.SaveChangesAsync();

                if (guestLogin.Blocked)
                {
                    _serverCache.SetSubDictionaryValue(CacheMap.BlockedIP, _user.IP, true);
                }
                return true;
            }
            catch (Exception)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(IncrementGuestLoginFailure)}: Error saving when updating guest login from IP '{_user.IP}'");
                return false;
            }
        }

        private async Task<WSGuestLogin> CreateGuestLogin()
        {
            var guestLogin = new WSGuestLogin()
            {
                IPAddress = _user.IP
            };

            var result = _context.GuestLogin.Add(guestLogin);
            try
            {
                await _context.SaveChangesAsync();
                return result.Entity;
            }
            catch (Exception)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(CreateGuestLogin)}: Error saving when creating guest login from IP '{_user.IP}'");
                return null;
            }
        }

        private async Task<WSGuestLogin> ResetGuestLoginFailure(WSGuestLogin guestLogin)
        {
            if (guestLogin == null)
            {
                return null;
            }

            guestLogin.FailedLogins = 0;
            guestLogin.Blocked = false;

            try
            {
                await _context.SaveChangesAsync();
                return guestLogin;
            }
            catch (Exception)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(ResetGuestLoginFailure)}: Error saving when resettting guest login from IP '{_user.IP}' to an active state");
                return null;
            }
        }

        #endregion
    }
}
