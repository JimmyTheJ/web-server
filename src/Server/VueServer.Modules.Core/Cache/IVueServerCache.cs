using System;

namespace VueServer.Modules.Core.Cache
{
    public interface IVueServerCache : IDisposable
    {
        bool TryGetValue<T>(string key, out T obj);
        bool GetSubDictionaryValue<T>(string key, string subKey, out T value);
        bool SetSubDictionaryValue<T>(string key, string subKey, T value);
        void Clear();
        void Update(string key);
    }
}
