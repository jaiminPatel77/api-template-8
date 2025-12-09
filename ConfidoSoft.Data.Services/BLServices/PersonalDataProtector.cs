using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConfidoSoft.Data.Services.BLServices
{
    /// <summary>
    /// Added PersonalDataProtector to add checking for null or empty string while protecting 
    /// User personal data.
    /// </summary>
    public class PersonalDataProtector: IPersonalDataProtector
    {
        private readonly ILookupProtectorKeyRing _keyRing;
        private readonly ILookupProtector _encryptor;

        public PersonalDataProtector(ILookupProtectorKeyRing keyRing, ILookupProtector protector)
        {
            _keyRing = keyRing;
            _encryptor = protector;
        }

        /// <summary>
        /// Unprotect the data. Throw error if not a valid encrypted data.
        /// </summary>
        /// <param name="data">The data to unprotect.</param>
        /// <returns>The unprotected data.</returns>
        public virtual string Unprotect(string data)
        {
            if (false == String.IsNullOrWhiteSpace(data))
            {
                var split = data.LastIndexOf(':');
                if (split == -1 || split == data.Length - 1)
                {
                    throw new InvalidOperationException("Malformed data.");
                }

                var dataValue = data.Substring(0, split);
                var keyId = data.Substring(split + 1);
                return _encryptor.Unprotect(keyId, dataValue);
            }
            else
            {
                return data;
            }
        }

        /// <summary>
        /// Protect the data.
        /// </summary>
        /// <param name="data">The data to protect.</param>
        /// <returns>The protected data.</returns>
        public virtual string Protect(string data)
        {
            if (false == String.IsNullOrWhiteSpace(data))
            {
                var keyId = _keyRing.CurrentKeyId;
                var dataValue = _encryptor.Protect(keyId, data);
                return dataValue + ":" + keyId;
            }
            else
            {
                return data;
            }           
        }
    }
}
