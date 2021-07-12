using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using VueServer.Core.Cache;
using VueServer.Models.Modules;

namespace VueServer.Controllers.Filters
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
