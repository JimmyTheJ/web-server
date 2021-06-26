using System;
using VueServer.Domain.Enums;
using VueServer.Domain.Interface;

namespace VueServer.Core.StatusFactory
{
    public interface IStatusCodeFactory<T>
    {

        T GetStatusCode(StatusCode code, Object obj = null);

        T GetStatusCode(IResult result);
    }
}
