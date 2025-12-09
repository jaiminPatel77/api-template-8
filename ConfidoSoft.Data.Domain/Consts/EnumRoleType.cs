using System;
using System.Collections.Generic;
using System.Text;

namespace ConfidoSoft.Data.Domain.Consts
{
    /// <summary>
    /// Type of roles
    /// </summary>
    public enum EnumRoleType
    {
        /// <summary>
        /// Custom role. Allowed permission are as per rights selection of that role.
        /// </summary>
        CustomRole = 0,

        /// <summary>
        /// Global Administrator role. Has access to everything.
        /// </summary>
        GlobalAdministrator = 1,

        //User = 2,
    }
}
