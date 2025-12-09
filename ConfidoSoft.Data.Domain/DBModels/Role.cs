using ConfidoSoft.Data.Domain.Consts;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConfidoSoft.Data.Domain.DBModels
{
    /// <summary>
    /// Identity Role table.
    /// </summary>
    public class Role : IdentityRole<long>, IModelBase, IRecordModifiedInfo, IRecordCreatedInfo
    {
        public Role()
        {
            //https://stackoverflow.com/questions/20757594/ef-codefirst-should-i-initialize-navigation-properties
            this.RolePermissions = new HashSet<RolePermission>();
        }

        /// <summary>
        /// Multi-line description of Role.
        /// </summary>
        [StringLength(1024)]
        public string Description { get; set; }

        /// <summary>
        /// User role type. e.g. Administrator or Custom Role or any other type as application needed.
        /// Global Administrator has all access in application. 
        /// </summary>
        public EnumRoleType RoleType { get; set; }

        /// <summary>
        /// Record created on DateTime.
        /// </summary>
        public DateTimeOffset CreatedOn { get; set; }

        /// <summary>
        /// Record created by UserId.
        /// </summary>
        public long? CreatedById { get; set; }

        /// <summary>
        /// Record Modified On DateTime. For newly created record it will be
        /// same as Created On.
        /// </summary>
        public DateTimeOffset ModifiedOn { get; set; }

        /// <summary>
        /// Record Modified by UserId.
        /// </summary>
        public long? ModifiedById { get; set; }

        /// <summary>
        /// Indicate that User record is disabled.
        /// So user will not be allowed to logged in.
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Date when user was last Enabled/Disabled.
        /// </summary>
        public DateTimeOffset? EnabledDisabledOn { get; set; }

        /// <summary>
        /// Navigation property for Permissions associated for custom/standard roles.
        /// </summary>
        public virtual ICollection<RolePermission> RolePermissions { get; set; }

        /// <summary>
        /// Navigation property for the roles this user belongs to.
        /// </summary>
        public virtual ICollection<IdentityUserRole<long>> UserRoles { get; set; }
       
    }
}
