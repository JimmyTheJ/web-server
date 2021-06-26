using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using VueServer.Domain.Interface;
using VueServer.Domain.Enums;
using System;

namespace VueServer.Core.StatusFactory
{
    public class StatusCodeFactory : IStatusCodeFactory<IActionResult>
    {
        public IActionResult GetStatusCode(StatusCode code, object obj = null)
        {
            switch (code)
            {
                case StatusCode.OK:
                    return new OkObjectResult(obj);
                case StatusCode.CREATED:
                    return new StatusCodeResult(StatusCodes.Status201Created);
                case StatusCode.ACCEPTED:
                    return new AcceptedResult();
                case StatusCode.NO_CONTENT:
                    return new NoContentResult();
                case StatusCode.MOVED_PERMANENTLY:
                    return new StatusCodeResult(StatusCodes.Status301MovedPermanently);
                case StatusCode.FOUND:
                    return new StatusCodeResult(StatusCodes.Status302Found);
                case StatusCode.TEMPORARY_REDIRECT:
                    return new StatusCodeResult(StatusCodes.Status307TemporaryRedirect);
                case StatusCode.PERMANENT_REDIRECT:
                    return new StatusCodeResult(StatusCodes.Status308PermanentRedirect);
                case StatusCode.BAD_REQUEST:
                    return new BadRequestObjectResult(obj);
                case StatusCode.FORBIDDEN:
                    return new ForbidResult();
                case StatusCode.UNAUTHORIZED:
                    return new UnauthorizedResult();
                case StatusCode.NOT_FOUND:
                    return new NotFoundResult();
                case StatusCode.SERVER_ERROR:
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                case StatusCode.UNAVAILABLE:
                    return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
                default:
                    Console.WriteLine($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Invalid Status Code. Returning null.");
                    return null;
            }
        }

        public IActionResult GetStatusCode(IResult result)
        {
            return GetStatusCode(result.Code, result.Obj);
        }
    }
}
