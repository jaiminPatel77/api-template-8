using System;
using System.Collections.Generic;
using System.Text;

namespace ConfidoSoft.Data.Domain.DBModels
{
    /// <summary>
    /// User Profile table. Has PK same as User and One-To-One relation between User and UserProfile.
    /// </summary>
    public class UserProfile : ModelBase
    {
        /// <summary>
        /// Image information as base64. Or event one can set whatever we need as application needs.
        /// e.g. URI in case of large image.
        /// </summary>
        public String Image { get; set; }

        /// <summary>
        /// Navigation property to User Table.
        /// </summary>
        public virtual User User { get; set; }
    }
}
