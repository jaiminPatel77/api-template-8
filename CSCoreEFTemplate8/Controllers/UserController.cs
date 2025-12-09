using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConfidoSoft.Data.Domain.Consts;
using ConfidoSoft.Data.Domain.DBModels;
using ConfidoSoft.Data.Domain.Dtos;
using ConfidoSoft.Data.Services.DataQuery;
using ConfidoSoft.Data.Services.DBServices;
using ConfidoSoft.Infrastructure.Extensions;
using CSCoreEFTemplate8.Auth;
using CSCoreEFTemplate8.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CSCoreEFTemplate8.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger _logger;

        public UserController(IUserService userService, 
            ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        #region CURD API

        // GET: api/User
        [HttpGet]
        //IEnumerable<CSUser>
        [CustomAuthorize(EnumPermissionFor.USER, EnumPermissions.ViewAccess)]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation("Calling the GetUsers action");
                }
                DataQueryOptions dataQueryOptions = DataQueryOptionsHelper.FillDataQueryOptions();
                var list = await _userService.GetFilteredDtoRecords(dataQueryOptions);
                return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.COMMON_LIST_ALL_ITEMS, list);
            }
            catch(Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_LIST_EXCEPTION, ex, _logger);
            }
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        [CustomAuthorize(EnumPermissionFor.USER, EnumPermissions.ViewAccess)]
        public async Task<IActionResult> Get(long id)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the Get action, {id}");
                }
                var record = await _userService.GetDtoRecord(id);
                if (record == null)
                {
                    return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_GET_ITEM_NOTFOUND, _logger);
                }
                return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.COMMON_GET_ITEM, record);
            }
            catch(Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_GET_EXCEPTION, ex, _logger);
            }
        }

        // POST: api/User
        [CustomAuthorize(EnumPermissionFor.USER, EnumPermissions.CreateAccess)]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]UserDto recordDto)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the Post action, {recordDto.Email}");
                }
                if (!this.ModelState.IsValid)
                {
                    return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_CREATE_ITEM, ModelState, _logger);
                }
                recordDto = await _userService.Create(recordDto);
                var url = this.Url.Action("Get", new { id = recordDto.Id });
                return this.CreatedResponse(EnumEntityType.USER, EnumEntityEvents.COMMON_CREATE_ITEM, recordDto, url);
            }
            catch(Exception ex)
            {              
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_CREATE_EXCEPTION, ex, _logger);
            }
        }

        // PUT: api/User/5
        [CustomAuthorize(EnumPermissionFor.USER, EnumPermissions.UpdateAccess)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody]UserDto recordDto)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the Put action, {recordDto.Email}");
                }
                if (!this.ModelState.IsValid)
                {
                    return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_PUT_EXCEPTION, ModelState, _logger);
                }
                await _userService.Update(recordDto);
                return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.COMMON_UPDATE_ITEM, recordDto);
            }
            catch(Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_PUT_EXCEPTION, ex, _logger);
            }
        }

        // DELETE: api/ApiWithActions/5
        [CustomAuthorize(EnumPermissionFor.USER, EnumPermissions.DeleteAccess)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the Delete action, {id}");
                }
                var dbRecord = await _userService.Remove(id);
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Deleted, {dbRecord.Email}");
                }
                return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.COMMON_DELETE_ITEM, true);
            }
            catch(Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_DELETE_EXCEPTION, ex, _logger);
            }
        }

        #endregion

        #region User with Permission detail

        /// <summary>
        /// Allowed to logged in user to get detail for his permission
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("user-with-permissions/{id}")]
        public async Task<IActionResult> GetUserWithPermissions(long id)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the GetUserWithPermissions action, {id}");
                }
                var record = await _userService.GetDtoWithPermissionsRecord(id);
                if (record == null)
                {
                    return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_GET_ITEM_NOTFOUND, _logger);
                }
                return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.COMMON_GET_ITEM, record);
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_GET_EXCEPTION, ex, _logger);
            }
        }
        #endregion

        #region Lookup List  & record

        [HttpGet("lookup-user/{id}")]
        [CustomAuthorize(EnumPermissionFor.USER, EnumPermissions.ViewAccess)]
        public async Task<IActionResult> GetUserLookUpRecord(long id)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the GetUserLookUpRecord action, { id }");
                }
                var record = await _userService.GetLookUpDtoRecord(id);

                if (record == null)
                {
                    return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_GET_ITEM_NOTFOUND, _logger);
                }
                return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.COMMON_GET_ITEM, record);
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_LIST_EXCEPTION, ex, _logger);
            }
        }

        [HttpGet("lookup-list")]
        public async Task<IActionResult> LookUpList(string filter = null, string orderBy = null,
            int pageNo = 1, int size = -1)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the LookUpList action, {filter}");
                }
                var dataQueryOption = DataQueryOptionsHelper.FillDataQueryOptions(filter, orderBy, pageNo, size);
                var list = await _userService.GetLookUpDtoRecords(dataQueryOption);
                return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.COMMON_LIST_FILTER_ITEMS, list);
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_LIST_EXCEPTION, ex, _logger);
            }
        }

        [HttpGet("rolebased-lookup-list/{roleid}")]
        public async Task<IActionResult> RoleBasedLookUpList(int roleid)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the RoleBasedLookUpList action, {roleid}");
                }
               
                var list = await _userService.GetLookUpDtoRecordsUserType((EnumUserType)roleid);
                return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.COMMON_LIST_FILTER_ITEMS, list);
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_LIST_EXCEPTION, ex, _logger);
            }
        }
        #endregion

        #region Filter List
        [HttpGet("filter-list")]
        [CustomAuthorize(EnumPermissionFor.USER, EnumPermissions.ViewAccess)]
        public async Task<IActionResult> FilterList(string filter = null, string orderBy = null,
            int pageNo = 1, int size = 10)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the filterList action, {filter}");
                }
                var dataQueryOption = DataQueryOptionsHelper.FillDataQueryOptions(filter, orderBy, pageNo, size);
                var list = await _userService.GetFilteredDtoRecords(dataQueryOption);
                //Thread.Sleep(5000); //wait for 2 sec..
                return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.COMMON_LIST_FILTER_ITEMS, list);
            }
            catch(Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_LIST_EXCEPTION, ex, _logger);
            }
        }
        #endregion

        #region Profile Update 

        [HttpGet("user-profile/{id}")]
        public async Task<IActionResult> GetUserProfile(long id)
        {
            UserProfileDto dtoRecord = null;
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the GetUserProfile action, {id}");
                }
                dtoRecord = await this._userService.GetUserProfile(id);
                if (dtoRecord != null)
                {
                    return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.COMMON_GET_ITEM, dtoRecord);
                }
                else
                {
                    return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_GET_ITEM_NOTFOUND, _logger);
                }
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_GET_EXCEPTION, ex, _logger);
            }
        }

        [HttpPatch("user-profile")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UserProfileDto dtoRecord)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the UpdateUserProfile action, {dtoRecord.Email}");
                }
                if (!this.ModelState.IsValid)
                {
                    return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_PUT_EXCEPTION, ModelState, _logger);
                }
                dtoRecord = await this._userService.UpdateUserProfile(dtoRecord);
                if (dtoRecord == null)
                {
                    return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.COMMON_GET_ITEM, dtoRecord);
                }
                return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.COMMON_UPDATE_ITEM, dtoRecord);
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_PUT_EXCEPTION, ex, _logger);
            }
        }

        [HttpPatch("change-password")]
        public async Task<IActionResult> ChangeUserPassword([FromBody] ChangePasswordDto dtoRecord)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the ChangeUserPassword action, {dtoRecord.UserId}");
                }
                if (!this.ModelState.IsValid)
                {
                    return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_INVALID_PASSWORD, ModelState, _logger);
                }
                var isSuccess = await this._userService.ChangeUserPassword(dtoRecord);
                return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_RESET_PASSWORD, isSuccess);
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_RESET_PASSWORD, ex, _logger);
            }
        }

        [HttpGet("image/{id}")]
        public async Task<IActionResult> GetImage(long id)
        {
            var imageUri = String.Empty;
            try
            {
                imageUri = await this._userService.GetImage(id);
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_GET_EXCEPTION, ex, _logger);
            }
            return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.COMMON_GET_ITEM, imageUri);
        }
        #endregion

        #region Reset Password APIs

        [CustomAuthorize(EnumPermissionFor.USER, EnumPermissions.UpdateAccess)]
        [HttpPost("admin-reset-password")]
        public async Task<IActionResult> AdminResetPassword([FromBody] ResetPasswordRequest dtoRecord)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the AdminResetPassword action, {dtoRecord.UserId}");
                }

                if (!this.ModelState.IsValid)
                {
                    return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_CHANGE_PASSWORD_FAILED, ModelState, _logger);
                }
                var isSuccess = await this._userService.AdminResetPassword(dtoRecord);                
                return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_RESET_PASSWORD, isSuccess);
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_CHANGE_PASSWORD_FAILED, ex, _logger);
            }
        }

        [CustomAuthorize(EnumPermissionFor.USER, EnumPermissions.UpdateAccess)]
        [HttpPost("admin-invite-user")]
        public async Task<IActionResult> AdminInviteUser([FromBody] ResetPasswordRequest dtoRecord)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the AdminResetPassword action, {dtoRecord.UserId}");
                }

                if (!this.ModelState.IsValid)
                {
                    return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_INVITE_USER_FAILED, ModelState, _logger);
                }
                var isSuccess = await this._userService.AdminInviteUser(dtoRecord);

                return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_INVITE_USER, isSuccess);
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_INVITE_USER_FAILED, ex, _logger);
            }
        }
        #endregion

        #region Update Status

        [HttpPatch("enable-disable-multiple")]
        public async Task<IActionResult> EnableDisableMultiple([FromBody] EnableDisableMultipleUserDto dtoRecord)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation("Calling the EnableDisableMultiple action");
                }
                if (!this.ModelState.IsValid)
                {
                    return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_UPDATE_ITEM, ModelState, _logger);
                }
                var success = await this._userService.EnableDisableMultiple(dtoRecord);
                return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.COMMON_UPDATE_ITEM, success);
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_UPDATE_ITEM, ex, _logger);
            }
        }

        #endregion

        #region Update Email address

        [HttpPatch("change-email-request")]
        public async Task<IActionResult> ChangeEmailRequest([FromBody] ChangeEmailDto changeEmailDto)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the ChangeEmailRequest action, {changeEmailDto.UserId}>{changeEmailDto.Email}");
                }

                if (!this.ModelState.IsValid)
                {
                    return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_EMAIL_VERIFICATION_FAILED, ModelState, _logger);
                }

                var requestStatus = await this._userService.ChangeEmailRequest(changeEmailDto);
                if (requestStatus != null)
                {
                    return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_EMAIL_VERIFICATION_SENT, requestStatus);
                }
                else
                {
                    return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_GET_ITEM_NOTFOUND, _logger);
                }
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_EMAIL_VERIFICATION_FAILED, ex, _logger);
            }
        }

        [HttpPatch("change-email")]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailDto changeEmailDto)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the ChangeEmail action, {changeEmailDto.UserId}>{changeEmailDto.Email}");
                }
                if (!this.ModelState.IsValid)
                {
                    return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_EMAIL_VERIFICATION_FAILED, ModelState, _logger);
                }

                var requestStatus = await this._userService.ChangeEmail(changeEmailDto);
                if (requestStatus != null)
                {
                    if (requestStatus.Status == ChangeRequestStatus.Success)
                    {
                        return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_EMAIL_VERIFICATION_SUCCESS, requestStatus);
                    }
                    else
                    {
                        return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_EMAIL_VERIFICATION_SUCCESS, requestStatus);
                    }                    
                }
                else
                {
                    return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_EMAIL_VERIFICATION_FAILED, _logger);
                }

            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_EMAIL_VERIFICATION_FAILED, ex, _logger);
            }
        }

        #endregion

    }
}
