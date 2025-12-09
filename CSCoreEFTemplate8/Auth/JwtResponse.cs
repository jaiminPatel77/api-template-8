using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSCoreEFTemplate8.Auth
{
    /// <summary>
    /// Jwt token information
    /// </summary>
    public class JwtInfo
    {
        /// <summary>
        /// Actual access token information.
        /// </summary>
        public string access_token { get; set; }
        /// <summary>
        /// Associated refresh token id.
        /// </summary>
        public string refresh_token { get; set; }
    }

    /// <summary>
    /// actual jwt token response!
    /// </summary>
    public class JwtResponse : JwtInfo
    {
        /// <summary>
        /// User id for which auth_token is generated.
        /// </summary>
        public string id { get; set; }
       
        /// <summary>
        /// Expires in second.
        /// </summary>
        public int expires_in { get; set; }
        /// <summary>
        /// Utc expiration time..
        /// </summary>
        public long expiration { get; set; }
       
    }
}
