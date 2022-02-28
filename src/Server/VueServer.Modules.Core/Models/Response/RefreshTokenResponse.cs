namespace VueServer.Modules.Core.Models.Response
{
    public class RefreshTokenResponse
    {
        public string Token { get; set; }

        public string RefreshToken { get; set; }

        public RefreshTokenResponse() { }

        public RefreshTokenResponse(string token, string refreshToken)
        {
            Token = token;
            RefreshToken = refreshToken;
        }
    }
}
