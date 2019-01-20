using System;
using System.Collections.Generic;
using System.Text;
using VueServer.Common.Enums;
using VueServer.Common.Interface;

namespace VueServer.Common.Factory.Interface
{
    public interface IStatusCodeFactory<T>
    {

        T GetStatusCode(StatusCode code, Object obj = null);

        T GetStatusCode(IResult result);
    }
}
