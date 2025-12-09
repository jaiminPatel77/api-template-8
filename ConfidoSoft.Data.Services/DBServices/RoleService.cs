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
using System.Threading.Tasks;

namespace ConfidoSoft.Data.Services.DBServices
{
    #region IRoleService interface

    /// <summary>
    /// Role Entity Service Interface
    /// </summary>
    public interface IRoleService : IBaseService<Role>
    {
        /// <summary>
        /// Get filtered list of RoleListDto based on Data Query option input.
        /// </summary>
        /// <param name="dataQueryOptions"> Data filter and sorting option. </param>
        /// <returns>DataQueryResult<RoleListDto> matching records.</returns>
        Task<DataQueryResult<RoleListDto>> GetFilteredDtoRecords(DataQueryOptions dataQueryOptions);

        /// <summary>
        /// Get Lookup list of RoleLookUpDto based on Data Query option input.
        /// </summary>
        /// <param name="dataQueryOptions">Data Query Option input.</param>
        /// <returns>Matching records.</returns>
        Task<DataQueryResult<RoleLookUpDto>> GetLookUpDtoRecords(DataQueryOptions dataQueryOptions);

        /// <summary>
        /// Get Role Dto for given role id or PK.
        /// Throw exception if record not found. Include permission detail also.
        /// </summary>
        /// <param name="id">Record id or PK</param>
        /// <returns>Record associated with given id</returns>
        Task<RoleDto> GetDtoRecord(long id);

        /// <summary>
        /// Create new role record.Throw error if same name role already exists.
        /// </summary>
        /// <param name="dtoRecord"></param>
        /// <returns></returns>
        Task<RoleDto> Create(RoleDto dtoRecord);

        /// <summary>
        /// Update existing role record, including associated permissions.
        /// Throw exception if record not found or modified by other user.
        /// </summary>
        /// <param name="dtoRecord">Role dto</param>
        /// <returns></returns>
        Task<RoleDto> Update(RoleDto dtoRecord);

        /// <summary>
        /// get the role detail for given user id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<RoleLookUpDto>> GetUserRoles(long userId);
    }

    #endregion

    #region RoleService class

    /// <summary>
    /// Role Entity Service interface.
    /// </summary>
    public class RoleService : BaseService<Role>, IRoleService
    {

        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;

        #region ctor

        public RoleService(ApplicationDbContext dbContext,
            ILogger<RoleService> logger,
             RoleManager<Role> roleManager,
              UserManager<User> userManager,
            ICurrentUserInfo currentUserInfo,
            IMapper mapper) : base(logger, dbContext, currentUserInfo, mapper)
        {
            this._roleManager = roleManager;
            this._userManager = userManager;
        }
        #endregion

        #region Must override methods

        /// <summary>
        /// Must override to add base filter for all table query!..
        /// For Role by default we filter out system generated records. so that such records can't be miss-used.
        /// or deleted.
        /// </summary>
        /// <returns></returns>
        protected override IQueryable<Role> AddBaseFilterExpression(IQueryable<Role> filterExp)
        {
        //Sanjay Need to check here for default filter... I think now it should be all for template.
            return filterExp.Where(r => (r.RoleType != EnumRoleType.GlobalAdministrator));            
        }

        #endregion

        #region Entity Specific Methods

        /// <summary>
        /// Get filtered list of RoleListDto based on Data Query option input.
        /// </summary>
        /// <param name="dataQueryOptions"> Data filter and sorting option. </param>
        /// <returns>DataQueryResult<RoleListDto> matching records.</returns>
        public async Task<DataQueryResult<RoleListDto>> GetFilteredDtoRecords(DataQueryOptions dataQueryOptions)
        {
            Expression<Func<Role, RoleListDto>> selectExpression = RoleListDto.ToDto;
            return await base.GetFilteredRecords(dataQueryOptions, selectExpression);
        }

        /// <summary>
        /// Get Lookup list of RoleLookUpDto based on Data Query option input.
        /// </summary>
        /// <param name="dataQueryOptions">Data Query Option input.</param>
        /// <returns>Matching records.</returns>
        public async Task<DataQueryResult<RoleLookUpDto>> GetLookUpDtoRecords(DataQueryOptions dataQueryOptions)
        {
            var listResult = await base.GetFilteredRecords<RoleLookUpDto>(dataQueryOptions, RoleLookUpDto.ToDto);
            return listResult;
        }

        public async Task<List<RoleLookUpDto>> GetUserRoles(long userId)
        {
            var userRoles = await this.DbSet.Where(e => (e.UserRoles.Any(ur => ur.UserId == userId))).Select(RoleLookUpDto.ToDto).ToListAsync();
            return userRoles;
        }

        #endregion

        #region Dto version of CURD

