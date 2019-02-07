using System;
using System.Collections.Generic;
using System.Text;
using VueServer.Domain.Enums;
using VueServer.Domain.Interface;

namespace VueServer.Domain.Factory.Interface
{
    public interface IStatusCodeFactory<T>
    {

        T GetStatusCode(StatusCode code, Object obj = null);

        T GetStatusCode(IResult result);
    }
}
