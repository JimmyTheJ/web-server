using System;

using VueServer.Common.Interface;

namespace VueServer.Services.Interface
{
    public interface IFileServerService
    {
        IResult<string[]> GetFiles (string webrootPath);
    }
}
