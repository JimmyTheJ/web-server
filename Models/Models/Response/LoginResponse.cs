using System;
using System.Collections.Generic;
using System.Text;
using VueServer.Models.User;

namespace VueServer.Models.Response
{
    public class LoginResponse
    {
        public string Token { get; set; }

        public string RefreshToken { get; set; }

        public WSUserResponse User { get; set; }

        public IList<String> Roles { get; set; }

        public LoginResponse () { }

        public LoginResponse (string token, string refreshToken, WSUserResponse user, IList<string> roles)
        {
            Token = token;
            RefreshToken = refreshToken;
            User = user;
            Roles = roles;
        }
    }
}
