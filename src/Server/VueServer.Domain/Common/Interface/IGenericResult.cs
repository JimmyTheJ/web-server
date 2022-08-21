using VueServer.Domain.Enums;

namespace VueServer.Domain.Interface
{
    public interface IResult<T> : IResult
    {
        new T Obj { get; set; }

        new StatusCode Code { get; set; }
    }
}
