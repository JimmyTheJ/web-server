namespace VueServer.Core
{
    public static class Extensions
    {
        public static string BoolToString(this bool val)
        {
            return val ? "1" : "0";
        }

        public static bool? StringToBool(this string val)
        {
            if (val != "1" && val != "0")
                return null;

            return val == "1" ? true : false;
        }
    }
}
