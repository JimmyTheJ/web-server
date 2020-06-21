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

        public WSUser User { get; set; }

        public IList<String> Roles { get; set; }

        public LoginResponse () { }

        public LoginResponse (String token, String refreshToken, WSUser user, IList<String> roles)
        {
            Token = token;
            RefreshToken = refreshToken;
            User = user;
            Roles = roles;
        }
    }
}
