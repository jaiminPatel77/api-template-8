using ConfidoSoft.Data.Domain.Consts;
using ConfidoSoft.Data.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text;

/// <summary>
/// File define User entity related Dto's including CURD operation for User entity.
/// </summary>
namespace ConfidoSoft.Data.Domain.Dtos
{

    #region User List view and detail view related dto's
    /// <summary>
    /// Lookup Dto for User entity. To be used for filling any lookup combo to
    /// display associated users for an entity.
    /// </summary>
    public class UserLookUpDto : BaseDto
    {
        /// <summary>
        /// User title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Full name of the user.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Email of the user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Is profile image is available or not.
        /// </summary>
        public bool IsImageAvailable { get; set; }

        /// <summary>
        /// Type of the user. e.g. system generated or manually created. 
        /// </summary>
        public EnumUserType UserType { get; set; }

        /// <summary>
        /// Is user is disabled or not.
        /// </summary>
        public bool IsDisabled { get; set; }

        /// <summary>
        /// Select expression to be used to populate Dto directly from Database
        /// </summary>
        public static Expression<Func<User, UserLookUpDto>> ToDto = e => new UserLookUpDto
        {
            Id = e.Id,
            Title = e.Title,
            FullName = e.FullName,
            Email = e.Email,
            IsImageAvailable = e.IsImageAvailable,
            UserType = e.UserType,
            IsDisabled = e.Disabled,
        };
    }

    /// <summary>
    /// Dto for User list view.
    /// </summary>
    public class UserListDto : BaseDto
    {
        /// <summary>
        /// Title of User
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Email of the user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Full name of the user.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Phone number of the user.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Indicate whether image is available or not.
        /// </summary>
        public bool IsImageAvailable { get; set; }

        /// <summary>
        /// Is user disabled.
        /// </summary>
        public bool IsDisabled { get; set; }

        /// <summary>
        /// User type. e.g. system generated or manually added.
        /// </summary>
        public EnumUserType UserType { get; set; }

        /// <summary>
        /// Status of User Record. e.g.Password is generated or reset password needed!
        /// </summary>
        public EnumUserStatus Status { get; set; }

        /// <summary>
        /// Date when user what last Enabled/Disabled.
        /// </summary>
        public DateTimeOffset? LastEnableDisable { get; set; }

        /// <summary>
        /// Indicate that user account is locked out because of invalid password try.
        /// </summary>
        public bool IsLockedOut { get; set; }
    }


    /// <summary>
    /// User detail page Dto.
    /// </summary>
    public class UserDto : BaseDto
    {
        /// <summary>
        /// EnterpriseId as FK to Enterprise table.
        /// Can be null for Global system user!
        /// Must set any new user!
        /// </summary>
        public long? EnterpriseId { get; set; }

        /// <summary>
        /// User email address.
        /// </summary>
        [Required]
        public string Email { get; set; }

        /// <summary>
        /// User full name.
        /// </summary>
        [Required]
        public string FullName { get; set; }

        /// <summary>
        /// User Title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// User phone number
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Is user image available.
        /// </summary>
        public bool IsImageAvailable { get; set; }

        /// <summary>
        /// Image information of the User.
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// Indicate whether user is disabled or not.
        /// </summary>
        public bool IsDisabled { get; set; }

        /// <summary>
        /// Type  e.g. SystemGenerated or registered or manually created.
        /// </summary>
        public EnumUserType UserType { get; set; }

        /// <summary>
        ///  Status of User Record. e.g.Password is generated or reset password needed!
        /// </summary>
        public EnumUserStatus Status { get; set; }


        /// <summary>
        /// Date when user was last Enabled/Disabled.
        /// </summary>
        public DateTimeOffset? LastEnableDisable { get; set; }
        public List<RoleLookUpDto> Roles { get; set; }

        ///// <summary>
        ///// Select expression to be used to populate Dto directly from Database
        ///// </summary>
        //public static Expression<Func<User, UserDto>> ToDto = e => new UserDto
        //{
        //    Id = e.Id,
        //    Title = e.Title,
        //    Email = e.Email,
        //    DisplayName = e.DisplayName,
        //    PhoneNumber = e.PhoneNumber,
        //    IsImageAvailable = e.IsImageAvailable,
        //    IsDisabled = e.IsDisabled,
        //    UserType = e.UserType,
        //    Status = e.Status,
        //    LastEnableDisable = e.LastEnableDisable
        //};
    }

    #endregion

    #region User Profile and logged in user Dto

    /// <summary>
    /// User Dto which include user permissions detail.
    /// Used to send all permissions for currently logged in user to client application.
    /// As soon as successful login to validate allowed operations in client application base
    /// on permissions.
    /// </summary>
    public class UserWithPermissionsDto : UserDto
    {
        /// <summary>
        /// collation of all the permissions of all user roles.
        /// </summary>
        public List<PermissionDto> PermissionsList { get; set; }
    }


    /// <summary>
    /// Dot for User profile page.
    /// </summary>
    public class UserProfileDto : BaseDto
    {
        /// <summary>
        /// User Title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// User Email
        /// </summary>
        [Required]
        public string Email { get; set; }

        /// <summary>
        /// User full name
        /// </summary>
        [Required]
        public string FullName { get; set; }

