using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConfidoSoft.Data.Services.Helpers
{
    /// <summary>
    /// Helper or Extension method for IdentityResult type.
    /// </summary>
    public static class IdentityResultExtension
    {
        /// <summary>
        /// Extension method for IdentityResult to return error string by 
        /// con-cat all error by given separator or default as '!'
        /// </summary>
        public static string ToErrorMessage(this IdentityResult result, string separator = null)
        {            
            if (separator == null)
            {
                separator = "! ";
            }
            var errorDetail = String.Join(separator, result.Errors.Select(x => x.Description));
            return errorDetail;
        }
    }
}
