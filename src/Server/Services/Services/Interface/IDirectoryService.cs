﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using VueServer.Domain.Enums;
using VueServer.Domain.Interface;
using VueServer.Models;
using VueServer.Models.Request;
using VueServer.Models.Directory;


namespace VueServer.Services.Interface
{
    public interface IDirectoryService
    {
        Task<IResult<IEnumerable<ServerDirectory>>> GetDirectories ();

        Task<IResult<Tuple<string, string, string>>> Download (string filename, string user, bool media = false);

        Task<IResult<IOrderedEnumerable<WebServerFile>>> Load (string directory, string subDir);

        Task<IResult<WebServerFile>> Upload(UploadDirectoryFileRequest model);

        Task<IResult<bool>> Delete(DeleteFileModel model);
    }
}