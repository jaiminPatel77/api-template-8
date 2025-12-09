using System;
using System.Collections.Generic;
using System.Text;

namespace ConfidoSoft.Data.Domain.Consts
{
    /// <summary>
    /// Define status of setting record.
    /// </summary>
    public enum EnumSettingStatus
    {
        /// <summary>
        /// Indicate that record is yet not created.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Indicate that setting record is created. but not tested.
        /// </summary>
        Created = 1,

        /// <summary>
        /// Indicate that setting are tested and test is failed.
        /// </summary>
        TestFailed = 2,

        /// <summary>
        /// Setting are completed. e.g. Tested and OK if application allow to test settings.
        /// </summary>
        Complete = 3

    }
}
