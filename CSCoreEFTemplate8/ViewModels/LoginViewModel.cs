using ConfidoSoft.Data.Domain.Consts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CSCoreEFTemplate8.ViewModels
{
    /// <summary>
    /// Base model for login.
    /// </summary>
    public class LoginBaseModel
    {
        /// <summary>
        /// Client type form where login is called. e.g. web or android mobile applicaiton or ios applicaiton.
        /// </summary>
        public EnumClientType ClientType { get; set; }

        /// <summary>
        /// DeviceId e.g. IP or mobile id..
        /// </summary>
        public String DeviceId { get; set; }

        /// <summary>
        /// Indicate that user login to be remembered. for us it will be infinity refreshToken time.
        /// </summary>
        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// Login with User Email and password!
    /// </summary>
    public class LoginViewModel : LoginBaseModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
