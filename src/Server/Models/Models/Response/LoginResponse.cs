using System.Collections.Generic;

namespace VueServer.Models.Response
{
    public class LoginResponse
    {
        public string Token { get; set; }

        public string RefreshToken { get; set; }

        public WSUserResponse User { get; set; }

        public IList<string> Roles { get; set; }

        public LoginResponse() { }

        public LoginResponse(string token, WSUserResponse user, IList<string> roles)
        {
            Token = token;
            User = user;
            Roles = roles;
        }
    }
}
