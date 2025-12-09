using System;
using System.Collections.Generic;
using System.Text;

namespace ConfidoSoft.Data.Domain.Consts
{
    /// <summary>
    /// Unique Id for each Entity/Table we support for an Application.
    /// </summary>
    public enum EnumEntityType
    {
        /// <summary>
        /// Default and which is not used!
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// User entity/table unique id.
        /// </summary>
        USER = 1,

        /// <summary>
        /// Role  entity/table unique id.
        /// </summary>
        ROLE = 2,

        /// <summary>
        /// Setting entity/table unique id.
        /// </summary>
        SETTING = 3,

        /// <summary>
        /// User refresh token entity/table unique id.
        /// </summary>
        USERREFRESHTOKEN = 4,
    }
}