        /// <summary>
        /// User phone number
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Is Image available
        /// </summary>
        public bool IsImageAvailable { get; set; }

        /// <summary>
        /// Image URL or actual image.
        /// </summary>
        public string Image { get; set; }
    }

    #endregion

    #region User specific operations related dto's

    /// <summary>
    /// Model for disable multiple users.
    /// </summary>
    public class EnableDisableMultipleUserDto
    {
        /// <summary>
        /// User ids while are to be disabled.
        /// </summary>
        public List<long> UserIds { get; set; }

        /// <summary>
        /// Indicate whether to disable or enable.
        /// </summary>
        public bool IsDisabled { get; set; }
    }


    /// <summary>
    /// User profile change request status for key fields like email or phone number
    /// if SMS is supported by application.
    /// </summary>
    public enum ChangeRequestStatus
    {
        /// <summary>
        /// Change request operation successful.
        /// </summary>
        Success,
        /// <summary>
        /// Verification code send as email or SMS to user
        /// </summary>
        VerificationCodeSent,

        /// <summary>
        /// Verification failed.
        /// </summary>
        Failed,
    }

    /// <summary>
    /// Model for profile change request status. e.g. User email address.
    /// </summary>
    public class ProfileChangeRequestStatus
    {
        public ChangeRequestStatus Status { get; set; }
    }

    /// <summary>
    /// Model send reset password email to user.
    /// </summary>
    public class ResetPasswordRequest
    {
        /// <summary>
        /// User Id for which reset password request is.
        /// </summary>
        [Required]
        public String UserId { get; set; }

        /// <summary>
        /// Callback Url of application to complete the reset password operation.
        /// </summary>
        [Required]
        public String AuthUrl { get; set; }
    }

    /// <summary>
    /// User change password Dto
    /// </summary>
    public class ChangePasswordDto
    {
        /// <summary>
        /// User id or PK
        /// </summary>
        [Required]
        public String UserId { get; set; }

        /// <summary>
        /// Old password
        /// </summary>
        [Required]
        public String OldPassword { get; set; }

        /// <summary>
        /// New password.
        /// </summary>
        [Required]
        public String NewPassword { get; set; }
    }

    /// <summary>
    /// User forgot password request model to send email to user with 
    /// code to verify and return URL of the application.
    /// </summary>
    public class ForgotPasswordRequest
    {
        /// <summary>
        /// Email address of the user. where email will be sent.
        /// </summary>
        [Required]
        public string Email { get; set; }
        /// <summary>
        /// secret code to verify before allowing actual reset of the password.
        /// </summary>
        [Required]
        public string SecretCode { get; set; }

        /// <summary>
        /// User will be redirected to Url to reset his password!
        /// </summary>
        [Required]
        public string ReturnUrl { get; set; }
    }

    /// <summary>
    /// User forgot password request model from Mobile to send email to user with 
    /// code to verify and return URL of the application.
    /// </summary>
    public class MobileForgotPasswordRequest
    {
        /// <summary>
        /// User Code to verify before sending email to user!.
        /// </summary>
        [Required]
        public string Code { get; set; }
   }
    /// <summary>
    /// Model to allow actual reset of the password.
    /// </summary>
    public class ResetPasswordDto
    {
        /// <summary>
        /// User email address
        /// </summary>
        [Required]
        public string Email { get; set; }

        /// <summary>
        /// User new password.
        /// </summary>
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// Associated code to be verified before changing the password.
        /// Which was sent to user as part of change request.
        /// </summary>
        [Required]
        public string Code { get; set; }
    }

    /// <summary>
    /// Model to allow updating user email.
    /// </summary>
    public class ChangeEmailDto
    {
        /// <summary>
        /// User id or PK
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// New email address of the user.
        /// </summary>
        [Required]
        public string Email { get; set; }

        /// <summary>
        /// Security code to be validated before changing user email address.
        /// Which was sent to user as part of change request.
        /// </summary>
        [Required]
        public string Code { get; set; }
    }

    /// <summary>
    /// Model which hold user id.
    /// </summary>
    public class UserIdDto
    {
        [Required]
        public string UserId { get; set; }
    }

    /// <summary>
    /// Dto to handle request access from any new user.
    /// Which will send email to application SMTP From email account.
    /// </summary>
    public class RequestAccessDto
    {
        /// <summary>
        /// Full name of the user.
        /// </summary>
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        /// <summary>
        /// email address of the user.
        /// </summary>
        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        /// <summary>
        /// Phone number of user.
        /// </summary>
        [Required]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"(^\+?[0-9\(][0-9\s\(\)-]{8,15}(?:x.+)?$)", ErrorMessage = "Not a valid Phone Number")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Reason for access needed.
        /// </summary>
        [Required]
        [Display(Name = "Reason for Access")]
        public string ReasonForAccess { get; set; }

        /// <summary>
        /// secret code to verify before sending email from system/application.
        /// </summary>
        [Required]
        [Display(Name = "Secret Code")]
        public string SecretCode { get; set; }

        /// <summary>
        /// Callback URL in email to give access to requested user manually by administrator.
        /// </summary>
        [Required]
        public string CallbackUrl { get; set; }
    }

    #endregion

}
