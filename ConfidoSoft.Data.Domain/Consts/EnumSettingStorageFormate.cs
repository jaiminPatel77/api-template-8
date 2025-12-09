using System;
using System.Collections.Generic;
using System.Text;

namespace ConfidoSoft.Data.Domain.Consts
{
    /// <summary>
    /// Storage formate of the settings.
    /// </summary>
    public enum EnumSettingStorageFormate
    {
        /// <summary>
        /// By default stored as string value.
        /// </summary>
        StringFormat,

        /// <summary>
        /// Store value in JSON format in database.
        /// </summary>
        JSONFormat,
    }
}
