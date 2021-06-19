using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VueServer.Core.Cache
{
    public interface IVueServerCache : IDisposable
    {
        public bool TryGetValue<T>(string key, out T obj);
        public void Clear();
        public void Update(string key);
    }
}
