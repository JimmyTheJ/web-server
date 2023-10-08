using System;
using VueServer.Domain.Enums;

namespace VueServer.Domain.Interface
{
    public interface IServerResult
    {
        Object Obj { get; set; }

        StatusCode Code { get; set; }
    }
}
