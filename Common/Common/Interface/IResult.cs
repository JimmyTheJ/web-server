using System;
using VueServer.Common.Enums;

namespace VueServer.Common.Interface
{
    public interface IResult
    {
        Object Obj { get; set; }

        StatusCode Code { get; set; }
    }
}
