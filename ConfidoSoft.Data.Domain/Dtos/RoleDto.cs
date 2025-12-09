using ConfidoSoft.Data.Domain.Consts;
using ConfidoSoft.Data.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text;

namespace ConfidoSoft.Data.Domain.Dtos
{
    #region Role Permission Dto model
    /// <summary>
    /// Dto for Role permission.
    /// Permission Information for each actual storage Entity or any virtual section or department.
    /// </summary>
    public class PermissionDto : BaseDto
    {

        /// <summary>
        /// Indicate to what permission apply to. e.g. Specific storage entity or virtual department in application.
        /// </summary>
        public EnumPermissionFor PermissionFor { get; set; }

        /// <summary>
        /// Define actual allowed Permissions for associated entity.
        /// </summary>
        public EnumPermissions Permissions { get; set; }

        /// <summary>
        /// Select expression to be used to populate Dto directly from Database
        /// </summary>
        public static Expression<Func<RolePermission, PermissionDto>> ToDto = e => new PermissionDto
        {
            Id = e.Id,
            PermissionFor = e.PermissionFor,
            //RoleId = e.RoleId,
            Permissions = e.Permissions
        };

        /// <summary>
        /// Function will Marge the permission information from source to destination
        /// </summary>
        /// <param name="source">Source Role permissions</param>
        /// <param name="dest">Destination Permission Dto</param>
        public static void MargePermission( RolePermission source, PermissionDto dest)
        {
            dest.Permissions = (dest.Permissions | source.Permissions);
        }
    }

    #endregion

    #region Role entity related Dto's

    /// <summary>
    /// Dto for any lookup of role
    /// </summary>
    public class RoleLookUpDto : BaseDto
    {
        /// <summary>
        /// Role name.
        /// </summary>
        public string Name { get; set; }
        //public string Description { get; set; }

        /// <summary>
        /// Role type.
        /// </summary>
        public EnumRoleType RoleType { get; set; }

        /// <summary>
        /// Select expression to be used to populate Dto directly from Database
        /// </summary>
        public static Expression<Func<Role, RoleLookUpDto>> ToDto = e => new RoleLookUpDto
        {
            Id = e.Id,
            Name = e.Name,
            RoleType = e.RoleType,
        };        
    }


    /// <summary>
    /// Dto to be used for Role list view.
    /// </summary>
    public class RoleListDto : BaseDtoWithCommonFields
    {
        /// <summary>
        /// Name of the role
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the Role.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Role type by default it's custom.
        /// </summary>
        public EnumRoleType RoleType { get; set; }        

        /// <summary>
        /// Select expression to be used to populate Dto directly from Database
        /// </summary>
        public static Expression<Func<Role, RoleListDto>> ToDto = e => new RoleListDto
        {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description,
            RoleType = e.RoleType,
            CreatedById = e.CreatedById,
            CreatedOn = e.CreatedOn,
            ModifiedById = e.ModifiedById,
            ModifiedOn = e.ModifiedOn
        };
    }

    /// <summary>
    /// Dto for Role detail page!
    /// </summary>
    public class RoleDto : BaseDtoWithCommonFields
    {
        /// <summary>
        /// Name of the role. Must input and has to be unique.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Description of the role.
        /// </summary>
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// Role type. default is custom.
        /// </summary>
        public EnumRoleType RoleType { get; set; }

        /// <summary>
        /// List of the Users having a role.
        /// </summary>
        public List<UserLookUpDto> Users { get; set; }

        /// <summary>
        /// List of the Permissions selected for a role.
        /// </summary>
        public List<PermissionDto> Permissions { get; set; }
        //public Dictionary<EnumPermissionFor, RolePermissionDto> Permissions { get; set; }

        /// <summary>
        /// select expression to be used to populate Dto directly from Database
        /// </summary>
        public static Expression<Func<Role, RoleDto>> ToDto = e => new RoleDto
        {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description,
            RoleType = e.RoleType,
            CreatedById = e.CreatedById,
            CreatedOn = e.CreatedOn,
            ModifiedById = e.ModifiedById,
            ModifiedOn = e.ModifiedOn
        };
    }

    #endregion


}
