using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConfidoSoft.Data.Domain.DBModels
{

    #region Common Interfaces

    #region Base interface for all entity

    /// <summary>
    /// Base interface for all Entity in Database.
    /// Common name for PK for all table.
    /// </summary>
    public interface IModelBase
    {

        /// <summary>
        /// Primary Key
        /// </summary>
        long Id { get; set; }
    }

    #endregion

    #region Interface to support recording record last updated information.  

    /// <summary>
    /// Entity recording record last updated information.
    /// </summary>
    public interface IRecordModifiedInfo
    {
        /// <summary>
        /// Record Last Modified On DateTime information.
        /// </summary>
        DateTimeOffset ModifiedOn { get; set; }

        /// <summary>
        /// Record last modified by UserId
        /// </summary>
        long? ModifiedById { get; set; }
    }

    #endregion

    #region Interface to support recording record creation information.

    /// <summary>
    /// Entity recording record created information.
    /// </summary>
    public interface IRecordCreatedInfo
    {
        /// <summary>
        /// Record created DateTime information.
        /// </summary>
        DateTimeOffset CreatedOn { get; set; }

        /// <summary>
        /// Record created by UserId
        /// </summary>
        long? CreatedById { get; set; }
    }

    #endregion

    #region Interface to support concurrency checking while updating records to database.

    /// <summary>
    /// Interface to implement concurrency check.
    /// i.e. during update if record is modified by other user, application should not update the record and throw
    /// error back!
    /// </summary>
    public interface IRowVersion {

        /// <summary>
        /// RowVersion information.
        /// </summary>
        long RowVersion { get; set; }
    }

    #endregion

    #region Interface for recording Address information.

    /// <summary>
    /// Interface which define address information of any things.
    /// </summary>
    public interface IAddress
    {
        /// <summary>
        /// Address line 1
        /// </summary>
        string AddressLine1 { get; set; }
        
        /// <summary>
        /// Address line 2
        /// </summary>
        string AddressLine2 { get; set; }

        /// <summary>
        /// City Name
        /// </summary>
        string City { get; set; }

        /// <summary>
        /// State name.
        /// </summary>
        string State { get; set; }

        /// <summary>
        /// Country Name
        /// </summary>
        string Country { get; set; }

        /// <summary>
        /// Postal Code detail.
        /// </summary>
        string PostalCode { get; set; }

        /// <summary>
        /// Address Latitude information.
        /// </summary>
        double? Lat { get; set; }

        /// <summary>
        /// Address longitude information.
        /// </summary>
        double? Lng { get; set; }

        /// <summary>
        /// Email associated with address.
        /// </summary>
        string Email { get; set; }

        /// <summary>
        /// Phone number associated with address.
        /// </summary>
        string Phone { get; set; }

        /// <summary>
        /// Mobile number associated with address.
        /// </summary>
        string Mobile { get; set; }
    }
    #endregion

    #region Interface for entity with name and Description 

    /// <summary>
    /// Interface for entity having name as Description field value!
    /// </summary>
    public interface IModelBaseNameDescription : IModelBase
    {
        /// <summary>
        /// Unique Name of an entity. 
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Description of an entity.
        /// </summary>
        string Description { get; set; }
    }

    #endregion
    
    #region Interface for entity with are Enterprise specific or Tenant specific

    /// <summary>
    /// Interface indicate that Table is a child of Enterprise or
    /// only have Tenant specific records.
    /// </summary>
    public interface IEnterpriseChild : IModelBase
    {
        /// <summary>
        /// EnterpriseId ad TenantId
        /// </summary>
        long EnterpriseId { get; set; }
    }

    /// <summary>
    /// Interface indicate that Table is a child of Enterprise or
    /// it has global records also.
    /// </summary>
    public interface IOptionalEnterpriseChild : IModelBase
    {
        /// <summary>
        /// EnterpriseId ad TenantId
        /// </summary>
        long? EnterpriseId { get; set; }
    }

    #endregion

    #endregion

    #region Base Model/class Definitions

    #region Base model for all entity

    /// <summary>
    /// Base class for all Entity of a Domain.
    /// </summary>
    public class ModelBase : IModelBase
    {
        /// <summary>
        /// PK field.
        /// </summary>
        [Required]
        public long Id { get; set; }
    }

    #endregion

    #region Base model for one time creation type entities.

    /// <summary>
    /// Base class for all entity where we don't need to record created information. 
    /// e.g. One time creation and not required row version as update is not applicable or automatic.
    /// </summary>
    public class ModelBaseWithCreatedInfoFields : ModelBase, IRecordCreatedInfo
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
    
    #region Base model for most of the entity which support CURD operations!

    /// <summary>
    /// Base class for Entity with common fields
    /// </summary>
    public class ModelBaseWithCommonFields : ModelBaseWithCreatedInfoFields, IRecordModifiedInfo, IRowVersion
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

        [Timestamp]
        /// <summary>
        /// Record row version to support concurrency update for each record!
        /// </summary>
        public long RowVersion { get; set; }
    }

    #endregion

    #region Base model for entity with address.

    /// <summary>
    /// Base model for all entity having address information.
    /// </summary>
    public class ModelBaseWithAddress : ModelBaseWithCommonFields, IAddress {

        /// <summary>
        /// Address line 1
        /// </summary>
        [StringLength (128)]
        public string AddressLine1 { get; set; }

        /// <summary>
        /// Address line 2
        /// </summary>
        [StringLength (128)]
        public string AddressLine2 { get; set; }

        /// <summary>
        /// City Name
        /// </summary>
        [StringLength (64)]
        public string City { get; set; }

        /// <summary>
        /// State Name
        /// </summary>
        [StringLength (64)]
        public string State { get; set; }

        /// <summary>
        /// Country Name
        /// </summary>
        [StringLength (64)]
        public string Country { get; set; }

        /// <summary>
        /// Postal Code information.
        /// </summary>
        [StringLength (16)]
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
        [StringLength (64)]
        public string Email { get; set; }

        /// <summary>
        /// Phone associated with address.
        /// </summary>
        [StringLength (15)]
        public string Phone { get; set; }

        /// <summary>
        /// Mobile associated with address.
        /// </summary>
        [StringLength (15)]
        public string Mobile { get; set; }
    }

    #endregion

    #region Base Model for entity having Name and Description

    /// <summary>
    /// Base model/class for all entity with name and description field.
    /// </summary>
    public class ModelBaseNameDescription : ModelBaseWithCommonFields, IModelBaseNameDescription
    {
        /// <summary>
        /// Unique Name of an entity. 
        /// </summary>
        [Required]
        [StringLength (128)]
        public string Name { get; set; }

        /// <summary>
        /// Description of entity.
        /// </summary>
        [StringLength (512)]
        public string Description { get; set; }
    }
   

    #endregion

    #endregion

}