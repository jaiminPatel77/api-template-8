using System;
using System.Collections.Generic;
using System.Text;

namespace ConfidoSoft.Data.Domain.DBModels.Settings
{
    /// <summary>
    /// Model for Storing Encryption Key Setting information.
    /// </summary>
    public class EncryptionKeySetting
    {
        /// <summary>
        /// Id of key which will be stored in each field which is encrypted using this key.
        /// It just a unique id e.g. 01, 02. Using keyId associated actual key will be used 
        /// to decrypt the data.
        /// </summary>
        public string KeyId { get; set; }

        /// <summary>
        /// Actual Encryption Key associated with KeyId field.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// DateTime when key was generated on.
        /// </summary>
        public DateTime CreatedOn { get; set; }
        
        /// <summary>
        /// Expiry DateTime if needed. If null it's valid always.
        /// </summary>
        public DateTime? ExpiryDate { get; set; }


        /// <summary>
        /// Generate new random Key. Also set appropriate KeyId value from previous Id.
        /// </summary>
        /// <param name="previousKeyId"> Previous KeyId if any otherwise null</param>
        /// <returns> Return new EncryptionKeySetting </returns>
        public static EncryptionKeySetting GenerateNewKey(String previousKeyId)
        {
            var encryptionKeySetting = new EncryptionKeySetting();
            encryptionKeySetting.CreatedOn = DateTime.UtcNow;
            //for now no limit!
            //encryptionKeySetting.ExpiryDate = DateTime.UtcNow.AddYears(1);
            int intKeyId = 1; //start with 1
            if (false == String.IsNullOrEmpty(previousKeyId))
            {
                if (int.TryParse(previousKeyId, out intKeyId))
                {
                    intKeyId++;
                    if (intKeyId > 10)
                    {
                        //back to 01.
                        intKeyId = 1;
                    }
                }
            }
            encryptionKeySetting.KeyId = intKeyId.ToString("D2");
            encryptionKeySetting.Key = Guid.NewGuid().ToString("N").ToUpperInvariant();            
            return encryptionKeySetting;
        }
    }
}