        /// <summary>
        /// Get Role Dto for given role id or PK.
        /// Throw exception if record not found. Include permission detail also.
        /// </summary>
        /// <param name="id">Record id or PK</param>
        /// <returns>Record associated with given id</returns>
        public async Task<RoleDto> GetDtoRecord(long id)
        {
            var dtoRecord =  await this.TableNoTracking.Where(e => e.Id == id).Select(RoleDto.ToDto).FirstOrDefaultAsync();
            if (dtoRecord != null)
            {
                //Set users information.                
                var users = await this._userManager.GetUsersInRoleAsync(dtoRecord.Name);
                var usersList = new List<UserLookUpDto>();
                UserLookUpDto userDto = null;
                foreach (var user in users)
                {
                    userDto = this._mapper.Map<User, UserLookUpDto>(user);
                    usersList.Add(userDto);
                }
                dtoRecord.Users = usersList;

                //Set permission information here
                var rolePermissions = this._dbContext.RolePermissions.Where(e => e.RoleId == dtoRecord.Id).ToDictionary(e => e.PermissionFor, e => e);
                RolePermission rolePermission = null;
                List<PermissionDto> rolePermissionsDto = new List<PermissionDto>();

                foreach (EnumPermissionFor entityType in Enum.GetValues(typeof(EnumPermissionFor)))
                {
                    rolePermission = null;
                    if (rolePermissions.ContainsKey(entityType))
                    {
                        rolePermission = rolePermissions[entityType];
                    }
                    if (rolePermission == null)
                    {
                        rolePermission = new RolePermission
                        {
                            RoleId = dtoRecord.Id,
                            PermissionFor = entityType
                        };                        
                    }
                    var rolePermissionDto = _mapper.Map<RolePermission, PermissionDto>(rolePermission);
                    rolePermissionsDto.Add(rolePermissionDto);
                }
                dtoRecord.Permissions = rolePermissionsDto;                

                return dtoRecord;
            }
            else
            {
                throw new CSApplicationException(EnumEntityEvents.COMMON_GET_ITEM_NOTFOUND.ToString(), CommonConstStr.COMMON_ITEM_NOTFOUND);
            }
        }

        /// <summary>
        /// Create new role record.Throw error if same name role already exists.
        /// </summary>
        /// <param name="dtoRecord"></param>
        /// <returns></returns>
        public async Task<RoleDto> Create(RoleDto dtoRecord)
        {
            SetRecordCreatedInfo(dtoRecord);
            SetRecordModifiedInfo(dtoRecord);
            var record = _mapper.Map<RoleDto, Role>(dtoRecord);            
            //Add permission if any selected!
            if (dtoRecord.Permissions != null)
            {
                foreach (var permissionDto in dtoRecord.Permissions)
                {
                    var rolePermission = _mapper.Map<PermissionDto, RolePermission>(permissionDto);
                    record.RolePermissions.Add(rolePermission);
                }
            }
            SetRecordCreatedInfo(record);
            SetRecordModifiedInfo(record);

            var result = await this._roleManager.CreateAsync(record);
            if(result.Succeeded)
            {
                //Add users in roles..
                foreach (var user in dtoRecord.Users)
                {
                    var dbUser = await this._userManager.Users.Where(e => e.Id == user.Id).FirstOrDefaultAsync();
                    if (dbUser != null)
                    {
                        await this._userManager.AddToRoleAsync(dbUser, record.Name);
                    }
                }
                _mapper.Map<Role, RoleDto>(record, dtoRecord);
                return dtoRecord;
            }
            else
            {
                var errorDetail = result.ToErrorMessage();
                throw new Exception(errorDetail);
            }           
        }

        /// <summary>
        /// Update existing role record, including associated permissions.
        /// Throw exception if record not found or modified by other user.
        /// </summary>
        /// <param name="dtoRecord">Role dto</param>
        /// <returns></returns>
        public async Task<RoleDto> Update(RoleDto dtoRecord)
        {
            var dbRecord = await this.GetAssociatedDbRecord(dtoRecord.Id);
            if (dbRecord != null)
            {
                this.PreProcessingOnRecordUpdate(dbRecord, dtoRecord);
                _mapper.Map<RoleDto, Role>(dtoRecord, dbRecord);                

                //update permission detail.
                if (dtoRecord.Permissions != null)
                {
                    var rolePermisions = _dbContext.RolePermissions.Where(e => e.RoleId == dbRecord.Id).ToDictionary(e => e.PermissionFor, e => e);

                    foreach (var permissionDto in dtoRecord.Permissions)
                    {
                        if (rolePermisions.ContainsKey(permissionDto.PermissionFor))
                        {
                            var dbPermission = rolePermisions[permissionDto.PermissionFor];
                            dbPermission.Permissions = permissionDto.Permissions;
                        }
                        else
                        {
                            var rolePermission = _mapper.Map<PermissionDto, RolePermission>(permissionDto);
                            dbRecord.RolePermissions.Add(rolePermission);
                        }
                    }
                }
                var result = await this._roleManager.UpdateAsync(dbRecord);
                if (!result.Succeeded)
                {
                    var errorDetail = result.ToErrorMessage();
                    throw new Exception(errorDetail);
                }

                //Code to update the users details.
                var usersInRole = await this._userManager.GetUsersInRoleAsync(dbRecord.Name);
                var users = usersInRole.ToDictionary(user => user.Id, user => user);
                foreach (var user in dtoRecord.Users)
                {
                    if (users.ContainsKey(user.Id))
                    {
                        //already exists so do nothing..
                    }
                    else
                    {
                        var u = await this._userManager.Users.Where( e => e.Id == user.Id).FirstOrDefaultAsync();
                        if(u != null)
                        {
                            await this._userManager.AddToRoleAsync(u, dbRecord.Name);
                        }                        
                    }
                    users.Remove(user.Id);
                }
                foreach (var kvp in users)
                {
                    await this._userManager.RemoveFromRoleAsync(kvp.Value, dbRecord.Name);
                }
                return dtoRecord;
            }
            else
            {
                throw new CSApplicationException(EnumEntityEvents.COMMON_UPDATE_ITEM_NOTFOUND.ToString(), CommonConstStr.COMMON_ITEM_NOTFOUND);
            }
        }

        #endregion

    }

    #endregion

}