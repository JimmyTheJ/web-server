using Newtonsoft.Json.Serialization;

namespace VueServer.Classes
{
    public class LowercaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLower()[0] + propertyName.Substring(1);
        }
    }
}
