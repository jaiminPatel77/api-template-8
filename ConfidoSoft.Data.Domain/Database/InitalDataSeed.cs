using ConfidoSoft.Data.Domain.Consts;
using ConfidoSoft.Data.Domain.DBModels;
using ConfidoSoft.Data.Domain.HCModels;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConfidoSoft.Data.Domain.Database
{
    /// <summary>
    /// Helper class to populate initial default database records if not already exists.
    /// </summary>
    public static class InitalDataSeed
    {
        /// <summary>
        /// Initialize database with default records.
        /// </summary>
        /// <param name="context">Application database context</param>
        /// <param name="userManager">User Manager to add default users records if not exits.</param>
        /// <param name="roleManager">Role Manage to add default roles if not exists.</param>
        /// <returns></returns>
        public static async Task InitializeDatabase(ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<Role> roleManager
            )
        {
            SystemGeneratedRecordHelper hdDataRecordHelper = new SystemGeneratedRecordHelper();
            //Adding code to create default roles here.
            if (!context.Roles.AsQueryable().Any(r => r.Name == hdDataRecordHelper.GlobalAdministrator))
            {
                var defaultApplicationRoles = hdDataRecordHelper.GetHardcodeRoles();

                foreach (var roleRecord in defaultApplicationRoles)
                {
                    await roleManager.CreateAsync(roleRecord);
                }
            }

            //create default user             
            if (!context.Users.AsQueryable().Any(u => u.UserName == hdDataRecordHelper.Developer))
            {
                var defaultRoles = hdDataRecordHelper.GetDeveloperAndAdminUser();
                var userRecord = new User
                {
                    UserName = hdDataRecordHelper.Developer,
                    Email = hdDataRecordHelper.Developer,
                    FullName = hdDataRecordHelper.DeveloperName,
                    UserType = EnumUserType.GlobalAdministrator,
                    CreatedOn = DateTimeOffset.UtcNow,
                    ModifiedOn = DateTimeOffset.UtcNow,
                };
                await userManager.CreateAsync(userRecord, hdDataRecordHelper.DefaultPassword);
                await userManager.AddToRolesAsync(userRecord, defaultRoles);
                userRecord = new User
                {
                    UserName = hdDataRecordHelper.AdminUser,
                    FullName = hdDataRecordHelper.AdminUserName,
                    Email = hdDataRecordHelper.AdminUser,
                    UserType = EnumUserType.GlobalAdministrator,
                    CreatedOn = DateTimeOffset.UtcNow,
                    ModifiedOn = DateTimeOffset.UtcNow,
                };
                await userManager.CreateAsync(userRecord, hdDataRecordHelper.DefaultPassword);
                await userManager.AddToRolesAsync(userRecord, defaultRoles);
            }
        }
    }
}
