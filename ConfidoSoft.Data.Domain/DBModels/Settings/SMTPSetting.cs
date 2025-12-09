using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace ConfidoSoft.Data.Domain.DBModels.Settings
{
    /// <summary>
    /// SMTP Setting for any Email to be send from application.
    /// </summary>
    public class SMTPSetting
    {
        public SMTPSetting()
        {
            IsSSL = true;
            IsUsingSendGrid = false;
        }

        /// <summary>
        /// SMTP UserName
        /// </summary>
        public String UserName { get; set; }

        /// <summary>
        /// Associated password.
        /// </summary>
        public String Password { get; set; }

        /// <summary>
        /// SMTP server address.
        /// </summary>
        public String ServerAddress { get; set; }

        /// <summary>
        /// SMTP port to connect to email server.
        /// </summary>
        public int ServerPort { get; set; }

        /// <summary>
        /// Indicate whether to use SSL or not while sending email.
        /// </summary>
        public bool IsSSL { get; set; }

        /// <summary>
        /// Form e-mail address which is to be used, while sending any e-mail alert.
        /// </summary>
        public String FromEmail { get; set; }

        /// <summary>
        /// Send Mail Using sendGrid or normally
        /// </summary>
        public bool IsUsingSendGrid { get; set; }

        /// <summary>
        ///Added few more SMTP related settings.
        /// </summary>
        public bool UseDefaultCredentials { get; set; }
        public SmtpDeliveryFormat DeliveryFormat { get; set; }
        public SmtpDeliveryMethod DeliveryMethod { get; set; }

        public String PickupDirectoryLocation { get; set; }

    }
}
