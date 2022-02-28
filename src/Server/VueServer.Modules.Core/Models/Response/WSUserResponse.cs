using Newtonsoft.Json;
using VueServer.Modules.Core.Models.User;

namespace VueServer.Modules.Core.Models.Response
{
    public class WSUserResponse
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Avatar { get; set; }
        public bool ChangePassword { get; set; }

        public WSUserResponse() { }

        public WSUserResponse(WSUser user)
        {
            if (user != null)
            {
                Id = user.Id;
                DisplayName = user.DisplayName;
                Avatar = user.UserProfile?.AvatarPath;
            }
        }
    }
}
