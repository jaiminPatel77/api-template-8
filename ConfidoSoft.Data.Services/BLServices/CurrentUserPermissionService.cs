using ConfidoSoft.Data.Domain.Consts;
using ConfidoSoft.Data.Domain.Database;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace ConfidoSoft.Data.Services.BLServices
{

    #region ICurrentUserPermissionService interface
    
    /// <summary>
    /// Implementation for custom role permissions.
    /// </summary>
    public interface ICurrentUserPermissionService
    {
        /// <summary>
        /// Return true, if currently logged in user has requested permission for given permission type/For!
        /// </summary>
        /// <param name="permissionFor">Permission type or for which entity or area</param>
        /// <param name="permissions">Actual permission to be checked.</param>
        /// <returns></returns>
        bool HasPermission(EnumPermissionFor permissionFor, EnumPermissions permissions);
    }

    #endregion


    #region ICurrentUserPermissionService implementation

    /// <summary>
    /// Implementation for custom role permissions.
    /// </summary>
    public class CurrentUserPermissionService : ICurrentUserPermissionService
    {
        private readonly ILogger _logger;
        private readonly ICurrentUserInfo _currentUser;
        private readonly ApplicationDbContext _dbContext;
        

        public CurrentUserPermissionService(ILogger<CurrentUserPermissionService> logger,
            ApplicationDbContext dbContext,
            ICurrentUserInfo currentUser)
        {
            this._logger = logger;
            this._currentUser = currentUser;
            this._dbContext = dbContext;
        }

        /// <summary>
        /// Return true, if currently logged in user has requested permission for given permission type/For!
        /// </summary>
        /// <param name="permissionFor">Permission type or for which entity or area</param>
        /// <param name="permissions">Actual permission to be checked.</param>
        /// <returns></returns>
        public bool HasPermission(EnumPermissionFor permissionFor, EnumPermissions permissions)
        {
            bool isAllowed = false;
            if (this._currentUser.IsUserLoggedIn)
            {
                //For global admin all operations allowed!
                if ( (this._currentUser.UserType == EnumUserType.GlobalAdministrator)
                    || (this._currentUser.UserType == EnumUserType.EnterpriseAdministrator))
                {
                    isAllowed = true;
                }
                else
                {
                    //Check as per valid roles for logged in user
                    if (this._currentUser.RoleIds.Count > 0)
                    {
                        //var userRoleIdList = userRoles.Select(e => e.RoleId).ToList();
                        var userRoleIdList = this._currentUser.RoleIds;
                        isAllowed = this._dbContext.RolePermissions.Where(r => ((r.PermissionFor == permissionFor) && ((r.Permissions & permissions) != 0) && (userRoleIdList.Contains(r.RoleId)))).Any();
                    }
                }
            }
            return isAllowed;
        }
    }

    #endregion

}
