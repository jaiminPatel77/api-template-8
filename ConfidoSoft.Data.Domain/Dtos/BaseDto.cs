using ConfidoSoft.Data.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

/// <summary>
/// File define all commonly used DTO model which will be used as base DTO model for 
/// entity specific API as appropriate.
/// </summary>
namespace ConfidoSoft.Data.Domain.Dtos
{
    #region Base DTO for all Object having PK

    /// <summary>
    /// Base class/model for all DTO for domain entity.
    /// DTO object are used to transfer data to any client. It can have
    /// properties which are relevant for specific use case.
    /// </summary>
    public class BaseDto : IModelBase
    {
        /// <summary>
        /// Primary Key
        /// </summary>
        [Required]
        public long Id { get; set; }
    }

    #endregion

    #region Base DTO for one time creation type entities.

    /// <summary>
    /// Base class for all entity where we don't need to record last modified information. 
    /// e.g. One time creation and not required row version as update is not applicable or automatic.
    /// Use it as base class as required for associated Entity for CURD operation.
    /// </summary>
    public class BaseDtoWithCreatedInfoFields : BaseDto, IRecordCreatedInfo
    {
        /// <summary>
        /// Record created on DateTime.
        /// </summary>
        public DateTimeOffset CreatedOn { get; set; }

        /// <summary>
        /// Record created by UserId.
        /// </summary>
        public long? CreatedById { get; set; }
    }

    #endregion


    #region Base DTO for most of the entity which support CURD operations!

    /// <summary>
    /// Base DTO class for Entity with common fields. Use it as appropriate for CURD related 
    /// API for an entity.
    /// </summary>
    public class BaseDtoWithCommonFields : BaseDtoWithCreatedInfoFields, IRecordModifiedInfo, IRowVersion
    {
        /// <summary>
        /// Record Modified On DateTime. For newly created record it will be same as created on.
        /// </summary>
        public DateTimeOffset ModifiedOn { get; set; }

        /// <summary>
        /// Record Modified by UserId.
        /// </summary>
        public long? ModifiedById { get; set; }

        /// <summary>
        /// Indicate that record is disable. e.g. Not allowed to update or uses
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// DateTime when record was last Disabled or Enable again!
        /// </summary>
        public DateTimeOffset? EnabledDisabledOn { get; set; }

        /// <summary>
        /// Record row version to support concurrency update for each record!
        /// </summary>
        public long RowVersion { get; set; }

    }

    #endregion


    #region Base DTO for entity with address.

    /// <summary>
    /// Base DTO for all entity having address information.
    /// </summary>
    public class BaseDtoWithAddress : BaseDtoWithCommonFields, IAddress
    {

        /// <summary>
        /// Address line 1
        /// </summary>
        [StringLength(128)]
        public string AddressLine1 { get; set; }

        /// <summary>
        /// Address line 2
        /// </summary>
        [StringLength(128)]
        public string AddressLine2 { get; set; }

        /// <summary>
        /// City Name
        /// </summary>
        [StringLength(64)]
        public string City { get; set; }

        /// <summary>
        /// State Name
        /// </summary>
        [StringLength(64)]
        public string State { get; set; }

        /// <summary>
        /// Country Name
        /// </summary>
        [StringLength(64)]
        public string Country { get; set; }

        /// <summary>
        /// Postal Code information.
        /// </summary>
        [StringLength(16)]
        public string PostalCode { get; set; }

        /// <summary>
        ///  Address Latitude information.
        /// </summary>
        public double? Lat { get; set; }

        /// <summary>
        /// Address longitude information.
        /// </summary>
        public double? Lng { get; set; }

        /// <summary>
        /// Email associated with address.
        /// </summary>
        [StringLength(64)]
        public string Email { get; set; }

        /// <summary>
        /// Phone associated with address.
        /// </summary>
        [StringLength(15)]
        public string Phone { get; set; }

        /// <summary>
        /// Mobile associated with address.
        /// </summary>
        [StringLength(15)]
        public string Mobile { get; set; }
    }

    #endregion
    
    #region Base DTO for entity having Name and Description field

    /// <summary>
    /// Base model/class for all entity with name and description field.
    /// </summary>
    public class BaseDtoWithNameDescription : BaseDtoWithCommonFields, IModelBaseNameDescription
    {
        /// <summary>
        /// Unique Name of an entity. 
        /// </summary>
        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        /// <summary>
        /// Description of entity.
        /// </summary>
        [StringLength(512)]
        public string Description { get; set; }
    }


    #endregion
    
    #region Base DTO for Any lookup

    /// <summary>
    /// Base model/class for all lookup for any entity with name and id field.
    /// </summary>
    public class BaseLookUpDto : BaseDto
    {
        /// <summary>
        /// Name of an entity. 
        /// </summary>
        public string Name { get; set; }
    }

    #endregion    

}
