using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using VueServer.Common.Interface;
using VueServer.Models;
using VueServer.Models.Directory;
using VueServer.Common;

namespace VueServer.Services.Interface
{
    public interface IDirectoryService
    {
        IResult<IEnumerable<string>> GetDirectories (int level = Constants.NO_LEVEL);

        IResult<Tuple<string, string, string>> Download (int level, string filename);

        IResult<IOrderedEnumerable<WebServerFile>> Load (string directory, string subDir, int level);
    }
}
