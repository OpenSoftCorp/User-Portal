using Microsoft.AspNetCore.Mvc.Controllers;
using System.Net;

namespace UserPortal.Middleware
{
    public class AuthenticationMiddleware: IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {

            var EndPoint = context.GetEndpoint();
            if (EndPoint != null)
            {
                var controllerActionDescriptor = EndPoint.Metadata.GetMetadata<ControllerActionDescriptor>();

                var controllerName = controllerActionDescriptor.ControllerName;

                var actionName = controllerActionDescriptor.ActionName;

                if (controllerName != "Login")
                {
                    if (context.Session.GetString("Role") == null)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        context.Response.Redirect("/Login/Index");
                    }
                    else
                    {
                        await next(context);
                    }
                }
                else
                {
                    await next(context);
                }
            }
            else
            {
                await next(context);
            }
        }
    }
}
