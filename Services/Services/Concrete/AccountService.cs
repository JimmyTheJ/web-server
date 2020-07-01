using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
using VueServer.Domain.Concrete;
using VueServer.Domain.Helper;
using VueServer.Domain.Interface;
using VueServer.Models.Account;
using VueServer.Models.Context;
using VueServer.Models.Response;
using VueServer.Models.User;
using VueServer.Services.Interface;

namespace VueServer.Services.Concrete
{
    public class AccountService : IAccountService
    {
        /// <summary>  Used to manage the user sign in process, which is all part of the Identity Framework </summary>
        private readonly SignInManager<WSUser> _signInManager;
        /// <summary> Used to manage the roles of the user, either create or check roles </summary>
        private readonly RoleManager<WSRole> _roleManager;
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

        public AccountService (
            UserManager<WSUser> userManager,
            SignInManager<WSUser> signInManager,
            RoleManager<WSRole> roleManager,
            IWSContext context,
            IWebHostEnvironment env,
            ILoggerFactory logger,
            IConfigurationRoot config,
            IAntiforgery forgery,
            IUserService user
        )
        {
            _antiForgery = forgery ?? throw new ArgumentNullException("Anti-Forgery service is null");
            _config = config ?? throw new ArgumentNullException("Configuration is null");
            _env = env ?? throw new ArgumentNullException("Hosting environment is null");
            _roleManager = roleManager ?? throw new ArgumentNullException("Role manager is null");
            _signInManager = signInManager ?? throw new ArgumentNullException("Signin manager is null");
            _user = user ?? throw new ArgumentNullException("User service is null");
            _context = context ?? throw new ArgumentNullException("User context is null");
            _userManager = userManager ?? throw new ArgumentNullException("User manager is null");
            _logger = logger?.CreateLogger<AccountService>() ?? throw new ArgumentNullException("Logger factory is null");
        }

        #region -> Public Functions 

