using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                result = Set<T>(key, obj, out value);
            }

            return result;
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
                Set<object>(key, obj, out var value);
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

        private bool Set<T>(string key, object obj, out T value)
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

    public class CacheMap
    {
        public const string UserModuleAddOn = "UserModuleAddOn";
        public const string UserModuleFeature = "UserModuleFeature";
    }
}
