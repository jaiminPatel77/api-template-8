using ConfidoSoft.Data.Domain.Consts;
using ConfidoSoft.Data.Services.BLServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSCoreEFTemplate8.Auth
{

    /// <summary>
    /// Custom permission attribute validation!
    /// //https://stackoverflow.com/questions/31464359/how-do-you-create-a-custom-authorizeattribute-in-asp-net-core
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CustomAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly EnumPermissionFor _permissionFor;
        private readonly EnumPermissions _permission;
        
        public CustomAuthorizeAttribute(EnumPermissionFor permissionFor, EnumPermissions permission)
        {
            _permissionFor = permissionFor;
            _permission = permission;
        }

        /// <summary>
        /// Return Forbidden if required permission not found for logged in user.. or yet to loggin!
        /// </summary>
        /// <param name="context"></param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                // it isn't needed to set unauthorized result 
                // as the base class already requires the user to be authenticated
                // this also makes redirect to a login page work properly
                // context.Result = new UnauthorizedResult();
                //log4net.GlobalContext.Properties["currentUser"] = "";
                return;
            }

            // you can also use registered services
            var currentUser = context.HttpContext.RequestServices.GetService<ICurrentUserInfo>();
            var currentUserPermission = context.HttpContext.RequestServices.GetService<ICurrentUserPermissionService>();
            //if (currentUser != null)
            //{
            //    log4net.GlobalContext.Properties["currentUser"] = currentUser.Username;
            //}
            var isAccess = currentUserPermission.HasPermission(this._permissionFor, this._permission);
            if (!isAccess)
            {
                context.Result = new StatusCodeResult((int)System.Net.HttpStatusCode.Forbidden);
                return;
            }

            //var isAuthorized = someService.IsUserAuthorized(user.Identity.Name, _someFilterParameter);
            //if (!isAuthorized)
            //{
            //    context.Result = new StatusCodeResult((int)System.Net.HttpStatusCode.Forbidden);
            //    return;
            //}
        }
    }

}
