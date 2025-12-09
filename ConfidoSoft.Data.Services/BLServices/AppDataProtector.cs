using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConfidoSoft.Data.Services.BLServices
{
    /// <summary>
    /// Interface to load/remove key dynamically
    /// </summary>
    public interface ISetLookupProtectorKeyRing
    {
        /// <summary>
        /// load new key and associated Id for handling encryption.
        /// </summary>
        /// <param name="keyId">Id of a Key e.g. 01, 02</param>
        /// <param name="keyValue">Actual encryption key.</param>
        /// <param name="setAsCurrentKey">True if key need to be set as currently encryption key.</param>
        void SetKey(string keyId, string keyValue, bool setAsCurrentKey);

        /// <summary>
        /// Remove associated Key and Id from cache.
        /// </summary>
        /// <param name="keyId">KeyId</param>
        void RemoveKey(string keyId);
    }


    /// <summary>
    /// Manage named keys used to protect lookups for asp identity application.
    /// </summary>
    public class AppDataKeyRing : ILookupProtectorKeyRing, ISetLookupProtectorKeyRing
    {
        private static Object _lockObject = new object();
        public static string DefaultKeyId = "00";
        private Dictionary<string, string> _keyIdsLookup;
        private string _currentKeyLookupId;

        public AppDataKeyRing()
        {
        }

        /// <summary>
        /// Return the key associated with given Key Id.
        /// </summary>
        /// <param name="keyId"> KeyId for which key lookup is do be performed.</param>
        /// <returns> Return key associated with give key Id. throw exception if
        /// key id is not found.
        /// </returns>
        public string this[string keyId]
        {
            get
            {
                lock (_lockObject)
                {
                    var keyIdsLookup = GetActualKeyIdsLookup();
                    if (keyIdsLookup.ContainsKey(keyId))
                    {
                        return keyIdsLookup[keyId];
                    }
                    else
                    {
                        var error = $"Didn't find Encryption Key for KeyId:{keyId}";
                        throw new InvalidOperationException(error);
                    }
                }
            }
        }

        /// <summary>
        /// Return currently KeyId
        /// </summary>
        public string CurrentKeyId
        {
            get
            {
                lock (_lockObject)
                {
                    GetActualKeyIdsLookup();
                    return _currentKeyLookupId;
                }
            }
        }

        /// <summary>
        /// Return Enumerable for all currently registered Key Id.
        /// </summary>
        /// <returns> Enumerable for all currently Key Id. </returns>
        public IEnumerable<string> GetAllKeyIds()
        {
            lock (_lockObject)
            {
                return GetActualKeyIdsLookup().Keys;
            }
        }

        /// <summary>
        /// load new key and associated Id for handling encryption.
        /// </summary>
        /// <param name="keyId">Id of a Key e.g. 01, 02</param>
        /// <param name="keyValue">Actual encryption key.</param>
        /// <param name="setAsCurrentKey">True if key need to be set as currently encryption key.</param>
        public void SetKey(string keyId, string keyValue, bool setAsCurrentKey)
        {
            lock (_lockObject)
            {
                var keyIds = GetActualKeyIdsLookup();
                if (keyIds.ContainsKey(keyId))
                {
                    var existingKey = keyIds[keyId];
                    if (existingKey != keyValue)
                    {
                        keyIds[keyId] = keyValue;
                    }
                }
                else
                {
                    keyIds.Add(keyId, keyValue);
                }
                if (setAsCurrentKey)
                {
                    _currentKeyLookupId = keyId;
                }
            }
        }

        /// <summary>
        /// Remove associated Key and Id from cache.
        /// </summary>
        /// <param name="keyId">Key Id</param>
        public void RemoveKey(string keyId)
        {
            lock (_lockObject)
            {
                var keyIds = GetActualKeyIdsLookup();
                if (keyIds.ContainsKey(keyId))
                {
                    keyIds.Remove(keyId);
                }
            }
        }

        private Dictionary<string, string> GetActualKeyIdsLookup()
        {
            if (_keyIdsLookup == null)
            {
                _keyIdsLookup = new Dictionary<string, string>();
                _keyIdsLookup.Add(DefaultKeyId, DataEncryptionService.AppKey);
                _currentKeyLookupId = DefaultKeyId;
            }
            return _keyIdsLookup;
        }
    }

    /// <summary>
    /// Used to protect/UN-protect lookups with a specific key for asp identity.
    /// </summary>
    public class AppDataProtector : ILookupProtector
    {
        private static Object _lockObject = new object();        
        ILookupProtectorKeyRing _lookupProtectorKeyRing;
        DataEncryptionService _dataEncryptionService;

        public AppDataProtector(ILookupProtectorKeyRing lookupProtectorKeyRing)
        {
            _lookupProtectorKeyRing = lookupProtectorKeyRing;
        }

        /// <summary>
        /// Unprotect the data using key associated with given keyId
        /// </summary>
        /// <param name="keyId"> keyId to find the decrypt the data using associated key</param>
        /// <param name="data">Encrypted data</param>
        /// <returns>Decrypted data/string using key associated with given keyId</returns>
        public string Unprotect(string keyId, string data)
        {
            var key = _lookupProtectorKeyRing[keyId];
            var service = GetDataEncryptionService(key);
            var actualData = service.Decrypt(data);
            return actualData;            
        }

        /// <summary>
        /// Encrypt given data using key associated with given key id.
        /// </summary>
        /// <param name="keyId"> Key id to lookup the key.</param>
        /// <param name="data"> Data to be encrypted.</param>
        /// <returns>Encrypted data</returns>
        public string Protect(string keyId, string data)
        {
            var key = _lookupProtectorKeyRing[keyId];
            var service = GetDataEncryptionService(key);
            var retVale = service.Encrypt(data);
            return retVale;            
        }

        /// <summary>
        /// Get DataEncryptionservice for given key.
        /// </summary>
        /// <param name="key"> Encryption key</param>
        /// <returns>Service which help to encrypt data using given key</returns>
        private DataEncryptionService GetDataEncryptionService(string key)
        {
            lock (_lockObject)
            {
                if (this._dataEncryptionService == null)
                {
                    this._dataEncryptionService = new DataEncryptionService(key, DataEncryptionService.AppIV);
                }
                else if (this._dataEncryptionService.Key != key)
                {                    
                    this._dataEncryptionService = new DataEncryptionService(key, DataEncryptionService.AppIV);
                }
                return this._dataEncryptionService;
            }
        }
    }

}
