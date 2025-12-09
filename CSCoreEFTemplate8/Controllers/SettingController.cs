using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConfidoSoft.Data.Domain.Consts;
using ConfidoSoft.Data.Domain.DBModels;
using ConfidoSoft.Data.Domain.DBModels.Settings;
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
    public class SettingController : Controller
    {
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;

        public SettingController(ISettingService personService, 
            ILogger<SettingController> logger)
        {
            _settingService = personService;
            _logger = logger;
        }

        #region CURD API
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        [CustomAuthorize(EnumPermissionFor.SETTING, EnumPermissions.DeleteAccess)]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the Delete action, {id}");
                }
                var dbRecord = await _settingService.Remove(id);
                if (dbRecord == null)
                {
                    return this.CreateBadRequest(EnumEntityType.SETTING, EnumEntityEvents.COMMON_DELETE_ITEM_NOTFOUND, _logger);
                }
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Deleted setting, {dbRecord.SettingType}");
                }
                return this.OkResponse(EnumEntityType.SETTING, EnumEntityEvents.COMMON_DELETE_ITEM, dbRecord);
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.SETTING, EnumEntityEvents.COMMON_DELETE_EXCEPTION, ex, _logger);
            }
        }
        #endregion

        #region SMSSetting

        [HttpGet("sms-setting")]
        [CustomAuthorize(EnumPermissionFor.SETTING, EnumPermissions.ViewAccess)]
        public async Task<IActionResult> GetSMSSetting()
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation("Calling the GetSMSSetting action");
                }
                var setting = await _settingService.GetSetting<SMSSetting>(EnumSettingType.SMSSettings);
                return this.OkResponse(EnumEntityType.SETTING, EnumEntityEvents.COMMON_GET_ITEM, setting);
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.SETTING, EnumEntityEvents.COMMON_GET_ITEM, ex, _logger);
            }
        }

        [HttpPut("sms-setting")]
        [CustomAuthorize(EnumPermissionFor.SETTING, EnumPermissions.ViewUpdateCreateAccess)]
        public async Task<IActionResult> UpdateSMSSetting([FromBody] SettingDto<SMSSetting> dtoRecord)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation("Calling the UpdateSMSSetting action");
                }
                if (this.ModelState.IsValid)
                {
                    var dbRecord = await _settingService.UpdateSetting<SMSSetting>(dtoRecord);
                    return this.OkResponse(EnumEntityType.SETTING, EnumEntityEvents.COMMON_UPDATE_ITEM, dbRecord);
                }
                else
                {
                    return this.CreateBadRequest(EnumEntityType.SETTING, EnumEntityEvents.COMMON_PUT_EXCEPTION, ModelState, _logger);
                }
            }
            catch(Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.SETTING, EnumEntityEvents.COMMON_PUT_EXCEPTION, ex, _logger);
            }
        }
        #endregion

        #region SMTPSetting

        [HttpGet("smpt-setting")]
        [CustomAuthorize(EnumPermissionFor.SETTING, EnumPermissions.ViewAccess)]
        public async Task<IActionResult> GetSMTPSetting()
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation("Calling the GetSMTPSetting action");
                }
                var setting = await _settingService.GetSetting<SMTPSetting>(EnumSettingType.SMTPSettings);
                return this.OkResponse(EnumEntityType.SETTING, EnumEntityEvents.COMMON_GET_ITEM, setting);
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.SETTING, EnumEntityEvents.COMMON_GET_ITEM, ex, _logger);
            }
        }

        [HttpPut("smpt-setting")]
        [CustomAuthorize(EnumPermissionFor.SETTING, EnumPermissions.ViewUpdateCreateAccess)]
        public async Task<IActionResult> UpdateSMTPSetting([FromBody] SettingDto<SMTPSetting> dtoRecord)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation("Calling the UpdateSMTPSetting action");
                }
                if (this.ModelState.IsValid)
                {
                    var dbRecord = await _settingService.UpdateSetting<SMTPSetting>(dtoRecord);
                    return this.OkResponse(EnumEntityType.SETTING, EnumEntityEvents.COMMON_UPDATE_ITEM, dbRecord);
                }
                else
                {
                    return this.CreateBadRequest(EnumEntityType.SETTING, EnumEntityEvents.COMMON_PUT_EXCEPTION, ModelState, _logger);
                }
            }
            catch(Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.SETTING, EnumEntityEvents.COMMON_PUT_EXCEPTION, ex, _logger);
            }
        }

        #endregion
    }
}