        public async Task<IResult> Register(RegisterRequest model)
        {
            if (_env.IsProduction())
            {
                _logger.LogWarning("[AccountService] Register: User attemped to register a new account from: " + _user.IP + " with credentials: username=" + model.Username + ", password=" + model.Password + ", comfirm_password=" + model.ConfirmPassword + ", role=" + model.Role);
                return new Result<IResult>(null, Domain.Enums.StatusCode.FORBIDDEN);
            }

            // Initialize the roles
            if (!InitRoles())
            {
                _logger.LogError("[AccountService] Register: Failed to initialize the roles");
                return new Result<IResult>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            // Create a new identity user to pass to the registration method
            var userId = model.Username.ToLower();
            var newUser = new WSUser
            {
                Id = userId,
                UserName = model.Username,
                NormalizedUserName = model.Username.ToUpper(),
                DisplayName = model.Username
            };

            // Try and register the new user
            var result = await _userManager.CreateAsync(newUser, model.Password);

            // If it does not succeed then fill error list and redirect back to view
            if (!result.Succeeded)
            {
                _logger.LogInformation("[AccountService] Register: Failed to create user.");
                return new Result<IResult>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            // Succeeded in making the new user apply the role to that user
            var resultRole = await _userManager.AddToRoleAsync(newUser, model.Role);

            if (!resultRole.Succeeded)
            {
                _logger.LogInformation("[AccountService] Register: Failed to add user to the role.");
                return new Result<IResult>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            // Role applied successfully, update user
            var resultUpdate = await _userManager.UpdateAsync(newUser);
            if (!resultUpdate.Succeeded)
            {
                _logger.LogInformation("[AccountService] Register: Failed to update user with new role.");
                return new Result<IResult>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            // Create profile for user
            await CreateUserProfile(userId);

            // Login was a success and now they should log in
            return new Result<IResult>(null, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<LoginResponse>> Login (LoginRequest model)
        {
            // Try and sign in with the username and password
            //var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, true, false);

            // If log in was unsuccessful redirect back to view
            if (!result.Succeeded)
            {
                _logger.LogWarning("[AccountService] Login: Failed login from " + _user.IP + " with credentials: username=" + model.Username + ", password=" + model.Password);
                return new Result<LoginResponse>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }
            _logger.LogInformation("[AccountService] Login: Successful login of " + model.Username + " @ " + _user.IP );

            //set user role to session
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                _logger.LogWarning("[AccountService] Login: User not found");
                return new Result<LoginResponse>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles == null || roles.Count == 0)
            {
                _logger.LogWarning("[AccountService] Login: Roles not found");
                return new Result<LoginResponse>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            DateTime now = DateTime.UtcNow;
            // Claims to register the JWT
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, GenerateRefreshToken()),
                new Claim(JwtRegisteredClaimNames.Iat, now.ToUniversalTime().ToString(), ClaimValueTypes.Integer64),
                new Claim(ClaimTypes.Role, roles[0])
            };
            //ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Token");
            //claimsIdentity.AddClaims(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var token = GenerateJwtToken(claims);
            var refreshToken = GenerateRefreshToken();

            await InvalidateAllRefreshTokensForUser(user.Id, _user.IP);
            await SaveRefreshToken(user.Id, refreshToken);

            // Try to get user profile. If we don't find one, that is okay it won't have any real impact on the app
            user.UserProfile = await _context.UserProfile.Where(x => x.UserId == user.Id).SingleOrDefaultAsync();

            var resp = new LoginResponse(token, refreshToken, new WSUserResponse(user), roles);
            return new Result<LoginResponse>(resp, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult> Logout (HttpContext context)
        {
            // Sign me out
            await _signInManager.SignOutAsync();

            context.Session.Clear();

            return new Result<IResult>(null, Domain.Enums.StatusCode.OK);
        }

        public IResult<string> GetCsrfToken (HttpContext context)
        {
            return new Result<string>(GenerateCsrfToken(context), Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<RefreshTokenResponse>> RefreshJwtToken (RefreshTokenRequest model)
        {
            var principal = GetPrincipalFromExpiredToken(model.Token);
            var username = principal.Claims.Where(x => x.Type == "sub").Select(x => x.Value).FirstOrDefault();

            var user = await _userManager.FindByNameAsync(username);
            var validToken = await CheckRefreshToken(user.Id, model.RefreshToken);
            if (validToken == null)
            {
                return new Result<RefreshTokenResponse>(null, Domain.Enums.StatusCode.UNAUTHORIZED);
            }

            var newJwtToken = GenerateJwtToken(principal.Claims);
            var newRefreshToken = GenerateRefreshToken();

            await InvalidateRefreshToken(validToken);
            await SaveRefreshToken(user.Id, newRefreshToken);  // Save token to some data store

            return new Result<RefreshTokenResponse>(new RefreshTokenResponse(newJwtToken, newRefreshToken), Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IEnumerable<WSUser>>> GetUsers ()
        {
            var users = await _context.Users.ToListAsync();

            return new Result<IEnumerable<WSUser>>(users, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IEnumerable<WSUserResponse>>> GetUserIds()
        {
            var users = await _context.Users.ToListAsync();

            var list = new List<WSUserResponse>();
            foreach (var user in users)
            {
                list.Add(new WSUserResponse(user));
            }

            return new Result<IEnumerable<WSUserResponse>>(list, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<string>> UpdateUserAvatar(IFormFile file)
        {
            var user = await _user.GetUserByNameAsync(_user.Name);
            if (user == null)
            {
                _logger.LogWarning($"[AccountService] UpdateUserAvatar: Unable to get user by name with name ({_user.Name})");
                return new Result<string>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            if (file == null)
            {
                _logger.LogInformation($"[AccountService] UpdateUserAvatar: File is null");
                return new Result<string>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var fileType = MimeTypeHelper.GetFileType(file.ContentType);
            if (fileType != Domain.Enums.MimeFileType.Photo)
            {   
                _logger.LogInformation($"[AccountService] UpdateUserAvatar: File {file.FileName} uploaded is not an image");
                return new Result<string>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            var userProfile = await _context.UserProfile.Where(x => x.UserId == user.Id).SingleOrDefaultAsync();
            if (userProfile == null)
            {
                if (await CreateUserProfile(user.Id))
                {
                    userProfile = await _context.UserProfile.Where(x => x.UserId == user.Id).SingleOrDefaultAsync();
                }
                else
                {
                    _logger.LogInformation($"[AccountService] UpdateUserAvatar: Can't create user profile for {user.Id}");
                    return new Result<string>(null, Domain.Enums.StatusCode.SERVER_ERROR);
                }
            }

            var filename = $"{user.Id}-{file.FileName}";
            var path = Path.Combine(_env.WebRootPath, "public");

            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch
                {
                    _logger.LogError($"[AccountService] UpdateUserAvatar: Cannot create directory {path}");
                    return new Result<string>(null, Domain.Enums.StatusCode.SERVER_ERROR);
                }
            }            

            try
            {
                using (FileStream fs = File.Create(Path.Combine(path, filename)))
                {
                    await file.CopyToAsync(fs);
                    fs.Flush();
                    _logger.LogInformation($"[AccountService] UpdateUserAvatar: Upload success {filename}");
                }
            }
            catch
            {
                _logger.LogInformation($"[AccountService] UpdateUserAvatar: Upload FAILED {filename}");
                return new Result<string>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            userProfile.AvatarPath = filename;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogWarning($"[AccountService] CreateUserProfile: Error saving after updating profile user '{user.Id}' avatar");
                return new Result<string>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            return new Result<string>(filename, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<bool>> UpdateDisplayName(string name)
        {
            var user = await _user.GetUserByNameAsync(_user.Name);
            if (user == null)
            {
                _logger.LogWarning($"[AccountService] UpdateDisplayName: Unable to get user by name with name ({_user.Name})");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            var wsUser = await _context.Users.Where(x => x.Id == user.Id).SingleOrDefaultAsync();
            if (wsUser == null)
            {
                _logger.LogWarning($"[AccountService] UpdateDisplayName: User with user id ({user.Id}) doesn't exist. This shouldn't be possible though.");
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
                _logger.LogWarning($"[AccountService] CreateUserProfile: Error saving after updating profile user '{user.Id}' avatar");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            return new Result<bool>(true, Domain.Enums.StatusCode.OK);
        }


        #endregion

        #region -> Private Functions

        /// <summary>
        /// Check the validity of the passed in token based on the user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<WSUserTokens> CheckRefreshToken (string id, string token)
        {
            var foundTok = await _context.UserTokens
                .Where(x => x.UserId == id && x.Token == token && x.Valid && x.Source == _user.IP)
                .FirstOrDefaultAsync();

            if (foundTok == null)
                _logger.LogWarning($"Token: '{token}' from user: '{id}' not found in data store or previously invalidated.");

            return foundTok;
        }

        /// <summary>
        /// Invalidate the passed in token
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<bool> InvalidateAllRefreshTokensForUser (string id, string source = null)
        {
            List<WSUserTokens> tokens = null;
            if (source == null)
                tokens = await _context.UserTokens.Where(x => x.UserId == id).ToListAsync();
            else
                tokens = await _context.UserTokens.Where(x => x.UserId == id && x.Source == source).ToListAsync();

            if (tokens == null || tokens.Count == 0)
            {
                _logger.LogInformation($"User: '{id}' has no refresh tokens in the data store");
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
                    _logger.LogWarning($"Error saving after invalidating all of user: '{id}' refresh tokens");
                else
                    _logger.LogWarning($"Error saving after invalidating all of user: '{id}' refresh tokens from the current source: {source}");
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
        private async Task<bool> InvalidateRefreshToken (WSUserTokens token)
        {
            token.Valid = false;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogWarning($"Error saving after invalidating the user: '{token.UserId}' token from IP: {token.Source}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Save the newly created and validated refresh token to the data store
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<bool> SaveRefreshToken (string id, string token)
        {
            var now = DateTime.UtcNow;
            var ip = _user.IP;
            var tokenIP = await _context.UserTokens.Where(x => x.Source == ip && x.UserId == id).FirstOrDefaultAsync();
            if (tokenIP == null)
            {
                var newTok = new WSUserTokens
                {
                    Token = token,
                    UserId = id,
                    Valid = true,
                    Source = _user.IP
                };
                _context.UserTokens.Add(newTok);
            }
            else
            {
                tokenIP.Issued = now;
                tokenIP.Token = token;
                tokenIP.Valid = true;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogWarning($"Error saving after creating the refresh token: '{token}' token from IP: {ip}");
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

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        /// <summary>
        /// Generate a Jwt Token to authenticate the client
        /// </summary>
        /// <param name="username"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private string GenerateJwtToken (IEnumerable<Claim> claims)
        {
            var token = new JwtSecurityToken
            (
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(30),
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
            if (tokens != null && !string.IsNullOrWhiteSpace(tokens.RequestToken) )
                return tokens.RequestToken;

            return Guid.Empty.ToString();
        }

        /// <summary>
        /// Secure way to build a random 256 bit key
        /// </summary>
        /// <returns></returns>
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create()){
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        /// <summary>
        /// TODO: Convert to dotnet core 2.1 seeding [HasData]
        /// TODO: Don't use the default implementation of Identity Framework. Do it all "in-house"
        /// Initialize Identity Framework roles
        /// </summary>
        /// <returns></returns>
        private bool InitRoles()
        {
            // Create a general user role if it does not exist
            if (!_roleManager.RoleExistsAsync("User").Result)
            {
                // Create new Role
                WSRole role = new WSRole()
                {
                    Name = "User",
                    NormalizedName = "User"
                };

                // Create role in data store
                var result = _roleManager.CreateAsync(role).Result;

                // If does not succeed creating role
                if (!result.Succeeded)
                {
                    _logger.LogInformation("[AccountService] InitRoles: Error creating general user role.");
                    return false;
                }
            }

            // Create an elevated user role if it does not exist
            if (!_roleManager.RoleExistsAsync("Elevated").Result)
            {
                // Create new Role
                WSRole role = new WSRole()
                {
                    Name = "Elevated",
                    NormalizedName = "Elevated"
                };

                // Create role in data store
                var result = _roleManager.CreateAsync(role).Result;

                // If does not succeed creating role
                if (!result.Succeeded)
                {
                    _logger.LogInformation("[AccountService] InitRoles: Error creating elevated user role.");
                    return false;
                }
            }

            // Create a admin user role if it does not exist
            if (!_roleManager.RoleExistsAsync("Administrator").Result)
            {
                // Create new Role
                WSRole role = new WSRole()
                {
                    Name = "Administrator",
                    NormalizedName = "Administrator"
                };

                // Create role in data store
                var result = _roleManager.CreateAsync(role).Result;

                // If does not succeed creating role
                if (!result.Succeeded)
                {
                    _logger.LogInformation("[AccountService] InitRoles: Error creating admin user role.");
                    return false;
                }
            }

            // If it makes it this far it was a success
            return true;
        }

        private async Task<bool> CreateUserProfile(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogInformation($"[AccountService] CreateUserProfile: User id is null or empty");
                return false;
            }

            WSUserProfile profile = new WSUserProfile()
            {
                UserId = userId
            };

            _context.UserProfile.Add(profile);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogWarning($"[AccountService] CreateUserProfile: Error saving after creating a profile for user '{userId}'");
                return false;
            }

            return true;
        }

        #endregion
    }
}
