using System;
using System.Collections.Generic;
using System.Text;

namespace VueServer.Domain.Interface
{
    public interface IPK<T>
    {
        T Id { get; set; }
    }
}
