using System;
using System.Collections.Generic;
using System.Text;

namespace ConfidoSoft.Data.Domain.Consts
{
    /// <summary>
    /// Indicate permission applied to/for which entity or department.
    /// Note: Values from 1-1000 will be used for entity specific permission.
    /// Value > 1001 are for any virtual entity or department will be used.
    /// </summary>
    public enum EnumPermissionFor
    {
        //Note: Values from 1-999 for entity specific value as required.
        //i.e. 

        /// <summary>
        /// Indicate that permission is for User entity.
        /// </summary>
        USER = 1,

        /// <summary>
        /// Indicate that permission is for Role Entity.
        /// </summary>
        ROLE = 2,

        /// <summary>
        /// Indicate that Permission is for Setting Entity.
        /// </summary>
        SETTING = 3,

        //Note: Value > 1000 for any department or virtual entity type value!
    }

    /// <summary>
    /// Bit flag values for each rights supported in application.
    /// e.g. View,Update,Create,Delete,Approve rights..
    /// </summary>
    [Flags]
    public enum EnumPermissions
    {
        #region Standard permissions

        /// <summary>
        /// Specifies No rights.
        /// </summary>
        None = 0x00000000,

        /// <summary>
        /// Specifies View/Read access. e.g. Allow to view record or information.
        /// </summary>
        ViewAccess = 0x00000001,

        /// <summary>
        /// Specifies Update access. e.g. Allow to Update existing record/information.
        /// </summary>
        UpdateAccess = 0x00000002,

        /// <summary>
        /// Specifies View and Update access e.g. Allow to Update or view existing records.
        /// </summary>
        ViewUpdateAccess = 0x00000003,

        /// <summary>
        /// Specifies Create access. e.g. Allow to create new records.
        /// </summary>
        CreateAccess = 0x00000004,

        /// <summary>
        /// Specifies Create and View access. e.g. Allow to view and Create record.
        /// </summary>
        ViewCreateAccess = 0x00000005,

        /// <summary>
        /// Specifies the right to create, update and view a record.
        /// </summary>
        ViewUpdateCreateAccess = 0x00000007,

        /// <summary>
        /// Specifies the right to delete records.
        /// </summary>
        DeleteAccess = 0x00000008,

        /// <summary>
        /// Specifies Create,update,read/view, delete rights.
        /// </summary>
        CURDAccess = 0x0000000F,

        /// <summary>
        /// Specifies Approve access for that entity.
        /// </summary>
        ApproveAccess = 0x00000010,

        #endregion

    }
}
