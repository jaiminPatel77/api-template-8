using ConfidoSoft.Data.Domain.Consts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConfidoSoft.Data.Domain.DBModels
{
    /// <summary>
    /// Store Application setting, with able to store encrypted detail.
    /// </summary>
    public class Setting : ModelBaseWithCommonFields
    {
        /// <summary>
        /// Unique Fix type of application settings
        /// </summary>
        public EnumSettingType SettingType { get; set; }

        /// <summary>
        /// Associated category for a given setting record
        /// </summary>
        public EnumSettingCategory SettingCategory { get; set; }

        /// <summary>
        /// Indicate how setting are stored in database as string or JSON object or what.
        /// </summary>
        public EnumSettingStorageFormate StorageFormate { get; set; }

        /// <summary>
        /// Indicate the setting is stored as Encrypted string value.
        /// </summary>
        public bool IsEncrypted { get; set; }

        /// <summary>
        /// Actual value stored in the db of a setting
        /// </summary>
        [JsonIgnore]
        public String DBValue { get; set; }
        
        /// <summary>
        /// Status of setting
        /// </summary>
        public EnumSettingStatus Status { get; set; }

    }
}
