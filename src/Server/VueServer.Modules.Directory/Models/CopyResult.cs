
using VueServer.Domain.Enums;

namespace VueServer.Modules.Directory.Services
{
    internal class CopyResult
    {
        public StatusCode Code { get; set; }
        public string SourceBasePath { get; set; }
        public string SourceFullPath { get; set; }
        public string DestinationBasePath { get; set; }
        public string DestinationFullPath { get; set; }
    }
}
