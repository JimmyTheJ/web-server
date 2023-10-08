using VueServer.Domain.Enums;
using VueServer.Domain.Interface;

namespace VueServer.Core.Status
{
    public interface IStatusCodeFactory<T>
    {

        T GetStatusCode(StatusCode code, object obj = null);

        T GetStatusCode(IServerResult result);
    }
}
