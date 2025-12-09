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
    /// Model for Application User (using ASP Identity).
    /// </summary>
    public class User : IdentityUser<long>, IModelBase, IRecordModifiedInfo, IRecordCreatedInfo
    {
        /// <summary>
        /// Full name of user
        /// </summary>
        [Required]
        [StringLength(128)]
        [ProtectedPersonalData]
        public string FullName { get; set; }

        /// <summary>
        /// Title of User
        /// </summary>
        [StringLength(128)]
        [ProtectedPersonalData]
        public string Title { get; set; }

        /// <summary>
        /// Indicate whether User profile image is available or not.
        /// </summary>
        public bool IsImageAvailable { get; set; }        


        /// <summary>
        /// Type  e.g. SystemGenerated or registered or manually created.
        /// </summary>
        public EnumUserType UserType { get; set; }

        /// <summary>
        /// Status of User Record. e.g.Password is generated or reset password needed!
        /// </summary>
        public EnumUserStatus Status { get; set; }
       
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
        /// Last password change date. Used to allowed must change password within some duration.
        /// </summary>
        public DateTimeOffset? PreviousPasswordDate { get; set; }

        /// <summary>
        /// Previous password 1. Used to validate that user must not set same password.
        /// </summary>
        [StringLength(1024)]
        public string PreviousPassword1 { get; set; }

        /// <summary>
        /// Previous password 2. Used to validate that user must not set same password.
        /// </summary>
        [StringLength(1024)]
        public string PreviousPassword2 { get; set; }

        /// <summary>
        /// Previous password 3. Used to validate that user must not set same password.
        /// </summary>
        [StringLength(1024)]
        public string PreviousPassword3 { get; set; }

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
        /// User Profile information. Used separate table as it include profile image.
        /// </summary>
        public virtual UserProfile UserProfile { get; set; }

        /// <summary>
        /// User Refresh tokens navigation property!
        /// </summary>
        public virtual ICollection<UserRefreshToken> UserRefreshTokens { get; set; }

        /// <summary>
        /// Navigation property for the roles this user belongs to.
        /// </summary>
        public virtual ICollection<IdentityUserRole<long>> UserRoles { get; set; }


    }
}
