using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using VueServer.Models.Context;

namespace VueServer.Core.Cache
{
    public class VueServerCache : IVueServerCache
    {
        private bool disposedValue;

        private static Dictionary<string, object> Cache = new Dictionary<string, object>();

        private readonly IServiceScopeFactory _serviceScopeFactory;

        public VueServerCache(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            bool result = false;
            value = default(T);

            if (Cache == null)
            {
                Cache = new Dictionary<string, object>();
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                return result;
            }

            if (Cache.ContainsKey(key))
            {
                if (Cache[key] is T)
                {
                    value = (T)Cache[key];
                    result = true;
                }
            }
            else
            {
                var obj = GetCacheObject(key);
                if (obj != null)
                {
                    result = SetKey<T>(key, obj, out value);
                }
            }

            return result;
        }

        public bool GetSubDictionaryValue<T>(string key, string subKey, out T value)
        {
            bool result = false;
            value = default(T);

            if (Cache == null)
            {
                Cache = new Dictionary<string, object>();
            }

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(subKey))
            {
                return result;
            }

            if (Cache.ContainsKey(key) && Cache[key] is Dictionary<string, T> subDic)
            {
                if (subDic.ContainsKey(subKey))
                {
                    value = subDic[subKey];
                    result = true;
                }

            }

            return result;
        }

        public bool SetSubDictionaryValue<T>(string key, string subKey, T value)
        {
            if (Cache == null)
            {
                Cache = new Dictionary<string, object>();
            }

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(subKey))
            {
                return false;
            }

            if (Cache.ContainsKey(key) && Cache[key] is Dictionary<string, T> subDic)
            {
                subDic[subKey] = value;
            }
            else
            {
                Cache[key] = new Dictionary<string, T>()
                {
                    { subKey, value }
                };
            }

            return true;
        }

        public void Update(string key)
        {
            if (Cache == null)
            {
                Cache = new Dictionary<string, object>();
            }

            if (!string.IsNullOrWhiteSpace(key))
            {
                var obj = GetCacheObject(key);
                SetKey<object>(key, obj, out var value);
            }
        }

        public void Clear()
        {
            if (Cache != null)
            {
                Cache[CacheMap.UserModuleAddOn] = null;
                Cache[CacheMap.UserModuleFeature] = null;
            }

            Cache = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private bool SetKey<T>(string key, object obj, out T value)
        {
            var result = false;
            value = default(T);

            if (obj == null)
            {
                return result;
            }

            if (obj is T)
            {
                Cache[key] = obj;
                value = (T)Cache[key];
                result = true;
            }

            return result;
        }

        private object GetCacheObject(string key)
        {
            object value = null;

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<IWSContext>();

                if (key == CacheMap.UserModuleAddOn)
                {
                    value = context.UserHasModule.ToList();
                }
                else if (key == CacheMap.UserModuleFeature)
                {
                    value = context.UserHasFeature.ToList();
                }
            }

            return value;
        }
    }

    public static class CacheMap
    {
        public const string UserModuleAddOn = "UserModuleAddOn";
        public const string UserModuleFeature = "UserModuleFeature";
        public const string BlockedIP = "BlockedIP";
    }
}
