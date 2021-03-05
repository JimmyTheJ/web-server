using System;
using System.Collections.Generic;
using System.Text;

namespace VueServer.Models.Response
{
    public class RefreshTokenResponse
    {
        public String Token { get; set; }

        public String RefreshToken { get; set; }

        public RefreshTokenResponse () { }

        public RefreshTokenResponse (String token, String refreshToken)
        {
            Token = token;
            RefreshToken = refreshToken;
        }
    }
}
