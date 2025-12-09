using ConfidoSoft.Data.Domain.Consts;
using ConfidoSoft.Data.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConfidoSoft.Data.Domain.HCModels
{
    /// <summary>
    /// Default system generated records helper class.
    /// Which will create default records in database as part of database migration process.
    /// </summary>
    public class SystemGeneratedRecordHelper
    {
        /// <summary>
        /// Master admin for all site or allowed all the operations.
        /// Add all other roles for this user if you add this role to a user.. all allowed!!
        /// So add this user in all other next level roles, so that no special handling required for this role login!
        /// </summary>
        public readonly String GlobalAdministrator = "GlobalAdministrator";
        //private readonly string GlobalAdministratorDesc = "Global Administrator have full access to application";
               
        /// <summary>
        /// Master login for developers! user for all site or allowed all the operations.
        /// Add all other roles for this user if you add this role to a user.. all allowed!!
        /// This login is not to publish
        /// </summary>
        public readonly String Developer = "developer@confidosoft.com";
        public readonly String DeveloperName = "Developer";
        public readonly String DefaultPassword = "Test@123";

        /// <summary>
        /// admin login for user site owner.
        /// </summary>
        public readonly String AdminUser = "admin@confidosoft.com";
        public readonly String AdminUserName = "Administrator";


        /// <summary>
        /// Default roles for developer and admin user.
        /// </summary>
        /// <returns></returns>
        public List<String> GetDeveloperAndAdminUser()
        {
            return new List<string> { 
                //GlobalAdministrator 
            };
        }

        /// <summary>
        /// List of fix roles
        /// </summary>
        /// <returns></returns>
        public List<Role> GetHardcodeRoles()
        {
            List<Role> roleList = new List<Role>();

//            Role standardRole = null;
//
//            standardRole = new Role
//            {
//                Name = GlobalAdministrator,
//                Description = GlobalAdministratorDesc,
//                RoleType = EnumRoleType.GlobalAdministrator,
//                CreatedOn = DateTime.UtcNow,
//            };
//            standardRole.ModifiedOn = standardRole.CreatedOn;
//            roleList.Add(standardRole);

            
            return roleList;
        }
    }
}
