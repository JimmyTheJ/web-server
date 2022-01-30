using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace VueServer.Models.Response
{
    public class LoginResponse
    {
        public string Token { get; set; }

        public string RefreshToken { get; set; }

        public WSUserResponse User { get; set; }

        public IList<string> Roles { get; set; }

        [JsonIgnore]
        public VueCookie RefreshCookie { get; set; } = new VueCookie();

        [JsonIgnore]
        public VueCookie ClientIdCookie { get; set; }

        public LoginResponse() { }

        public LoginResponse(string token, WSUserResponse user, IList<string> roles)
        {
            Token = token;
            User = user;
            Roles = roles;
        }
    }

    public class VueCookie
    {
        public CookieOptions Options { get; set; } = new CookieOptions();
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
