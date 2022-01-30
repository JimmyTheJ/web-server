namespace VueServer.Services.Enums
{
    public enum TokenValidation
    {
        Valid = 0,
        MissingRefreshToken = 1,
        InvalidRefreshToken = 2,
        RequiresNewJwt = 3
    }
}
