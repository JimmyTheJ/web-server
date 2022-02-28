namespace VueServer.Modules.Core.Services.Account
{
    public enum TokenValidation
    {
        Valid = 0,
        MissingRefreshToken = 1,
        InvalidRefreshToken = 2,
        RequiresNewJwt = 3
    }
}
