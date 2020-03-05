﻿using System;

using VueServer.Domain.Enums;
using VueServer.Domain.Interface;

namespace VueServer.Domain.Concrete
{
    public class Result<T> : IResult<T>, IResult
    {
        public T Obj { get; set; }

        public StatusCode Code { get; set; }

        Object IResult.Obj
        {
            get
            {
                return (T) Obj;
            }
            set
            {
                if (!(value is T))
                {
                    throw new ArgumentException($"The type of {value.GetType().Name} cannot be assigned to type of {typeof(T).Name}");
                }

                Obj = (T)value;
            }
        }

        public Result(T obj, StatusCode code = StatusCode.OK)
        {
            Obj = obj;
            Code = code;
        }
    }
}
