using AutoMapper;
using ConfidoSoft.Data.Domain.Consts;
using ConfidoSoft.Data.Domain.Database;
using ConfidoSoft.Data.Domain.DBModels;
using ConfidoSoft.Data.Domain.Dtos;
using ConfidoSoft.Data.Services.BLServices;
using ConfidoSoft.Data.Services.DataQuery;
using ConfidoSoft.Data.Services.Helpers;
using ConfidoSoft.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ConfidoSoft.Data.Services.DBServices
{
    #region IUserService interface

    /// <summary>
    /// User Entity Service Interface
    /// </summary>
    public interface IUserService : IBaseService<User>
    {
        //User specific methods go here!
        Task<DataQueryResult<UserListDto>> GetFilteredDtoRecords(DataQueryOptions dataQueryOptions);
        Task<UserDto> GetDtoRecord(long id);      
        Task<UserDto> Create(UserDto dtoRecord);
        Task<UserDto> Update(UserDto dtoRecord);
        Task<UserWithPermissionsDto> GetDtoWithPermissionsRecord(long id);

        //Lookup API
        Task<DataQueryResult<UserLookUpDto>> GetLookUpDtoRecords(DataQueryOptions dataQueryOptions);
        Task<UserLookUpDto> GetLookUpDtoRecord(long id);

        //Manage user specific operations
        Task<UserProfileDto> GetUserProfile(long id);
        Task<UserProfileDto> UpdateUserProfile(UserProfileDto dtoRecord);
        Task<string> GetImage(long userId);
        Task<bool> EnableDisableMultiple(EnableDisableMultipleUserDto dtoRecord);
        Task<bool> RequestAccess(RequestAccessDto requestAccessDto);
        Task<bool> ForgotPassword(ForgotPasswordRequest forgotPassword);
        Task<bool> ResetPassword(ResetPasswordDto resetPasswordDto);
        Task<bool> ChangeUserPassword(ChangePasswordDto changePasswordDto);
        Task<bool> AdminResetPassword(ResetPasswordRequest resetPasswordRequest);
        Task<bool> AdminInviteUser(ResetPasswordRequest resetPasswordRequest);
        Task<ProfileChangeRequestStatus> ChangeEmailRequest(ChangeEmailDto changeEmailDto);
        Task<ProfileChangeRequestStatus> ChangeEmail(ChangeEmailDto changeEmailDto);
        Task<List<UserLookUpDto>> GetLookUpDtoRecordsUserType(EnumUserType userType);
    }

    #endregion

    #region UserService class

    /// <summary>
    /// User Entity Service interface.
    /// </summary>
    public class UserService : BaseService<User>, IUserService
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly UrlEncoder _urlEncoder;
        private readonly IEmailSender _emailSender;

        #region ctor
        public UserService(ApplicationDbContext dbContext,
            ILogger<UserService> logger,
            ICurrentUserInfo currentUserInfo,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IEmailSender emailSender,
            UrlEncoder urlEncoder,
            IMapper mapper) : base(logger, dbContext, currentUserInfo, mapper)
        {
            this._roleManager = roleManager;
            this._userManager = userManager;
            this._urlEncoder = urlEncoder;
            this._emailSender = emailSender;
        }
        #endregion

        #region Entity Specific Methods

        public async Task<DataQueryResult<UserListDto>> GetFilteredDtoRecords(DataQueryOptions dataQueryOptions)
        {
            DataQueryResult<UserListDto> result = new DataQueryResult<UserListDto>();
            var userListResult = await base.GetFilteredRecords(dataQueryOptions);
            List<UserListDto> dtoList = new List<UserListDto>();
            foreach ( var user in userListResult.Items)
            {
                var userDto = _mapper.Map<User, UserListDto>(user);
                userDto.IsLockedOut = await this._userManager.IsLockedOutAsync(user);                
                //userDto.Roles = await this._userManager.GetRolesAsync(user);
                dtoList.Add(userDto);
            }
            result.Items = dtoList;
            result.PageNo = userListResult.PageNo;
            result.Size = userListResult.Size;
            result.TotalRecords = userListResult.TotalRecords;
            return result;
        }


        public async Task<DataQueryResult<UserLookUpDto>> GetLookUpDtoRecords(DataQueryOptions dataQueryOptions)
        {            
            var listResult = await base.GetFilteredRecords<UserLookUpDto>(dataQueryOptions, UserLookUpDto.ToDto);
            return listResult;
        }

        public async Task<List<UserLookUpDto>> GetLookUpDtoRecordsUserType(EnumUserType userType)
        {
            var users = await this._dbContext.Users.Where(e => e.UserType ==userType).Select(UserLookUpDto.ToDto).ToListAsync();
            if (users != null)
            {              
                return users;
            }
            else
            {
                throw new CSApplicationException(EnumEntityEvents.COMMON_GET_ITEM_NOTFOUND.ToString(), CommonConstStr.COMMON_ITEM_NOTFOUND);
            }
        }


        public async Task<UserLookUpDto> GetLookUpDtoRecord(long id)
        {
            var user = await this.DbSet.FindAsync(id);
            if (user != null)
            {
                var dtoRecord = this._mapper.Map<User, UserLookUpDto>(user);               
                return dtoRecord;
            }
            else
            {
                throw new CSApplicationException(EnumEntityEvents.COMMON_GET_ITEM_NOTFOUND.ToString(), CommonConstStr.COMMON_ITEM_NOTFOUND);
            }
        }
        #endregion

        #region Must override methods

        /// <summary>
        /// Must override to add base filter for all table query!..
        /// Added filter to return Tenant related records and not system generated records.
        /// </summary>
        /// <returns></returns>
        protected override IQueryable<User> AddBaseFilterExpression(IQueryable<User> filterExp)
        {
            return filterExp.Where(r => (r.UserType != EnumUserType.GlobalAdministrator));
        }

        #endregion

        #region Dto version of CURD

        public async Task<UserDto> GetDtoRecord(long id)
        {
            var user = await this.Table.Where(e => e.Id == id).Include(b => b.UserProfile).FirstOrDefaultAsync();
            if (user != null)
            {
                var dtoRecord = this._mapper.Map<User, UserDto>(user);

                var userRoles = await this._dbContext.Roles.Where(e => (e.UserRoles.Any(ur => ur.UserId == user.Id))).Select(RoleLookUpDto.ToDto).ToListAsync();
                //List<RoleLookUpDto> userRoles = new List<RoleLookUpDto>();
                dtoRecord.Roles = userRoles;
                //var roles = await this._userManager.GetRolesAsync(user);
                //foreach (var role in roles)
                //{
                //    var r = await this._roleManager.FindByNameAsync(role);
                //    userRoles.Add(new RoleLookUpDto
                //    {
                //        Id = r.Id,
                //        Name = r.Name,
                //        RoleType = r.RoleType
                //        //Description = r.Description
                //    });
                //}
                //Get Image data.
                if (user.UserProfile != null)
                {
                    dtoRecord.Image = user.UserProfile.Image;
                }
                return dtoRecord;
            }
            else
            {
                throw new CSApplicationException(EnumEntityEvents.COMMON_GET_ITEM_NOTFOUND.ToString(), CommonConstStr.COMMON_ITEM_NOTFOUND);
            }
        }


        public async Task<UserWithPermissionsDto> GetDtoWithPermissionsRecord(long id)
        {
            var user = await this.Table.Where(e => e.Id == id).Include(b => b.UserProfile).FirstOrDefaultAsync();
            if (user != null)
            {
                var dtoRecord = this._mapper.Map<User, UserWithPermissionsDto>(user);

                List<RoleLookUpDto> userRoles = new List<RoleLookUpDto>();
                dtoRecord.Roles = userRoles;
                var roles = await this._userManager.GetRolesAsync(user);
                List<long> roleIds = new List<long>();
                foreach (var role in roles)
                {
                    var r = await this._roleManager.FindByNameAsync(role);
                    if (r != null)
                    {
                        userRoles.Add(new RoleLookUpDto
                        {
                            Id = r.Id,
                            Name = r.Name,
                            RoleType = r.RoleType
                            //Description = r.Description
                        });
                        roleIds.Add(r.Id);
                    }
                }
                //Get the permission detail!
                Dictionary<EnumPermissionFor, PermissionDto> allPermission = new Dictionary<EnumPermissionFor, PermissionDto>();
                PermissionDto rolePermissionDto = null;
                foreach (EnumPermissionFor entityType in Enum.GetValues(typeof(EnumPermissionFor)))
                {
                    rolePermissionDto = new PermissionDto
                    {
                        PermissionFor = entityType
                    };
                    allPermission.Add(entityType, rolePermissionDto);
                }
                //Update permission with values in db!
                var permisionList = _dbContext.RolePermissions.Where(e => roleIds.Contains(e.RoleId)).ToList();
                foreach( var rolePermission in permisionList)
                {
                    rolePermissionDto = null;
                    if (allPermission.ContainsKey(rolePermission.PermissionFor))
                    {
                        rolePermissionDto = allPermission[rolePermission.PermissionFor];
                    }
                    if(rolePermissionDto != null)
                    {
                        PermissionDto.MargePermission(rolePermission, rolePermissionDto);
                    }
                }
                dtoRecord.PermissionsList = allPermission.Values.ToList();

                //Get Image data.
                if (user.UserProfile != null)
                {
                    dtoRecord.Image = user.UserProfile.Image;
                }
                return dtoRecord;
            }
            else
            {
                throw new CSApplicationException(EnumEntityEvents.COMMON_GET_ITEM_NOTFOUND.ToString(), CommonConstStr.COMMON_ITEM_NOTFOUND);
            }
        }



        //We should not allowed until we allow to set the password using Dto!!!
        public async Task<UserDto> Create(UserDto dtoRecord)
        {
            var dbRecord = _mapper.Map<UserDto, User>(dtoRecord);
            SetRecordCreatedInfo(dbRecord);
            SetRecordModifiedInfo(dbRecord);
            var result = await this._userManager.CreateAsync(dbRecord);
            if (result.Succeeded)
            {
                if (dtoRecord.Roles != null && dtoRecord.Roles.Count > 0)
                {
                    var roleNames = dtoRecord.Roles.Select(e => e.Name);
                    var roleResult = await this._userManager.AddToRolesAsync(dbRecord, roleNames);
                    if (!roleResult.Succeeded)
                    {
                        await this._userManager.DeleteAsync(dbRecord);
                        var errorDetail = roleResult.ToErrorMessage();
                        throw new Exception(errorDetail);
                    }
                }
                _mapper.Map<User, UserDto>(dbRecord, dtoRecord);
            }
            else
            {
                var errorDetail = result.ToErrorMessage();
                throw new Exception(errorDetail);
            }
            return dtoRecord;
        }

        public async Task<UserDto> Update(UserDto dtoRecord)
        {
            var dbRecord = await this.Table.Where(e => e.Id == dtoRecord.Id).Include(b => b.UserProfile).FirstOrDefaultAsync();
            if (dbRecord != null)
            {
                if ((dtoRecord.Image != null) && (dtoRecord.Image.Length > 0))
                {
                    dtoRecord.IsImageAvailable = true;
                }
                else
                {
                    dtoRecord.IsImageAvailable = false;
                }                
                this.PreProcessingOnRecordUpdate(dbRecord, dtoRecord);
                _mapper.Map<UserDto, User>(dtoRecord, dbRecord);
                var result = await this._userManager.UpdateAsync(dbRecord);
                if (result.Succeeded)
                {
                    var userRoles = await this._userManager.GetRolesAsync(dbRecord);
                    foreach (var role in dtoRecord.Roles)
                    {
                        if (userRoles.Contains(role.Name))
                        {
                            //alread exists so do nothing..
                        }
                        else
                        {
                            await this._userManager.AddToRoleAsync(dbRecord, role.Name);
                        }
                        userRoles.Remove(role.Name);
                    }
                    foreach (var remainRole in userRoles)
                    {
                        await this._userManager.RemoveFromRoleAsync(dbRecord, remainRole);
                    }

                    //update user image/profile detail.
                    var userProfile = _dbContext.UserProfiles.Where(e => e.Id == dbRecord.Id).FirstOrDefault();
                    if (userProfile != null)
                    {
                        userProfile.Image = dtoRecord.Image;
                    }
                    else
                    {
                        if ((dtoRecord.Image != null) && (dtoRecord.Image.Length > 0))
                        {
                            var newProfile = new UserProfile
                            {
                                Id = dbRecord.Id,
                                Image = dtoRecord.Image
                            };
                            _dbContext.UserProfiles.Add(newProfile);
                        }
                    }
                    await this.SaveChanges();
                    return dtoRecord;
                }
                else
                {
                    var errorDetail = result.ToErrorMessage();
                    throw new Exception(errorDetail);
                }
            }
            else
            {
                throw new CSApplicationException(EnumEntityEvents.COMMON_UPDATE_ITEM_NOTFOUND.ToString(), CommonConstStr.COMMON_ITEM_NOTFOUND);
            }
        }

        #endregion

        #region user profile related functions.

        public async Task<UserProfileDto> GetUserProfile(long id)
        {
            var user = await this.Table.Where(e => e.Id == id).Include(b => b.UserProfile).FirstOrDefaultAsync();
            if (user != null)
            {
                var dtoRecord = this._mapper.Map<User, UserProfileDto>(user);
                //Get Image data.
                if (user.UserProfile != null)
                {
                    dtoRecord.Image = user.UserProfile.Image;
                }
                return dtoRecord;
            }
            else
            {
                return null;
            }
        }

        public async Task<UserProfileDto> UpdateUserProfile(UserProfileDto dtoRecord)
        {
            var dbRecord = await this.Table.Where(e => e.Id == dtoRecord.Id).Include(b => b.UserProfile).FirstOrDefaultAsync();
            if (dbRecord != null)
            {
                if ((dtoRecord.Image != null) && (dtoRecord.Image.Length > 0))
                {
                    dtoRecord.IsImageAvailable = true;
                }
                else
                {
                    dtoRecord.IsImageAvailable = false;
                }
                this.PreProcessingOnRecordUpdate(dbRecord, dtoRecord);
                _mapper.Map<UserProfileDto, User>(dtoRecord, dbRecord);
                var result = await this._userManager.UpdateAsync(dbRecord);
                if (result.Succeeded)
                {
                    //update user image/profile detail.
                    var userProfile = _dbContext.UserProfiles.Where(e => e.Id == dbRecord.Id).FirstOrDefault();
                    if (userProfile != null)
                    {
                        userProfile.Image = dtoRecord.Image;
                    }
                    else
                    {
                        if ((dtoRecord.Image != null) && (dtoRecord.Image.Length > 0))
                        {
                            var newProfile = new UserProfile
                            {
                                Id = dbRecord.Id,
                                Image = dtoRecord.Image
                            };
                            _dbContext.UserProfiles.Add(newProfile);
                        }
                    }
                    await this.SaveChanges();
                    return dtoRecord;
                }
            }
            return null; //retun null in case of error!
        }
        
        public async Task<string> GetImage(long userId)
        {
            var imageUri = String.Empty;//"assets/avatar_green.svg?v=1.2";
            try
            {
                var userProfile = await this._dbContext.UserProfiles.Where(e => e.Id == userId).FirstOrDefaultAsync();
                if (userProfile != null)
                {
                    if (false == String.IsNullOrEmpty(userProfile.Image))
                    {
                        imageUri = userProfile.Image;
                    }
                }
            }
            catch
            {
                //do nothing..
            }
            return imageUri;
        }

        #endregion

        #region Manage User operations

        /// <summary>
        /// Enable or disable multiple users.
        /// </summary>
        /// <param name="dtoRecord"></param>
        /// <returns></returns>
        public async Task<bool> EnableDisableMultiple(EnableDisableMultipleUserDto dtoRecord)
        {
            var users = this._dbContext.Users.Where(e => dtoRecord.UserIds.Contains(e.Id) && e.Disabled != dtoRecord.IsDisabled).ToList();
            if (users.Count > 0)
            {
                foreach (var user in users)
                {
                    user.EnabledDisabledOn = DateTimeOffset.UtcNow;
                    user.Disabled = dtoRecord.IsDisabled;
                }
                await this._dbContext.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> RequestAccess(RequestAccessDto requestAccessDto)
        {
            bool success = true;
            await _emailSender.SendAccessRequestEmailAsync(requestAccessDto);
            return success;
        }

        public async Task<bool> ForgotPassword(ForgotPasswordRequest forgotPassword)
        {
            bool success = true;
            var user = await this._userManager.FindByEmailAsync(forgotPassword.Email);
            if (user != null)
            {
                var codeOrg = await this._userManager.GeneratePasswordResetTokenAsync(user);
                var code = WebUtility.UrlEncode(codeOrg);
                var callbackUrl = forgotPassword.ReturnUrl + $"?code={code}";
                await _emailSender.SendUserResetPasswordEmailAsync(user, callbackUrl);
            }
            return success;
        }

        public async Task<bool> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            bool success = true;
            var user = await this._userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user != null)
            {
                ValidatePassword(user, resetPasswordDto.Password);
                if (user.Status != EnumUserStatus.PasswordSetResetCompleted)
                {
                    user.Status = EnumUserStatus.PasswordSetResetCompleted;
                }
                var result = await this._userManager.ResetPasswordAsync(user, resetPasswordDto.Code, resetPasswordDto.Password);
                if (!result.Succeeded)
                {
                    var errorDetail = result.ToErrorMessage();
                    throw new Exception(errorDetail);
                }
//                if (user.Status != EnumUserStatus.PasswordSetResetCompleted)
//                {
//                    user.Status = EnumUserStatus.PasswordSetResetCompleted;
//                }
            }
            return success;
        }

        public async Task<bool> ChangeUserPassword(ChangePasswordDto changePasswordDto)
        {
            bool isSuccess = true;
            var user = await this._userManager.FindByIdAsync(changePasswordDto.UserId);
            if (user != null && await this._userManager.CheckPasswordAsync(user, changePasswordDto.OldPassword))
            {
                ValidatePassword(user, changePasswordDto.NewPassword);
                var code = await this._userManager.GeneratePasswordResetTokenAsync(user);
                var result = await this._userManager.ResetPasswordAsync(user, code, changePasswordDto.NewPassword);
                if (!result.Succeeded)
                {
                    var errorDetail = result.ToErrorMessage();
                    throw new Exception(errorDetail);
                }
            }
            else
            {
                throw new CSApplicationException(EnumEntityEvents.USER_INVALID_PASSWORD.ToString(), "Invalid password");
            }
            return isSuccess;
        }

        public async Task<bool> AdminResetPassword(ResetPasswordRequest resetPasswordRequest)
        {
            bool isSuccess = true;
            var user = await this._userManager.FindByIdAsync(resetPasswordRequest.UserId);
            if (user != null)
            {
                if (user.LockoutEnd != null)
                {
                    bool isLockout = await this._userManager.IsLockedOutAsync(user);
                    if (isLockout)
                    {
                        var result = await this._userManager.SetLockoutEndDateAsync(user, null);
                        if (!result.Succeeded)
                        {
                            var errorDetail = result.ToErrorMessage();
                            throw new Exception(errorDetail);
                        }
                    }
                }
                var code = await this._userManager.GeneratePasswordResetTokenAsync(user);
                var encodedCode = this._urlEncoder.Encode(code);                
                var retUrl = this._urlEncoder.Encode(String.Empty);
                var callbackUrl = $"{resetPasswordRequest.AuthUrl}?code={encodedCode}";
                var formUser = await this._userManager.FindByIdAsync(this._currentUserInfo.UserId);
                await this._emailSender.SendUserAdminResetPasswordEmailAsync(user, formUser, callbackUrl);
                if (user.Status != EnumUserStatus.AdminResetPassword)
                {
                    user.Status = EnumUserStatus.AdminResetPassword;
                    var result = await this._userManager.UpdateAsync(user);
                    if (!result.Succeeded)
                    {
                        var errorDetail = result.ToErrorMessage();
                        throw new Exception(errorDetail);
                    }
                }
            }
            else
            {
                throw new CSApplicationException(EnumEntityEvents.COMMON_GET_ITEM_NOTFOUND.ToString(), "User Record not found!");
            }
            return isSuccess;
        }

        public async Task<bool> AdminInviteUser(ResetPasswordRequest resetPasswordRequest)
        {
            bool isSuccess = true;
            var user = await this._userManager.FindByIdAsync(resetPasswordRequest.UserId);
            if (user != null)
            {
                if (user.LockoutEnd != null)
                {
                    bool isLockout = await this._userManager.IsLockedOutAsync(user);
                    if (isLockout)
                    {
                        var result = await this._userManager.SetLockoutEndDateAsync(user, null);
                        if (!result.Succeeded)
                        {
                            var errorDetail = result.ToErrorMessage();
                            throw new Exception(errorDetail);
                        }
                    }
                }
                string redirectUrl = String.Empty;
                string callbackUrl = String.Empty;

                var code = await this._userManager.GeneratePasswordResetTokenAsync(user);
                var encodedCode = this._urlEncoder.Encode(code);                
                callbackUrl = $"{resetPasswordRequest.AuthUrl}?code={encodedCode}";

                var formUser = await this._userManager.FindByIdAsync(this._currentUserInfo.UserId);
                await this._emailSender.SendUserInvitationEmailAsync(user, formUser, callbackUrl);
                if (user.Status == EnumUserStatus.Created)
                {
                    user.Status = EnumUserStatus.Invited;
                    var result = await this._userManager.UpdateAsync(user);
                    if (!result.Succeeded)
                    {
                        var errorDetail = result.ToErrorMessage();
                        throw new Exception(errorDetail);
                    }
                }
            }
            else
            {
                throw new CSApplicationException(EnumEntityEvents.COMMON_GET_ITEM_NOTFOUND.ToString(), "User Record not found!");
            }
            return isSuccess;
        }
        
        private void ValidatePassword(User user, string newPassword)
        {
            bool isPasswordValid = true;
            int canRepeatSamePasswordAfter = 4;

            if (canRepeatSamePasswordAfter <= 0)
            {
                return;
            }
            else
            {
                //check for first time user password..
                if (String.IsNullOrEmpty(user.PasswordHash))
                {
                    return;
                }
                //this.userManager
                //Validate the password with old password.
                var userPasswordValidator = new PasswordHasher<User>();
                var currentHashPassword = user.PasswordHash;
                PasswordVerificationResult passwordResult = userPasswordValidator.VerifyHashedPassword(user, currentHashPassword, newPassword);
                if (passwordResult != PasswordVerificationResult.Failed)
                {
                    isPasswordValid = false;
                }
                else
                {
                    if (false == String.IsNullOrEmpty(user.PreviousPassword1) && (canRepeatSamePasswordAfter > 1))
                    {
                        passwordResult = userPasswordValidator.VerifyHashedPassword(user, user.PreviousPassword1, newPassword);
                        if (passwordResult != PasswordVerificationResult.Failed)
                        {
                            isPasswordValid = false;
                        }
                        else
                        {
                            if (false == String.IsNullOrEmpty(user.PreviousPassword2) && (canRepeatSamePasswordAfter > 2))
                            {
                                passwordResult = userPasswordValidator.VerifyHashedPassword(user, user.PreviousPassword2, newPassword);
                                if (passwordResult != PasswordVerificationResult.Failed)
                                {
                                    isPasswordValid = false;
                                }
                                else
                                {
                                    //check the third password.
                                    if (false == String.IsNullOrEmpty(user.PreviousPassword3) && (canRepeatSamePasswordAfter > 3))
                                    {
                                        passwordResult = userPasswordValidator.VerifyHashedPassword(user, user.PreviousPassword3, newPassword);
                                        if (passwordResult != PasswordVerificationResult.Failed)
                                        {
                                            isPasswordValid = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (isPasswordValid == false)
                {
                    String errorMessage = string.Format("Password must be different from last {0} passwords", canRepeatSamePasswordAfter);
                    throw new CSApplicationException(EnumEntityEvents.USER_LAST4_PASSWORD_FAILED.ToString(), errorMessage);                    
                }
                else
                {
                    //Update password detail here.
                    //user.PreviousPassword3 = user.PreviousPassword2;
                    if (canRepeatSamePasswordAfter >= 4)
                    {
                        user.PreviousPassword3 = user.PreviousPassword2;
                    }
                    if (canRepeatSamePasswordAfter >= 3)
                    {
                        user.PreviousPassword2 = user.PreviousPassword1;
                    }
                    if (canRepeatSamePasswordAfter >= 2)
                    {
                        user.PreviousPassword1 = currentHashPassword;
                    }
                    user.PreviousPasswordDate = DateTimeOffset.UtcNow;
                }
            }
        }

        public async Task<ProfileChangeRequestStatus> ChangeEmailRequest(ChangeEmailDto changeEmailDto)
        {
            var user = await this._userManager.FindByIdAsync(changeEmailDto.UserId);
            if (user != null)
            {
                ProfileChangeRequestStatus responce = new ProfileChangeRequestStatus();
                string code = await this._userManager.GenerateChangeEmailTokenAsync(user, changeEmailDto.Email);
                await this._emailSender.SendChangeEmailAddressEmailAsync(user, changeEmailDto.Email, code);
                responce.Status = ChangeRequestStatus.VerificationCodeSent;
                return responce;
            }
            else
            {
                return null;
            }
        }

        public async Task<ProfileChangeRequestStatus> ChangeEmail(ChangeEmailDto changeEmailDto)
        {
            var user = await this._userManager.FindByIdAsync(changeEmailDto.UserId);
            if (user != null)
            {
                ProfileChangeRequestStatus responce = new ProfileChangeRequestStatus();
                var result = await this._userManager.ChangeEmailAsync(user, changeEmailDto.Email, changeEmailDto.Code);
                if (!result.Succeeded)
                {
                    responce.Status = ChangeRequestStatus.Failed;
                }
                else
                {
                    responce.Status = ChangeRequestStatus.Success;
                }
                return responce;
            }
            return null;
        }

        #endregion
    }

    #endregion

}