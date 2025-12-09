using ConfidoSoft.Data.Domain.Consts;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConfidoSoft.Data.Services.BLServices
{
    /// <summary>
    /// Data protector for application setting table.
    /// Which also store the dynamic encryption key which is encrypted using application key!
    /// </summary>
    public interface ISettingDataProtector
    {
        /// <summary>
        /// Encrypt the given application setting type.
        /// We use fix key to encrypt actual encryption key setting row.
        /// </summary>
        /// <param name="settingType">application setting type.</param>
        /// <param name="text">data/text to be encrypted.</param>
        /// <returns>Encrypted data.</returns>
        string Encrypt(EnumSettingType settingType, string text);

        /// <summary>
        /// Decrypt the given encrypted string.
        /// We use fix key to encrypt actual encryption key setting row.
        /// </summary>
        /// <param name="settingType">application setting type.</param>
        /// <param name="cipherText">encrypted string.</param>
        /// <returns>decrypted string</returns>
        string Decrypt(EnumSettingType settingType, string cipherText);
    }

    /// <summary>
    /// Helper service to encrypt the application setting rows.
    /// </summary>
    public class SettingDataProtector : ISettingDataProtector
    {
        private readonly ILookupProtectorKeyRing _keyRing;
        private readonly DataEncryptionService _appKeyEncryptionService;

        public SettingDataProtector(ILookupProtectorKeyRing keyRing)
        {
            this._keyRing = keyRing;
            this._appKeyEncryptionService = new DataEncryptionService(DataEncryptionService.AppKey);
        }

        /// <summary>
        /// Encrypt the given application setting type.
        /// We use fix key to encrypt actual encryption key setting row.
        /// </summary>
        /// <param name="settingType">application setting type.</param>
        /// <param name="text">data/text to be encrypted.</param>
        /// <returns>Encrypted data.</returns>
        public string Decrypt(EnumSettingType settingType, string data)
        {
            string retVal = null;

            if (String.IsNullOrWhiteSpace(data))
            {
                return data;
            }
            else
            {
                if (settingType == EnumSettingType.EncryptionKeySettings || settingType == EnumSettingType.PreviousEncryptionKeySettings)
                {
                    retVal = _appKeyEncryptionService.Decrypt(data);
                }
                else
                {
                    var split = data.LastIndexOf(':');
                    if (split == -1 || split == data.Length - 1)
                    {
                        throw new InvalidOperationException("Malformed data. Unable to Decrypt data!");
                    }
                    var cipherText = data.Substring(0, split);
                    var keyId = data.Substring(split + 1);
                    var key = this._keyRing[keyId];
                    var encryptionService = new DataEncryptionService(key);
                    retVal = encryptionService.Decrypt(cipherText);                    
                }
                return retVal;
            }
        }

        /// <summary>
        /// Decrypt the given encrypted string.
        /// We use fix key to encrypt actual encryption key setting row.
        /// </summary>
        /// <param name="settingType">application setting type.</param>
        /// <param name="cipherText">encrypted string.</param>
        /// <returns>decrypted string</returns>
        public string Encrypt(EnumSettingType settingType, string data)
        {
            string retVal = null;
            if (String.IsNullOrWhiteSpace(data))
            {
                return data;
            }
            else
            {
                if (settingType == EnumSettingType.EncryptionKeySettings || settingType == EnumSettingType.PreviousEncryptionKeySettings)
                {
                    retVal = this._appKeyEncryptionService.Encrypt(data);
                }
                else
                {
                    var currentKeyId = _keyRing.CurrentKeyId;
                    var key = this._keyRing[currentKeyId];
                    var encryptionService = new DataEncryptionService(key);
                    retVal = encryptionService.Encrypt(data);
                    retVal = retVal + ":" + currentKeyId;
                }
                return retVal;
            }
        }
    }
}
