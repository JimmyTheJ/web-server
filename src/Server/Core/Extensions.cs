namespace VueServer.Core
{
    public static class Extensions
    {
        public static string BoolToString(this bool val)
        {
            return val ? "1" : "0";
        }
    }
}
