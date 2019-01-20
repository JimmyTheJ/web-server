using VueServer.Common.Enums;

namespace VueServer.Common.Interface
{
    public interface IResult<T> : IResult
    {
        new T Obj { get; set; }

        new StatusCode Code { get; set; }
    }
}
