using ConfidoSoft.Data.Domain.Consts;
using ConfidoSoft.Data.Domain.DBModels;
using ConfidoSoft.Data.Domain.DBModels.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConfidoSoft.Data.Domain.Dtos
{
    /// <summary>
    /// Dto for Application Settings
    /// </summary>
    /// <typeparam name="T"> Associated Object type of a Setting type.</typeparam>
    public class SettingDto<T> : Setting where T: class
    {
        /// <summary>
        /// Typed object of the setting. Memory only property and it will be 
        /// serialized and de-serialized as part of put and get operation to store the setting
        /// detail in database.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Return default setting value object for a given Setting Type.
        /// Must add/update for each supported setting type!
        /// </summary>
        /// <param name="settingType">setting type for which default value to be created.</param>
        /// <returns>SettionDto<T> for given setting type.</returns>
        public static SettingDto<T> CreateDefault(EnumSettingType settingType)
        {
            SettingDto<T> setting = setting = new SettingDto<T>
            {
                SettingType = settingType,
                SettingCategory = EnumSettingCategory.SecuritySettings,
                IsEncrypted = true,
                StorageFormate = EnumSettingStorageFormate.JSONFormat,
                CreatedOn = DateTimeOffset.UtcNow,
            };

            Object value = null;
            switch (settingType)
            {
                case EnumSettingType.EncryptionKeySettings:
                case EnumSettingType.PreviousEncryptionKeySettings:
                    {
                        value = new EncryptionKeySetting();
                        setting.Value = (T)value;
                        break;
                    }
                case EnumSettingType.SMSSettings:
                    {
                        value = new SMSSetting();
                        setting.Value = (T)value;
                        break;
                    }
                case EnumSettingType.SMTPSettings:
                    {
                        value = new SMTPSetting();
                        setting.Value = (T)value;
                        break;
                    }
            }
            return setting;
        }

    }
}
