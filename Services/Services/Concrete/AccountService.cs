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
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VueServer.Common.Concrete;
using VueServer.Common.Interface;
using VueServer.Models.Account;
using VueServer.Models.Context;
using VueServer.Models.Response;
using VueServer.Services.Interface;

namespace VueServer.Services.Concrete
{
    public class AccountService : IAccountService
    {
        /// <summary>  Used to manage the user sign in process, which is all part of the Identity Framework </summary>
        private readonly SignInManager<ServerIdentity> _signInManager;
        /// <summary> Used to manage the roles of the user, either create or check roles </summary>
        private readonly RoleManager<IdentityRole> _roleManager;
        /// <summary>Hosting environment</summary>
        private readonly IHostingEnvironment _env;
        /// <summary> Configuration file. </summary>
        private readonly IConfiguration _config;
        /// <summary> AntiForgery service </summary>
        private readonly IAntiforgery _antiForgery;
        /// <summary>Logger</summary>
        private readonly ILogger _logger;
        /// <summary>User Manager</summary>
        private readonly UserManager<ServerIdentity> _userManager;
        /// <summary>User Context (Database)</summary>
        private readonly UserContext _userContext;
        /// <summary>User service to manipulate the context using the user manager</summary>
        private readonly IUserService _user;

        public AccountService (
            UserManager<ServerIdentity> userManager,
            SignInManager<ServerIdentity> signInManager,
            RoleManager<IdentityRole> roleManager,
            UserContext userContext,
            IHostingEnvironment env,
            ILoggerFactory logger,
            IConfiguration config,
            IAntiforgery forgery,
            IUserService user
        )
        {
            _antiForgery = forgery;
            _config = config;
            _env = env;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _user = user;
            _userContext = userContext;
            _userManager = userManager;
            _logger = logger.CreateLogger<AccountService>();
        }

        #region -> Public Functions 

        public async Task<IResult> Register(RegisterRequest model)
        {
            if (_env.IsProduction())
            {
                _logger.LogWarning("[AccountService] Register: User attemped to register a new account from: " + _user.IP + " with credentials: username=" + model.Username + ", password=" + model.Password + ", comfirm_password=" + model.ConfirmPassword + ", role=" + model.Role);
                return new Result<IResult>(null, Common.Enums.StatusCode.FORBIDDEN);
            }

            // Initialize the roles
            if (!InitRoles())
            {
                _logger.LogError("[AccountService] Register: Failed to initialize the roles");
                return new Result<IResult>(null, Common.Enums.StatusCode.SERVER_ERROR);
            }
                

            // Create a new identity user to pass to the registration method
            var newUser = new ServerIdentity
            {
                UserName = model.Username // Initialize the username for the identity user
            };

            // Try and register the new user
            var result = await _userManager.CreateAsync(newUser, model.Password);

            // If it does not succeed then fill error list and redirect back to view
            if (!result.Succeeded)
            {
                _logger.LogInformation("[AccountService] Register: Failed to create user.");
                return new Result<IResult>(null, Common.Enums.StatusCode.BAD_REQUEST);
            }

            // Succeeded in making the new user apply the role to that user
            var resultRole = await _userManager.AddToRoleAsync(newUser, model.Role);

            if (!resultRole.Succeeded)
            {
                _logger.LogInformation("[AccountService] Register: Failed to add user to the role.");
                return new Result<IResult>(null, Common.Enums.StatusCode.BAD_REQUEST);
            }

            // Role applied successfully, update user
            var resultUpdate = await _userManager.UpdateAsync(newUser);

            // Login was a success and now they should log in
            return new Result<IResult>(null, Common.Enums.StatusCode.OK);
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
                return new Result<LoginResponse>(null, Common.Enums.StatusCode.BAD_REQUEST);
            }
            _logger.LogInformation("[AccountService] Login: Successful login of " + model.Username + " @ " + _user.IP );

            //set user role to session
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                _logger.LogWarning("[AccountService] Login: User not found");
                return new Result<LoginResponse>(null, Common.Enums.StatusCode.BAD_REQUEST);
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles == null || roles.Count == 0)
            {
                _logger.LogWarning("[AccountService] Login: Roles not found");
                return new Result<LoginResponse>(null, Common.Enums.StatusCode.BAD_REQUEST);
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

            var resp = new LoginResponse(token, refreshToken, user.UserName, roles);

            return new Result<LoginResponse>(resp, Common.Enums.StatusCode.OK);
        }

        public async Task<IResult> Logout (HttpContext context)
        {
            // Sign me out
            await _signInManager.SignOutAsync();

            context.Session.Clear();

            return new Result<IResult>(null, Common.Enums.StatusCode.OK);
        }

        public IResult<string> GetCsrfToken (HttpContext context)
        {
            return new Result<string>(GenerateCsrfToken(context), Common.Enums.StatusCode.OK);
        }

        public async Task<IResult<RefreshTokenResponse>> RefreshJwtToken (RefreshTokenRequest model)
        {
            var principal = GetPrincipalFromExpiredToken(model.Token);
            var username = principal.Claims.Where(x => x.Type == "sub").Select(x => x.Value).FirstOrDefault();

            var user = await _userManager.FindByNameAsync(username);
            var validToken = await CheckRefreshToken(user.Id, model.RefreshToken);
            if (validToken == null)
            {
                return new Result<RefreshTokenResponse>(null, Common.Enums.StatusCode.UNAUTHORIZED);
            }

            var newJwtToken = GenerateJwtToken(principal.Claims);
            var newRefreshToken = GenerateRefreshToken();

            await InvalidateRefreshToken(validToken);
            await SaveRefreshToken(user.Id, newRefreshToken);  // Save token to some data store

            return new Result<RefreshTokenResponse>(new RefreshTokenResponse(newJwtToken, newRefreshToken), Common.Enums.StatusCode.OK);
        }

        #endregion

        #region -> Private Functions

        /// <summary>
        /// Check the validity of the passed in token based on the user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<UserTokens> CheckRefreshToken (string id, string token)
        {
            var foundTok = await _userContext.UserWebTokens
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
            List<UserTokens> tokens = null;
            if (source == null)
                tokens = await _userContext.UserWebTokens.Where(x => x.UserId == id).ToListAsync();
            else
                tokens = await _userContext.UserWebTokens.Where(x => x.UserId == id && x.Source == source).ToListAsync();

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
                await _userContext.SaveChangesAsync();
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
        private async Task<bool> InvalidateRefreshToken (UserTokens token)
        {
            token.Valid = false;
            try
            {
                await _userContext.SaveChangesAsync();
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
            var tokenIP = await _userContext.UserWebTokens.Where(x => x.Source == ip && x.UserId == id).FirstOrDefaultAsync();
            if (tokenIP == null)
            {
                var newTok = new UserTokens
                {
                    Token = token,
                    UserId = id,
                    Valid = true,
                    Source = _user.IP
                };
                _userContext.UserWebTokens.Add(newTok);
            }
            else
            {
                tokenIP.Issued = now;
                tokenIP.Token = token;
                tokenIP.Valid = true;
            }

            try
            {
                await _userContext.SaveChangesAsync();
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
                IdentityRole role = new IdentityRole()
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
                IdentityRole role = new IdentityRole()
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
                IdentityRole role = new IdentityRole()
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

        #endregion
    }
}
