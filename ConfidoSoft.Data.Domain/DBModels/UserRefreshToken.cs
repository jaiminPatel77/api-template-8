using ConfidoSoft.Data.Domain.Consts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ConfidoSoft.Data.Domain.DBModels
{
    /// <summary>
    /// Table to store user refresh token information for each logged in user.
    /// It will help tracking all current session of an user also.
    /// </summary>
    public class UserRefreshToken: ModelBase, IRecordCreatedInfo
    {        
        /// <summary>
        /// Type of client for which user refresh token is generated!
        /// e.g. web browser or mobile application.
        /// </summary>
        public EnumClientType ClientType { get; set; }

        /// <summary>
        /// Client device id information. e.g. IP or mobile device key..
        /// </summary>
        [StringLength(128)]
        public String DeviceId { get; set; }

        /// <summary>
        /// User for which Refresh token is applicable to.
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Refresh token valid till given UTC time.. 
        /// null if valid always! null value useful when we need long running token for mobile application!
        /// </summary>
        public DateTime? ValidTill { get; set; }

        /// <summary>
        /// Random Refresh Token id/value!
        /// </summary>
        [Required]
        [StringLength(64)]
        public String RefreshToken { get; set; }


        /// <summary>
        /// Record created on DateTime.
        /// </summary>
        public DateTimeOffset CreatedOn { get; set; }


        /// <summary>
        /// Record created by UserId.
        /// </summary>
        public long? CreatedById { get; set; }



        /// <summary>
        /// Navigation property of User Table.
        /// </summary>
        [JsonIgnore]
        public virtual User User { get; set; }
    }
}
