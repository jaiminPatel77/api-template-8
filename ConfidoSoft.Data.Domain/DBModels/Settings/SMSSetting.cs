using System;
using System.Collections.Generic;
using System.Text;

namespace ConfidoSoft.Data.Domain.DBModels.Settings
{
    /// <summary>
    /// SMS setting for Twilio/ASPSMS
    /// </summary>
    public class SMSSetting
    {
        /// <summary>
        /// Account Identification as part of SMS setting.
        /// </summary>
        public string AccountIdentification { get; set; }

        /// <summary>
        /// Associated password for SMS account.
        /// </summary>
        public string AccountPassword { get; set; }

        /// <summary>
        /// Account or Number from where one send the SMS alert to user!
        /// </summary>
        public string AccountFrom { get; set; }
    }
}
