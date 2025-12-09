using System;
using System.Collections.Generic;
using System.Linq;
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
    public class RoleController : Controller
    {
        private readonly IRoleService _roleService;
        private readonly ILogger _logger;

        public RoleController(IRoleService personService, 
            ILogger<RoleController> logger)
        {
            _roleService = personService;
            _logger = logger;
        }

        #region CURD API

        // GET: api/Role
        [HttpGet]
        //IEnumerable<CSRole>
        [CustomAuthorize(EnumPermissionFor.ROLE, EnumPermissions.ViewAccess)]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation("Calling the GetRoles action");
                }
                var dataQueryOption = DataQueryOptionsHelper.FillDataQueryOptions();
                var list = await _roleService.GetFilteredDtoRecords(dataQueryOption);
                return this.OkResponse(EnumEntityType.ROLE, EnumEntityEvents.COMMON_LIST_FILTER_ITEMS, list);
            }
            catch(Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.ROLE, EnumEntityEvents.COMMON_LIST_EXCEPTION, ex, _logger);
            }
        }

        // GET: api/Role/5
        [HttpGet("{id}")]
        [CustomAuthorize(EnumPermissionFor.ROLE, EnumPermissions.ViewAccess)]
        public async Task<IActionResult> GetRole(long id)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the GetRole action, {id}");
                }
                var record = await _roleService.GetDtoRecord(id);
                if (record == null)
                {
                    return this.CreateBadRequest(EnumEntityType.ROLE, EnumEntityEvents.COMMON_GET_ITEM_NOTFOUND, _logger);
                }
                return this.OkResponse(EnumEntityType.ROLE, EnumEntityEvents.COMMON_GET_ITEM, record);
            }
            catch(Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.ROLE, EnumEntityEvents.COMMON_GET_EXCEPTION, ex, _logger);
            }
        }
        
        // POST: api/Role
        [HttpPost]
        [CustomAuthorize(EnumPermissionFor.ROLE, EnumPermissions.CreateAccess)]
        public async Task<IActionResult> CreateRole([FromBody]RoleDto recordDto)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the CreateRole action, {recordDto.Name}");
                }
                if (!this.ModelState.IsValid)
                {
                    return this.CreateBadRequest(EnumEntityType.ROLE, EnumEntityEvents.COMMON_CREATE_EXCEPTION, ModelState, _logger);
                }
                recordDto = await _roleService.Create(recordDto);
                var url = this.Url.Action("Get", new { id = recordDto.Id });
                return this.CreatedResponse(EnumEntityType.ROLE, EnumEntityEvents.COMMON_CREATE_ITEM, recordDto, url);
            }
            catch(Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.ROLE, EnumEntityEvents.COMMON_CREATE_EXCEPTION, ex, _logger);
            }
        }
        
        // PUT: api/Role/5
        [HttpPut("{id}")]
        [CustomAuthorize(EnumPermissionFor.ROLE, EnumPermissions.UpdateAccess)]
        public async Task<IActionResult> PutRole(string id, [FromBody]RoleDto recordDto)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the PutRole action, {recordDto.Name}");
                }
                if (!this.ModelState.IsValid)
                {
                    return this.CreateBadRequest(EnumEntityType.ROLE, EnumEntityEvents.COMMON_PUT_EXCEPTION, ModelState, _logger);
                }
                await _roleService.Update(recordDto);
                return this.OkResponse(EnumEntityType.ROLE, EnumEntityEvents.COMMON_UPDATE_ITEM, recordDto);
            }
            catch(Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.ROLE, EnumEntityEvents.COMMON_PUT_EXCEPTION, ex, _logger);
            }
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        [CustomAuthorize(EnumPermissionFor.ROLE, EnumPermissions.DeleteAccess)]
        public async Task<IActionResult> DeleteRole(long id)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the DeleteRole action, {id}");
                }
                var record = await _roleService.Remove(id);
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Role {record.Name} Deleted!");
                }
                return this.OkResponse(EnumEntityType.ROLE, EnumEntityEvents.COMMON_DELETE_ITEM, record);
            }
            catch(Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.ROLE, EnumEntityEvents.COMMON_DELETE_EXCEPTION, ex, _logger);
            }
        }

        #endregion

        #region Filter List
        [HttpGet("filter-list")]
        [CustomAuthorize(EnumPermissionFor.ROLE, EnumPermissions.ViewAccess)]
        public async Task<IActionResult> GetAsync(string filter = null, string orderBy = null,
            int pageNo = 1, int size = 10)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation("Calling the filterList action");
                }
                var dataQueryOption = DataQueryOptionsHelper.FillDataQueryOptions(filter, orderBy, pageNo, size);
                var list = await _roleService.GetFilteredDtoRecords(dataQueryOption);
                return this.OkResponse(EnumEntityType.ROLE, EnumEntityEvents.COMMON_LIST_FILTER_ITEMS, list);
            }
            catch(Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.ROLE, EnumEntityEvents.COMMON_LIST_EXCEPTION, ex, _logger);
            }
        }
        #endregion

        #region Lookup List

        /// <summary>
        /// By default return all role records .. if no filter param passed!
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageNo"></param>
        /// <param name="size"></param>
        /// <returns></returns>
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
                var list = await _roleService.GetLookUpDtoRecords(dataQueryOption);
                return this.OkResponse(EnumEntityType.ROLE, EnumEntityEvents.COMMON_LIST_FILTER_ITEMS, list);
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.ROLE, EnumEntityEvents.COMMON_LIST_EXCEPTION, ex, _logger);
            }
        }

        #endregion
    }
}
