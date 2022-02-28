using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Collections.Generic;
using System.Linq;
using VueServer.Modules.Core.Cache;
using VueServer.Modules.Core.Models.Modules;

namespace VueServer.Modules.Core.Controllers.Filters
{
    public class ModuleAuthFilter : IActionFilter
    {
        private readonly IVueServerCache _cache;

        public string UserId { get; set; }
        public string Module { get; set; }
        public string Feature { get; set; }

        public ModuleAuthFilter(IVueServerCache cache)
        {
            _cache = cache;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // noop
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            UserId = context.HttpContext.User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Sub).Select(x => x.Value).Single();

            if (_cache.TryGetValue<IEnumerable<UserHasModuleAddOn>>(CacheMap.UserModuleAddOn, out var usersHaveModules))
            {
                var userHasModule = usersHaveModules.Where(x => x.ModuleAddOnId == Module && x.UserId == UserId).SingleOrDefault();
                if (userHasModule == null)
                {
                    context.Result = new UnauthorizedResult();
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(Feature))
                    {
                        if (_cache.TryGetValue<IEnumerable<UserHasModuleFeature>>(CacheMap.UserModuleFeature, out var usersHaveFeatures))
                        {
                            var userHasModuleFeature = usersHaveFeatures.Where(x => x.ModuleFeatureId == Feature && x.UserId == UserId).SingleOrDefault();
                            if (userHasModuleFeature == null)
                            {
                                context.Result = new UnauthorizedResult();
                            }
                        }
                        else
                        {
                            context.Result = new StatusCodeResult(500);
                        }
                    }
                }
            }
            else
            {
                context.Result = new StatusCodeResult(500);
            }
        }
    }
}
