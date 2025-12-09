using ConfidoSoft.Data.Domain.Consts;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConfidoSoft.Data.Services.BLServices
{
    /// <summary>
    /// Information above currently logged in user
    /// </summary>
    public interface ICurrentUserInfo
    {
        /// <summary>
        /// Email address of currently logged in user
        /// </summary>
        String Email { get; }

        /// <summary>
        /// Full name of currently logged in user.
        /// </summary>
        String FullName { get;}

        /// <summary>
        /// User Id as string format
        /// </summary>
        String UserId { get; }

        /// <summary>
        /// Logged in User database record id
        /// </summary>
        long? Id { get;}

        /// <summary>
        /// Indicate whether user is logged in or not..
        /// </summary>
        bool IsUserLoggedIn { get;}

        ///// <summary>
        ///// Logged in user max level role type!
        ///// Used to allow permissions base on max allowed role type.
        ///// </summary>
        //EnumRoleType RoleType { get;}

        /// <summary>
        /// Logged in user type!
        /// Used to allow permissions base on user  type or associated roles for a given user.
        /// </summary>
        EnumUserType UserType { get;}

        /// <summary>
        /// Logged in all role ids. Used to implement custom permissions
        /// </summary>
        List<long> RoleIds { get; }
    }
}
