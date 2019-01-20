using System;
using System.Collections.Generic;
using System.Text;

namespace VueServer.Models.Response
{
    public class LoginResponse
    {
        public String Token { get; set; }

        public String RefreshToken { get; set; }

        public String Username { get; set; }

        public IList<String> Roles { get; set; }

        public LoginResponse () { }

        public LoginResponse (String token, String refreshToken, String username, IList<String> roles)
        {
            Token = token;
            RefreshToken = refreshToken;
            Username = username;
            Roles = roles;
        }
    }
}
