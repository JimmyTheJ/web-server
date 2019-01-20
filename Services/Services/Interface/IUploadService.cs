using System.Collections.Generic;
using System.Threading.Tasks;
using VueServer.Common.Concrete;
using VueServer.Common.Interface;
using VueServer.Models;

namespace VueServer.Services.Interface
{
    public interface IUploadService
    {
        IResult<IEnumerable<string>> GetFolderList ();

        IResult<IEnumerable<DirectoryContentsResponse>> GetFiles();

        Task<IResult> Upload (UploadFileRequest model);

        IResult Delete (DeleteFileModel model);
    }
}
