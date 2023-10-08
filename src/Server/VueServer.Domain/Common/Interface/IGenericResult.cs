using VueServer.Domain.Enums;

namespace VueServer.Domain.Interface
{
    public interface IServerResult<T> : IServerResult
    {
        new T Obj { get; set; }

        new StatusCode Code { get; set; }
    }
}
