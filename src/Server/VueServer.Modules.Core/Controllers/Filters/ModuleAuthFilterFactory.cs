using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace VueServer.Modules.Core.Controllers.Filters
{
    public class ModuleAuthFilterFactory : Attribute, IFilterFactory
    {
        public string Module { get; set; }
        public string Feature { get; set; }

        public bool IsReusable => false;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var filter = serviceProvider.GetRequiredService<ModuleAuthFilter>();

            filter.Module = Module;
            filter.Feature = Feature;

            return filter;
        }
    }
}
