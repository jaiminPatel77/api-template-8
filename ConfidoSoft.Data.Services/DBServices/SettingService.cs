using AutoMapper;
using ConfidoSoft.Data.Domain.Consts;
using ConfidoSoft.Data.Domain.Database;
using ConfidoSoft.Data.Domain.DBModels;
using ConfidoSoft.Data.Domain.Dtos;
using ConfidoSoft.Data.Services.BLServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ConfidoSoft.Data.Services.DBServices
{
    #region ISettingService interface

    /// <summary>
    /// Setting Entity Service Interface.
    /// Allow to Get/Put for any setting type per Enterprise/Tenant or even application level!.
    /// </summary>
    public interface ISettingService : IBaseService<Setting>
    {
        /// <summary>
        /// Get the setting define for given setting type.
        /// It will return the setting for logged in Tenant.
        /// Note: Encryption key related are per application so will not be associated
        /// with any Enterprise/Tenant.
        /// </summary>
        /// <typeparam name="T">Typed object/setting detail for given setting type.</typeparam>
        /// <param name="settingType">Unique Setting type per Tenant.</param>
        /// <returns>Return setting from database if found otherwise return default value.</returns>
        Task<SettingDto<T>> GetSetting<T>(EnumSettingType settingType) where T : class;
        /// <summary>
        /// Create setting record if not already found in database otherwise 
        /// update the existing setting detail.
        /// Will create/update the setting type associate for logged in user tenant/Enterprise.
        /// </summary>
        /// <typeparam name="T">Typed object/setting detail for given setting type.</typeparam>
        /// <param name="settingType">Unique Setting type per Tenant.</param>
        /// <returns></returns>
        Task<SettingDto<T>> UpdateSetting<T>(SettingDto<T> settingDto) where T : class;
    }

    #endregion

    #region ISettingService class

    /// <summary>
    /// Setting Entity Service interface.
    /// </summary>
    public class SettingService : BaseService<Setting>, ISettingService
    {
        private ISettingDataProtector _settingDataProtector;

        #region ctor
        public SettingService(ApplicationDbContext dbContext,
            ILogger<SettingService> logger,
            ISettingDataProtector settingDataProtector,
            ICurrentUserInfo currentUserInfo,
            IMapper mapper) : base(logger, dbContext, currentUserInfo, mapper)
        {
            _settingDataProtector = settingDataProtector;
        }

        #endregion

        #region Dto version of CURD

        /// <summary>
        /// Get the setting define for given setting type.
        /// </summary>
        /// <typeparam name="T">Typed object/setting detail for given setting type.</typeparam>
        /// <param name="settingType">Unique Setting type.</param>
        /// <returns>Return setting from database if found otherwise return default value.</returns>
        public async Task<SettingDto<T>> GetSetting<T>(EnumSettingType settingType) where T : class
        {
            SettingDto<T> settingDto = null;
            var record = await this.TableNoTracking.Where(e => e.SettingType == settingType).FirstOrDefaultAsync();
            if (record != null)
            {
                if (false == String.IsNullOrEmpty(record.DBValue))
                {
                    var dbValue = record.DBValue;
                    if (record.IsEncrypted)
                    {
                        //Decrypt data
                        dbValue = _settingDataProtector.Decrypt(settingType, record.DBValue);
                    }
                    settingDto = new SettingDto<T>();
                    Copy(record, settingDto);
                    DeserializeSettingValue(dbValue, settingDto);
                }
            }
            else
            {
                settingDto = SettingDto<T>.CreateDefault(settingType);
                settingDto.CreatedById = this._currentUserInfo.Id;
                settingDto.ModifiedById = this._currentUserInfo.Id;
                settingDto.ModifiedOn = settingDto.CreatedOn;
            }
            return settingDto;
        }

        /// <summary>
        /// Update setting value if existing otherwise add it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingDto"></param>
        /// <returns></returns>
        public async Task<SettingDto<T>> UpdateSetting<T>(SettingDto<T> settingDto) where T : class
        {
            String fieldValue = null;
            if ( (null != settingDto.Value) && (settingDto.SettingType != EnumSettingType.None))
            {
                fieldValue = SerializeSettingValue(settingDto);
                if (settingDto.IsEncrypted)
                {
                    //Encrypt the value before store
                    fieldValue = _settingDataProtector.Encrypt(settingDto.SettingType, fieldValue);
                }
                var dbRecord = _dbContext.Settings.Where(e => e.SettingType == settingDto.SettingType).FirstOrDefault();
                if (dbRecord == null)
                {
                    dbRecord = new Setting();
                    Copy(settingDto, dbRecord);
                    dbRecord.DBValue = fieldValue;
                    dbRecord = await this.Create(dbRecord);
                    Copy(dbRecord, settingDto);
                }
                else
                {
                    this.PreProcessingOnRecordUpdate(dbRecord, settingDto);
                    Copy(settingDto, dbRecord);
                    dbRecord.DBValue = fieldValue;
                    await this.SaveChanges();
                }
            }            
            return settingDto;
        }

        #endregion

        #region Copy Setting to Dto and other ways

        /// <summary>
        /// Copy setting object to SettingDto<T> manually.
        /// </summary>
        /// <typeparam name="T">Type associated with given setting type.</typeparam>
        /// <param name="source">Source setting object.</param>
        /// <param name="dest">Copy to destination dto object.</param>

        public static void Copy<T>(Setting source, SettingDto<T> dest) where T: class
        {
            dest.Id = source.Id;
            dest.SettingType = source.SettingType;
            dest.SettingCategory = source.SettingCategory;
            dest.StorageFormate = source.StorageFormate;
            dest.IsEncrypted = source.IsEncrypted;
            dest.DBValue = source.DBValue;
            dest.Status = source.Status;
            dest.CreatedById = source.CreatedById;
            dest.CreatedOn = source.CreatedOn;
            dest.ModifiedById = source.ModifiedById;
            dest.ModifiedOn = source.ModifiedOn;
        }

        /// <summary>
        /// Copy setting dto object to Setting manually.
        /// </summary>
        /// <typeparam name="T">Type associated with given setting type.</typeparam>
        /// <param name="source">Source setting dto object.</param>
        /// <param name="dest">Copy to destination setting object.</param>
        public static void Copy<T>(SettingDto<T> source, Setting dest) where T : class
        {
            dest.Id = source.Id;
            dest.SettingType = source.SettingType;
            dest.SettingCategory = source.SettingCategory;
            dest.StorageFormate = source.StorageFormate;
            dest.IsEncrypted = source.IsEncrypted;
            dest.DBValue = source.DBValue;
            dest.CreatedById = source.CreatedById;
            dest.CreatedOn = source.CreatedOn;
            dest.ModifiedById = source.ModifiedById;
            dest.ModifiedOn = source.ModifiedOn;
            dest.Status = source.Status;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// De-serialize Setting.Value as Value Object based on storage format.
        /// </summary>
        /// <typeparam name="T">Type associated with given setting type.</typeparam>
        /// <param name="dbValue">raw string value stored in database.</param>
        /// <param name="settingDto"> setting dto to which object value to be set from raw dbValue</param>
        public static void DeserializeSettingValue<T>(string dbValue, SettingDto<T> settingDto) where T: class
        {
            switch (settingDto.StorageFormate)
            {
                case (EnumSettingStorageFormate.JSONFormat):
                    {
                        //using Newtonsoft.Json
                        settingDto.Value = JsonConvert.DeserializeObject<T>(dbValue);
                        break;
                    }
                default:
                    {
                        settingDto.Value = (T)((object)dbValue);
                        break;
                    }
            }
        }

        /// <summary>
        /// Serialize Setting.Value to get appropriate format string representation to be return.
        /// </summary>
        /// <typeparam name="T">type associated with given setting type</typeparam>
        /// <param name="settingDto">setting dto object value to be serialized</param>
        /// <returns>formated string value for a given setting type</returns>
        public static String SerializeSettingValue<T>(SettingDto<T> settingDto) where T : class
        {
            String settingValue = null;
            switch (settingDto.StorageFormate)
            {
                case (EnumSettingStorageFormate.JSONFormat):
                    {
                        //using Newtonsoft.Json
                        settingValue = JsonConvert.SerializeObject(settingDto.Value);
                        break;
                    }
                default:
                    {
                        settingValue = settingDto.Value.ToString();
                        break;
                    }
            }
            return settingValue;
        }

        #endregion


    }

    #endregion

}