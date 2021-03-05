using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using VueServer.Models.Context;

namespace VueServer.Controllers.Filters
{
    public class ModuleAuthFilter : IActionFilter
    {
        private readonly IWSContext _wsContext;

        public string UserId { get; set; }
        public string Module { get; set; }
        public string Feature { get; set; }

        public ModuleAuthFilter(IWSContext wsContext)
        {
            _wsContext = wsContext;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // noop
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            UserId = context.HttpContext.User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Sub).Select(x => x.Value).Single();

            var userHasModule = _wsContext.UserHasModule.Where(x => x.ModuleAddOnId == Module && x.UserId == UserId).SingleOrDefault();
            if (userHasModule == null)
            {
                context.Result = new UnauthorizedResult();
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(Feature))
                {
                    var userHasModuleFeature = _wsContext.UserHasFeature.Where(x => x.ModuleFeatureId == Feature && x.UserId == UserId).SingleOrDefault();
                    if (userHasModuleFeature == null)
                    {
                        context.Result = new UnauthorizedResult();
                    }
                }
            }
        }
    }
}
