using ConfidoSoft.Data.Domain.Consts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConfidoSoft.Data.Domain.DBModels
{
    /// <summary>
    /// Permission Information for each actual storage Entity or any virtual section or department.
    /// </summary>
    public class RolePermission : ModelBase
    {
        /// <summary>
        /// Indicate to what permission apply to. e.g. Specific storage entity or virtual department in application.
        /// </summary>
        public EnumPermissionFor PermissionFor { get; set; }

        /// <summary>
        /// Role id FK
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// Define actual allowed Permissions for that entity.
        /// </summary>
        public EnumPermissions Permissions { get; set; }

        /// <summary>
        /// Navigation property for Role Table.
        /// </summary>
        [JsonIgnore]
        public virtual Role Role { get; set; }
    }
}
