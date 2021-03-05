using Newtonsoft.Json.Serialization;

namespace VueServer.Classes
{
    public class LowercaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            if (propertyName != null && propertyName.Length > 1)
                return propertyName.ToLower()[0] + propertyName.Substring(1);
            else if (propertyName != null && propertyName.Length == 1)
                return propertyName.ToLower()[0].ToString();
            else
                return null;
        }
    }
}
