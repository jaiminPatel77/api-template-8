using AutoMapper;
using ConfidoSoft.Data.Domain.Consts;
using ConfidoSoft.Data.Domain.Database;
using ConfidoSoft.Data.Domain.DBModels;
using ConfidoSoft.Data.Domain.Dtos;
using ConfidoSoft.Data.Services.BLServices;
using ConfidoSoft.Data.Services.Consts;
using ConfidoSoft.Data.Services.DataQuery;
using ConfidoSoft.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConfidoSoft.Data.Services.DBServices
{
    #region IBaseService<TEntity> interface

    /// <summary>
    /// Base interface for all the data service.
    /// </summary>
    /// <typeparam name="TEntity"> Entity type</typeparam>
    public interface IBaseService<TEntity> where TEntity : class, IModelBase
    {
        #region CURD

        /// <summary>
        /// Get the record without tracking.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TEntity> GetRecord(long id);

        /// <summary>
        /// Get the Dto record without tracking.
        /// </summary>
        /// <typeparam name="TEntityDto">Dto data type</typeparam>
        /// <param name="id">Entity primary key</param>
        /// <returns>Return Dto record if found otherwise throw exception</returns>
        Task<TEntityDto> GetRecord<TEntityDto>(long id) where TEntityDto : BaseDto;

        /// <summary>
        /// Add record to DbContext. It will not add to database.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        TEntity AddedInContext(TEntity record);

        /// <summary>
        /// Add the record to DBContext and call SaveChages to save record to database!
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        Task<TEntity> Create(TEntity record);


        /// <summary>
        /// Add new record using given instance of Dto object for a entity type.
        /// And add to database context but not fire save changes to actually create db record.
        /// </summary>
        /// <typeparam name="TEntityDto">Dto Type</typeparam>
        /// <param name="dtoRecord">Dto instance</param>
        /// <returns>db record with detail as per detail in dto record.</returns>
        TEntity AddedInContext<TEntityDto>(TEntityDto dtoRecord) where TEntityDto : BaseDto;


        /// <summary>
        /// Create new record using given instance of Dto object for a entity type.
        /// </summary>
        /// <typeparam name="TEntityDto">Dto Type</typeparam>
        /// <param name="dtoRecord">Dto instance</param>
        /// <returns>Dto record with detail as per record created in db.</returns>
        Task<TEntityDto> Create<TEntityDto>(TEntityDto dtoRecord) where TEntityDto : BaseDto;

        /// <summary>
        /// Put an instance of an entity. First get the record from Db context and merge the value from
        /// given input entity instance and also set auto field link last modified info.
        /// </summary>
        /// <param name="entity">An instance of an Entity to put in database.</param>
        /// <returns>Return the entity instance which is attached to db context for update.
        /// Return null if record not found in database.
        /// </returns>
        Task<TEntity> UpdateInContext(TEntity record);

        /// <summary>
        /// Same as updated, except that it also call SaveChange() to save record to database!
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        Task<TEntity> Update(TEntity record);


        /// <summary>
        /// Put an instance of an entity. First get the record from Db context and merge the value from
        /// given input dto instance and also set auto field like last modified info.
        /// Use auto-mapper to map dto to actual database record. 
        /// But not fire save changes to update the record to db.
        /// And save record to database.
        /// </summary>
        /// <typeparam name="TEntityDto">Dto type </typeparam>
        /// <param name="dtoRecord">Instance of Dto type.</param>
        /// <returns>Return the db record with updated detail as per information passed to dto.
        /// Throw exception if record not found in database</returns>
        Task<TEntity> UpdateInContext<TEntityDto>(TEntityDto dtoRecord) where TEntityDto : BaseDto;

        /// <summary>
        /// Put an instance of an entity. merge the value from
        /// given input dto instance and also set auto field like last modified info.
        /// Use auto-mapper to map dto to actual database record. 
        /// But not fire save changes to update the record to db.
        /// And save record to database.
        /// </summary>
        /// <typeparam name="TEntityDto">Dto type </typeparam>
        /// <param name="dtoRecord">Instance of Dto type.</param>
        /// <returns>Return the db record with updated detail as per information passed to dto.
        /// Throw exception if record not found in database</returns>
        TEntity UpdateInContext<TEntityDto>(TEntityDto dtoRecord, TEntity dbRecord) where TEntityDto : BaseDto;

        /// <summary>
        /// Put an instance of an entity. First get the record from Db context and merge the value from
        /// given input dto instance and also set auto field like last modified info.
        /// Use auto-mapper to map dto to actual database record. 
        /// And save record to database.
        /// </summary>
        /// <typeparam name="TEntityDto">Dto type </typeparam>
        /// <param name="dtoRecord">Instance of Dto type.</param>
        /// <returns>Return the dto instance as per detail saved to database.
        /// Throw exception if record not found in database</returns>
        Task<TEntityDto> Update<TEntityDto>(TEntityDto dtoRecord) where TEntityDto : BaseDto;

        /// <summary>
        /// Attached Entity to DbContect if not already and make it as delete.
        /// </summary>
        /// <param name="entity">An instance of an Entity.</param>
        TEntity RemoveInContext(TEntity record);

        /// <summary>
        /// Same as Remove + remove/delete record from database.
        /// </summary>
        /// <param name="entity">An instance of an Entity.</param>
        Task<TEntity> Remove(long id);

        /// <summary>
        /// save all modification done to context to database.
        /// </summary>
        /// <returns></returns>
        Task SaveChanges();

        #endregion

        #region Generic Data filter Query
        /// <summary>
        /// Generic filter list for Entity itself
        /// </summary>
        /// <param name="dataQueryOptions">Generic model to perform any data query using EF</param>
        /// <returns>List of the matching records</returns>
        Task<DataQueryResult<TEntity>> GetFilteredRecords(DataQueryOptions dataQueryOptions);

        /// <summary>
        /// Generic filter with custom select.
        /// </summary>
        /// <typeparam name="TResult">Typed object for custom select</typeparam>
        /// <param name="dataQueryOptions">Generic model to perform any data query using EF</param>
        /// <param name="selectExpression">Represent Expression for custom select using EF </param>
        /// <returns>List of the matching records</returns>
        Task<DataQueryResult<TResult>> GetFilteredRecords<TResult>(DataQueryOptions dataQueryOptions, Expression<Func<TEntity, TResult>> selectExpression) where TResult : class;

        #endregion
    }
    #endregion

    #region IBaseService<TEntity> implementation 

    /// <summary>
    /// Base class for all the data service.
    /// </summary>
    /// <typeparam name="TEntity"> Entity type</typeparam>
    public abstract class BaseService<TEntity> : IBaseService<TEntity> where TEntity : class, IModelBase
    {
        #region Protected member
        protected static Type entityType = typeof(TEntity);
        protected readonly ILogger _logger;
        protected readonly ApplicationDbContext _dbContext;
        protected readonly ICurrentUserInfo _currentUserInfo;
        protected readonly IMapper _mapper;
        private readonly DbSet<TEntity> _dbSet;
        #endregion

        #region ctor

        public BaseService(ILogger logger, ApplicationDbContext dbContext, ICurrentUserInfo currentUserInfo, IMapper mapper)
        {
            _logger = logger;
            _dbContext = dbContext;
            _currentUserInfo = currentUserInfo;
            _mapper = mapper;
            _dbSet = _dbContext.Set<TEntity>();
        }
        #endregion

        #region Common CURD

        /// <summary>
        /// set common fields for update/create.
        /// Update the last modified information if model implement IRecordModifiedInfo interface.
        /// </summary>
        /// <param name="record">Any IModelBase record.</param>
        public virtual void SetRecordModifiedInfo(IModelBase record)
        {
            IRecordModifiedInfo recordModifiedInfo = record as IRecordModifiedInfo;
            if (recordModifiedInfo != null)
            {
                recordModifiedInfo.ModifiedOn = DateTimeOffset.UtcNow;
                if (_currentUserInfo.Id != null)
                {
                    recordModifiedInfo.ModifiedById = _currentUserInfo.Id;
                }
            }
        }


        /// <summary>
        /// Set the last modified info. avoid created by field update and row version handling 
        /// </summary>
        /// <param name="record"></param>
        public virtual void PreProcessingOnRecordUpdate(IModelBase dbRecord, IModelBase modifiedRecord)
        {
            SetRecordModifiedInfo(dbRecord);

            if (modifiedRecord != null)
            {
                IRowVersion modifiedEntityRowVersion = modifiedRecord as IRowVersion;
                //if ((modifiedEntityRowVersion != null) && (modifiedEntityRowVersion.RowVersion != null))
                if ((modifiedEntityRowVersion != null) && (modifiedEntityRowVersion.RowVersion > 0))
                {
                    var entry = this._dbContext.Entry(dbRecord);
                    entry.OriginalValues[nameof(modifiedEntityRowVersion.RowVersion)] = modifiedEntityRowVersion.RowVersion;
                }
                IRecordCreatedInfo modifiedCreatedInfo = modifiedRecord as IRecordCreatedInfo;
                IRecordCreatedInfo dbRecordCreatedInfo = dbRecord as IRecordCreatedInfo;

                // Don't allow to reset/update the created info during update operation
                if ((modifiedCreatedInfo != null) && (dbRecordCreatedInfo != null))
                {
                    modifiedCreatedInfo.CreatedOn = dbRecordCreatedInfo.CreatedOn;
                    modifiedCreatedInfo.CreatedById = dbRecordCreatedInfo.CreatedById;
                }
            }
        }


        /// <summary>
        /// set common fields for create.
        /// </summary>
        /// <param name="record">record to be create</param>
        public virtual void SetRecordCreatedInfo(IModelBase record)
        {
            IRecordCreatedInfo recordCreatedInfo = record as IRecordCreatedInfo;
            if (recordCreatedInfo != null)
            {
                recordCreatedInfo.CreatedOn = DateTimeOffset.UtcNow;
                if (_currentUserInfo.Id != null)
                {
                    recordCreatedInfo.CreatedById = _currentUserInfo.Id;
                }
            }
        }

        /// <summary>
        /// Get the record without tracking.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TEntity> GetRecord(long id)
        {
            return await this.TableNoTracking.Where(e => e.Id == id).FirstOrDefaultAsync();
        }


        /// <summary>
        /// Get the Dto record without tracking.
        /// </summary>
        /// <typeparam name="TEntityDto">Dto data type</typeparam>
        /// <param name="id">Entity primary key</param>
        /// <returns>Return Dto record if found otherwise throw exception</returns>
        public async Task<TEntityDto> GetRecord<TEntityDto>(long id) where TEntityDto : BaseDto
        {
            var dbRecord = await this.TableNoTracking.Where(e => e.Id == id).FirstOrDefaultAsync();
            if (dbRecord != null)
            {
                var dtoRecord = _mapper.Map<TEntity, TEntityDto>(dbRecord);
                return dtoRecord;
            }
            else
            {
                throw new CSApplicationException(EnumEntityEvents.COMMON_GET_ITEM_NOTFOUND.ToString(), CommonConstStr.COMMON_ITEM_NOTFOUND);
            }
        }

        /// <summary>
        /// Add record to DbContext. It will not add to database. (i.e. Don't fire SaveChanges).
        /// </summary>
        /// <param name="record">record to be add in db context</param>
        /// <returns>Record added in db context</returns>
        public TEntity AddedInContext(TEntity record)
        {
            SetRecordCreatedInfo(record);
            SetRecordModifiedInfo(record);

            EntityEntry<TEntity> entry = _dbContext.Entry<TEntity>(record);
            if ((entry == null) || (entry.State == EntityState.Detached)) // not already attached?
            {
                DbSet.Add(record);
            }
            else
            {
                throw new Exception("Add twice!!");
            }
            return record;
        }


        /// <summary>
        /// Add the record to DBContext and call SaveChages to save record to database!
        /// </summary>
        /// <param name="record">record to be create</param>
        /// <returns>record created.</returns>
        public async Task<TEntity> Create(TEntity record)
        {
            record = AddedInContext(record);
            await this.SaveChanges();
            return record;
        }


        /// <summary>
        /// Add new record using given instance of Dto object for a entity type.
        /// And add to database context but not fire save changes to actually create db record.
        /// </summary>
        /// <typeparam name="TEntityDto">Dto Type</typeparam>
        /// <param name="dtoRecord">Dto instance</param>
        /// <returns>db record with detail as per detail in dto record.</returns>
        public TEntity AddedInContext<TEntityDto>(TEntityDto dtoRecord) where TEntityDto : BaseDto
        {
            SetRecordCreatedInfo(dtoRecord);
            SetRecordModifiedInfo(dtoRecord);
            var dbRecord = _mapper.Map<TEntityDto, TEntity>(dtoRecord);
            EntityEntry<TEntity> entry = _dbContext.Entry<TEntity>(dbRecord);
            if ((entry == null) || (entry.State == EntityState.Detached)) // not already attached?
            {
                DbSet.Add(dbRecord);
            }
            else
            {
                throw new Exception("Add twice!!");
            }            
            return dbRecord;
        }

        /// <summary>
        /// Create new record using given instance of Dto object for a entity type.
        /// And fire save changes to create database record.
        /// </summary>
        /// <typeparam name="TEntityDto">Dto Type</typeparam>
        /// <param name="dtoRecord">Dto instance</param>
        /// <returns>Dto record with detail as per record created in db.</returns>
        public async Task<TEntityDto> Create<TEntityDto>(TEntityDto dtoRecord) where TEntityDto : BaseDto
        {
            var dbRecord =  this.AddedInContext(dtoRecord);
            await this.SaveChanges();
            //set auto increment key as applicable.
            dtoRecord.Id = dbRecord.Id;
            return dtoRecord;
        }

        /// <summary>
        /// Get the record from database using PK.
        /// Perform EF find operation to get record from database.
        /// i.e. Finds an entity with the given primary key values. If an entity with the given
        /// primary key values is being tracked by the context, then it is returned immediately
        /// without making a request to the database. Otherwise, a query is made to the database
        /// for an entity with the given primary key values and this entity, if found, is
        /// attached to the context and returned. If no entity is found, then null is returned.
        /// </summary>
        /// <param name="entity">Entity type</param>
        /// <returns>The entity found, or null.</returns>
        public virtual async Task<TEntity> GetAssociatedDbRecord(long id)
        {
            return await this.DbSet.FindAsync(id);
        }


        /// <summary>
        /// Put an instance of an entity. First get the record from Db context and merge the value from
        /// given input entity instance and also set auto field link last modified info.
        /// </summary>
        /// <param name="entity">An instance of an Entity to put in database.</param>
        /// <returns>Return the entity instance which is attached to db context for update.
        /// Return null if record not found in database.
        /// </returns>
        public virtual async Task<TEntity> UpdateInContext(TEntity record)
        {
            var dbRecord = await GetAssociatedDbRecord(record.Id);
            if (dbRecord != null)
            {
                PreProcessingOnRecordUpdate(dbRecord, record);
                var entry = this._dbContext.Entry(dbRecord);
                //Apply the current values from modified entity record.
                entry.CurrentValues.SetValues(record);
            }
            else
            {
                throw new CSApplicationException(EnumEntityEvents.COMMON_UPDATE_ITEM_NOTFOUND.ToString(), CommonConstStr.COMMON_ITEM_NOTFOUND);
            }
            return dbRecord;
        }


        /// <summary>
        /// Same as updated, except that it also call SaveChange() to save record to database!
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> Update(TEntity record)
        {
            var retVal = await this.UpdateInContext(record);
            await this.SaveChanges();
            return retVal;
        }

        /// <summary>
        /// Put an instance of an entity. First get the record from Db context and merge the value from
        /// given input dto instance and also set auto field like last modified info.
        /// Use auto-mapper to map dto to actual database record
        /// </summary>
        /// <param name="entity">An instance of an Entity to put in database.</param>
        /// <returns>Return the entity instance which is attached to db context for update.
        /// Return null if record not found in database.
        /// </returns>


        /// <summary>
        /// Put an instance of an entity. First get the record from Db context and merge the value from
        /// given input dto instance and also set auto field like last modified info.
        /// Use auto-mapper to map dto to actual database record. 
        /// But not fire save changes to update the record to db.
        /// And save record to database.
        /// </summary>
        /// <typeparam name="TEntityDto">Dto type </typeparam>
        /// <param name="dtoRecord">Instance of Dto type.</param>
        /// <returns>Return the db record with updated detail as per information passed to dto.
        /// Throw exception if record not found in database</returns>
        public async Task<TEntity> UpdateInContext<TEntityDto>(TEntityDto dtoRecord) where TEntityDto : BaseDto
        {
            SetRecordModifiedInfo(dtoRecord);
            var dbRecord = await this.GetAssociatedDbRecord(dtoRecord.Id);
            return UpdateInContext(dtoRecord, dbRecord);
        }

        /// <summary>
        /// Put an instance of an entity. merge the value from
        /// given input dto instance and also set auto field like last modified info.
        /// Use auto-mapper to map dto to actual database record. 
        /// But not fire save changes to update the record to db.
        /// And save record to database.
        /// </summary>
        /// <typeparam name="TEntityDto">Dto type </typeparam>
        /// <param name="dtoRecord">Instance of Dto type.</param>
        /// <returns>Return the db record with updated detail as per information passed to dto.
        /// Throw exception if record not found in database</returns>
        public TEntity UpdateInContext<TEntityDto>(TEntityDto dtoRecord, TEntity dbRecord) where TEntityDto : BaseDto
        {            
            if (dbRecord != null)
            {
                this.PreProcessingOnRecordUpdate(dbRecord, dtoRecord);
                _mapper.Map<TEntityDto, TEntity>(dtoRecord, dbRecord);
                return dbRecord;
            }
            else
            {
                throw new CSApplicationException(EnumEntityEvents.COMMON_UPDATE_ITEM_NOTFOUND.ToString(), CommonConstStr.COMMON_ITEM_NOTFOUND);
            }
        }



        /// <summary>
        /// Put an instance of an entity. First get the record from Db context and merge the value from
        /// given input dto instance and also set auto field like last modified info.
        /// Use auto-mapper to map dto to actual database record. 
        /// And save record to database.
        /// </summary>
        /// <typeparam name="TEntityDto">Dto type </typeparam>
        /// <param name="dtoRecord">Instance of Dto type.</param>
        /// <returns>Return the dto instance as per detail saved to database.
        /// Throw exception if record not found in database</returns>
        public async Task<TEntityDto> Update<TEntityDto>(TEntityDto dtoRecord) where TEntityDto : BaseDto         
        {
            //order status as invoice
            //recordDto.Status = EnumOrderStatus.Draft;
            var dbRecord = await this.UpdateInContext(dtoRecord);
            await this.SaveChanges();
            return dtoRecord;
        }

        /// <summary>
        /// Attached Entity to DbContect if not already and make it as delete.
        /// </summary>
        /// <param name="entity">An instance of an Entity.</param>
        public TEntity RemoveInContext(TEntity record)
        {
            var entry = this._dbContext.Entry(record);
            if ((entry == null) || (entry.State == EntityState.Detached)) // not already attached?
            {
                this._dbContext.Attach(record);
            }
            this.DbSet.Remove(record);
            return record;
        }


        /// <summary>
        /// Attached Entity to DbContect if not already and make it as delete.
        /// </summary>
        /// <param name="entity">An instance of an Entity.</param>
        public async Task<TEntity> Remove(long id)
        {
            var dbRecord = await this.DbSet.FindAsync(id);
            if (dbRecord != null)
            {
                this.DbSet.Remove(dbRecord);
                await this.SaveChanges();
            }
            else
            {
                throw new CSApplicationException(EnumEntityEvents.COMMON_DELETE_ITEM_NOTFOUND.ToString(), CommonConstStr.COMMON_ITEM_NOTFOUND);
            }
            return dbRecord;
        }

        /// <summary>
        /// Save all modification from DbContext to database.
        /// </summary>
        public async Task SaveChanges()
        {
            await this._dbContext.SaveChangesAsync();
        }
        #endregion
        
        #region List of Records using IQueryable

        /// <summary>
        /// Gets a table
        /// </summary>
        public IQueryable<TEntity> Table
        {
            get
            {
                return this.DbSet;
            }
        }

        /// <summary>
        /// Gets a table with "no tracking" enabled (EF feature) Use it only when you load record(s) only for read-only operations
        /// </summary>
        public IQueryable<TEntity> TableNoTracking
        {
            get
            {
                return this.DbSet.AsNoTracking();
            }
        }

        /// <summary>
        /// DbSet for specific table.
        /// </summary>
        protected DbSet<TEntity> DbSet
        {
            get
            {
                return _dbSet;
            }
        }

        #endregion

        #region Generic DataQuery Implementation

        /// <summary>
        /// Get the property information of filedName in associated Entity type.
        /// </summary>
        /// <param name="fieldName">Name of the field. It can include '.' to support selection of single
        /// child property detail.</param>
        /// <returns> PropertyInfo if field found otherwise null.</returns>
        public virtual PropertyInfo GetPropertyInfo(string fieldName)
        {
            if (fieldName.IndexOf(ServiceConst.PropertySeparator) >= 0)
            {
                PropertyInfo propertyInfo = null;
                Type propertyOf = entityType; //start with root.
                foreach (var member in fieldName.Split(ServiceConst.PropertySeparator))
                {
                    propertyInfo = propertyOf.GetProperty(member, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    propertyOf = propertyInfo.PropertyType;
                }
                return propertyInfo;
            }
            else
            {
                return entityType.GetProperty(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            }
        }

        /// <summary>
        /// Get the Default Sorting column Name.
        /// Not sued for now!
        /// </summary>
        /// <returns>Column Name</returns>
        //public virtual string GetOrderByFieldName()
        //{
        //    return ServiceConst.Id;
        //}


        /// <summary>
        /// Generic filter list for Entity itself
        /// </summary>
        /// <param name="dataQueryOptions">Generic model to perform any data query using EF</param>
        /// <returns>List of the matching records</returns>
        public async Task<DataQueryResult<TEntity>> GetFilteredRecords(DataQueryOptions dataQueryOptions)
        {
            IQueryable<TEntity> filteredEntities = GetFilterQuery(dataQueryOptions);
            IQueryable<TEntity> orderEntities = GetOrderByQuery(filteredEntities, dataQueryOptions);
            DataQueryResult<TEntity> result = null;
            //Prepares the result.
            result = new DataQueryResult<TEntity>();
            //Prepare the select clause.
            if (dataQueryOptions.Size > 0)
            {
                var filterList = await orderEntities.Skip(dataQueryOptions.SkipCount).Take(dataQueryOptions.Size).ToListAsync();
                result.Items = filterList;
                result.TotalRecords = filteredEntities.Count();
                result.PageNo = dataQueryOptions.PageNo;
                result.Size = dataQueryOptions.Size;
            }
            else
            {
                //Return all matching record in case of size in not define!
                var filterList = await orderEntities.ToListAsync();
                result.Items = filterList;
                result.TotalRecords = filterList.Count;
                result.PageNo = 1;
                result.Size = filterList.Count;
            }
            return result;
        }

        /// <summary>
        /// Generic filter with custom select.
        /// </summary>
        /// <typeparam name="TResult">Typed object for custom select</typeparam>
        /// <param name="dataQueryOptions">Generic model to perform any data query using EF</param>
        /// <param name="selectExpression">Represent Expression for custom select using EF </param>
        /// <returns>List of the matching records</returns>
        public async Task<DataQueryResult<TResult>> GetFilteredRecords<TResult>(DataQueryOptions dataQueryOptions, Expression<Func<TEntity, TResult>> selectExpression) where TResult : class
        {
            IQueryable<TEntity> filteredEntities = GetFilterQuery(dataQueryOptions);
            IQueryable<TEntity> orderEntities = GetOrderByQuery(filteredEntities, dataQueryOptions);
            DataQueryResult<TResult> result = null;            
            //Prepares the result.
            result = new DataQueryResult<TResult>();
            if(dataQueryOptions.Size > 0)
            {
                //Prepare the select clause.
                var filterList = await orderEntities.Skip(dataQueryOptions.SkipCount).Take(dataQueryOptions.Size).Select(selectExpression).ToListAsync();
                result.Items = filterList;
                result.TotalRecords = filteredEntities.Count();
                result.PageNo = dataQueryOptions.PageNo;
                result.Size = dataQueryOptions.Size;

            }
            else
            {
                //Return all matching record in case of size in not define!
                var filterList = await orderEntities.Select(selectExpression).ToListAsync();
                result.Items = filterList;
                result.TotalRecords = filterList.Count;
                result.PageNo = 1;
                result.Size = filterList.Count;
            }
            return result;
        }

        /// <summary>
        /// generate filter query from given generic Data query option.
        /// </summary>
        /// <param name="dataQueryOptions">Generic model to perform any data query using EF</param>
        /// <returns>Return IQuerable by apply filter as per query option input.</returns>
        public IQueryable<TEntity> GetFilterQuery(DataQueryOptions dataQueryOptions)
        {
            Expression<Func<TEntity, bool>> filterExpression = null;
            IQueryable<TEntity> filteredEntities = null;
            //Gets the Queryable.
            var queryable = this.TableNoTracking;

            //Apply filtering if applicable
            if (dataQueryOptions.Filter != null)
            {
                List<Expression<Func<TEntity, bool>>> customExpression = new List<Expression<Func<TEntity, bool>>>();
                //Prepares the expression for the filtering.
                filterExpression = this.GetFilterExpression(dataQueryOptions.Filter, customExpression);

                if (filterExpression != null)
                {
                    //Apply the filtering expression to the Queryable.
                    filteredEntities = queryable.Where(filterExpression);
                }
                else
                {
                    //Apply the filtering expression to the Queryable.
                    filteredEntities = queryable;
                }
                if (customExpression.Count > 0)
                {
                    foreach (var item in customExpression)
                    {
                        if (item != null)
                        {
                            filteredEntities = filteredEntities.Where(item);
                        }
                    }
                }
            }
            else
            {
                filteredEntities = queryable;
            }
            //Add base filter expression if any need.
            //filteredEntities = AddBaseFilterExpression(filteredEntities);
            filteredEntities = AddBaseFilterExpression(filteredEntities, dataQueryOptions);
            return filteredEntities;
        }

        /// <summary>
        /// generate order by query from given generic Data query option.
        /// </summary>
        /// <param name="dataQueryOptions">Generic model to perform any data query using EF</param>
        /// <returns>Return IQuerable by apply order by as per query option input.</returns>
        public IQueryable<TEntity> GetOrderByQuery(IQueryable<TEntity> filteredEntities, DataQueryOptions dataQueryOptions)
        {
            IQueryable<TEntity> orderEntities = null;
            //Apply the sorting
            orderEntities = GetOrderByExpression(filteredEntities, dataQueryOptions.OrderBy);
            //If no sorting is applied then apply the default Sorting
            if (orderEntities == null)
            {
                orderEntities = filteredEntities.OrderByDescending(e => e.Id);
                //List<OrderByField> orderOptions = 
                //    new List<OrderByField>
                //    {
                //        new OrderByField
                //        {
                //            Name = this.GetOrderByFieldName(),
                //            Direction = EnumOrderByDirection.Ascending
                //        }
                //    };
                //orderEntities = GetOrderByExpression(filteredEntities, orderOptions);
                //orderEntities = filteredEntities;
            }
            return orderEntities;
        }

        /// <summary>
        /// actual method to get filter expression.
        /// </summary>
        /// <param name="filter">list of the columns on which filter is to be created.</param>
        /// <param name="customExpression"> list in which custom expression/filters to be added.</param>
        /// <returns> Return IQuerable by apply filter as per query option input. </returns>
        protected Expression<Func<TEntity, bool>> GetFilterExpression(List<FilterByField> filter, List<Expression<Func<TEntity, bool>>> customExpression)
        {
            Expression<Func<TEntity, bool>> returnExpression = null;
            if ((filter == null) || (filter.Count == 0))
            {
                //do nothing.. return null
            }
            else
            {
                ParameterExpression param = Expression.Parameter(entityType, "t");
                Expression finalExpression = null;
                Expression currentExpression = null;
                var previousLogicalOperator = EnumLogicalOperator.AND;

                foreach (var filterByField in filter)
                {
                    if (filterByField.ConditionalOperator == EnumConditionalOperator.Custom)
                    {
                        var customExp = GetCustomExpression(filterByField);
                        customExpression.Add(customExp);
                    }
                    else
                    {
                        currentExpression = GetExpression(param, filterByField);
                        if (finalExpression == null)
                        {
                            finalExpression = currentExpression;
                        }
                        else
                        {
                            if (previousLogicalOperator == EnumLogicalOperator.AND)
                            {
                                finalExpression = Expression.AndAlso(finalExpression, currentExpression);
                            }
                            else
                            {
                                finalExpression = Expression.OrElse(finalExpression, currentExpression);
                            }
                        }
                        previousLogicalOperator = filterByField.LogicalOperator;
                    }
                }
                if (finalExpression != null)
                {
                    returnExpression = Expression.Lambda<Func<TEntity, bool>>(finalExpression, param);
                }                
            }
            return returnExpression;
        }

        /// <summary>
        /// Must override if any entity specific filter required for a table query!..
        /// e.g. EnterpriseId also known as TenantId filter to return relevant records only.        
        /// </summary>
        /// <returns> same as passed filter by default. </returns>
        protected virtual IQueryable<TEntity> AddBaseFilterExpression(IQueryable<TEntity> filterExp)
        {
            //Do nothing.
            return filterExp;
        }

        /// <summary>
        /// Must override if any entity specific filter required for a table query!..
        /// e.g. EnterpriseId also known as TenantId filter to return relevant records only.
        /// Also handle 
        /// 1) the case of disable/Archive record views.
        /// 2) Global/default search text filter implementation as relevant. 
        /// </summary>
        /// <param name="filterExp">filter exp based on filter input</param>
        /// <param name="dataQueryOptions">data query option posted from client.</param>
        /// <returns> same as passed filter by default. </returns>
        protected virtual IQueryable<TEntity> AddBaseFilterExpression(IQueryable<TEntity> filterExp, DataQueryOptions dataQueryOptions)
        {
            //Do nothing.
            return filterExp;
        }

        /// <summary>
        /// Must override to handle the custom filterByField in child service.
        /// </summary>
        /// <param name="filterByField"></param>
        /// <returns></returns>
        protected virtual Expression<Func<TEntity, bool>> GetCustomExpression(FilterByField filterByField)
        {
            throw new Exception("Custom filterByField in not implemented by derived service!");
        }

        /// <summary>
        /// Creates the IOrderedQueryable<T> from the passed in sorting info.
        /// </summary>
        /// <typeparam name="TEntity">Type or Entity on which the sorting is to be applied</typeparam>
        /// <typeparam name="Tkey">The Type of Key Property for the Entity</typeparam>
        /// <param name="items">The source IQueryable<T> on which the sorting needs to be applied</param>
        /// <param name="orderByFieldList">The List of SortOption containing the sorting Info</param>
        /// <returns></returns>
        protected IOrderedQueryable<TEntity> GetOrderByExpression(IQueryable<TEntity> items, List<OrderByField> orderByFieldList)
        {
            IOrderedQueryable<TEntity> returnQueryable = null;
            //If sortingList is null then no sorting needs to be applied so return immediately
            if ((orderByFieldList == null) || (orderByFieldList.Count == 0))
            {
                //Do nothing or return null
            }
            else
            {
                IOrderedQueryable<TEntity> orderedList = null;
                ParameterExpression param = Expression.Parameter(entityType, "e");

                foreach (var orderByField in orderByFieldList)
                {
                    PropertyInfo propertyInfo = this.GetPropertyInfo(orderByField.Name);
                    Type propertyType = propertyInfo.PropertyType;
                    Type delegateType = typeof(Func<,>).MakeGenericType(entityType, propertyType);
                    LambdaExpression expr = Expression.Lambda(delegateType, CreateExpression(param, orderByField.Name), param);
                    string methodName = string.Empty;
                    //No ordering is applied then use 'OrderBy' for applying the sorting
                    MethodInfo orderByMethodInfo = null;
                    if (orderedList == null)
                    {
                        //Apply sorting according to the sorting direction 
                        if (orderByField.Direction == EnumOrderByDirection.Descending)
                        {
                            orderByMethodInfo = ServiceConst.OrderByDescendingMethod;
                        }
                        else
                        {
                            orderByMethodInfo = ServiceConst.OrderByMethod;
                        }
                        orderedList = (IOrderedQueryable<TEntity>)orderByMethodInfo
                            .MakeGenericMethod(typeof(TEntity), propertyType)
                            .Invoke(null, new object[] { items, expr });
                    }
                    else
                    {
                        //Once the ordering is applied then use 'ThenBy' for applying the sorting
                        //Apply sorting according to the sorting direction 
                        if (orderByField.Direction == EnumOrderByDirection.Descending)
                        {
                            orderByMethodInfo = ServiceConst.ThenByDescendingMethod;
                        }
                        else
                        {
                            orderByMethodInfo = ServiceConst.ThenByMethod;
                        }
                        orderedList = (IOrderedQueryable<TEntity>)orderByMethodInfo
                            .MakeGenericMethod(entityType, propertyType)
                            .Invoke(null, new object[] { orderedList, expr });
                    }

                }
                //return the final ordered result
                returnQueryable = orderedList;
            }
            return returnQueryable;
        }

        /// <summary>
        /// Gets/Constructs the Lambda expression depending upon the passed Filter object. 
        /// </summary>
        /// <typeparam name="TEntity">Entity Type</typeparam>
        /// <param name="param"></param>
        /// <param name="filterByField"></param>
        /// <returns></returns>
        private Expression GetExpression(ParameterExpression param, FilterByField filterByField)
        {
            Expression member = CreateExpression(param, filterByField.Name);
            object columnValue = null;
            PropertyInfo propertyInfo = this.GetPropertyInfo(filterByField.Name);
            Type columnType = propertyInfo.PropertyType;
            if (columnType.IsEnum)
            {
                columnValue = Enum.Parse(columnType, filterByField.Value);
            }
            else if (columnType.IsGenericType && columnType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                //
                Type colType = columnType.GetGenericArguments()[0];
                Type dateTimeOffsetType = typeof(DateTimeOffset);
                if (colType.IsEnum)
                {
                    columnValue = Enum.Parse(colType, filterByField.Value);
                }
                else if (colType == dateTimeOffsetType)
                {
                    DateTimeOffsetConverter dateTimeOffsetConverter = new DateTimeOffsetConverter();
                    columnValue = dateTimeOffsetConverter.ConvertFromInvariantString(filterByField.Value);
                }
                else
                {
                    columnValue = Convert.ChangeType(filterByField.Value, colType, CultureInfo.InvariantCulture);
                }               

            }
            else
            {
                Type dateTimeOffsetType = typeof(DateTimeOffset);
                if (columnType == dateTimeOffsetType)
                {
                    DateTimeOffsetConverter dateTimeOffsetConverter = new DateTimeOffsetConverter();
                    columnValue = dateTimeOffsetConverter.ConvertFromInvariantString(filterByField.Value);
                }
                else
                {
                    columnValue = Convert.ChangeType(filterByField.Value, columnType, CultureInfo.InvariantCulture);
                }               
            }

            ConstantExpression constant = Expression.Constant(columnValue, columnType);

            switch (filterByField.ConditionalOperator)
            {
                case EnumConditionalOperator.Equal:
                    {
                        //return Expression.Equal(member, constant);
                        if (columnType == ServiceConst.StringType)
                        {
                            //PK need special handing..
                            if (filterByField.Name == ServiceConst.Id)
                            {
                                return Expression.Equal(member, constant);
                            }
                            else
                            {
                                var toLowerExpression = Expression.Call(member, ServiceConst.ToLowerMethod);
                                return Expression.Equal(toLowerExpression, constant);
                            }
                        }
                        else
                        {
                            return Expression.Equal(member, constant);
                        }
                    }

                case EnumConditionalOperator.NotEqual:
                    {
                        //return Expression.NotEqual(member, constant);
                        if (columnType == ServiceConst.StringType)
                        {
                            var toLowerExpression = Expression.Call(member, ServiceConst.ToLowerMethod);
                            return Expression.NotEqual(toLowerExpression, constant);
                        }
                        else
                        {
                            return Expression.NotEqual(member, constant);
                        }
                    }
                case EnumConditionalOperator.GreaterThan:
                    return Expression.GreaterThan(member, constant);

                case EnumConditionalOperator.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(member, constant);

                case EnumConditionalOperator.LessThan:
                    return Expression.LessThan(member, constant);

                case EnumConditionalOperator.LessThanOrEqual:
                    return Expression.LessThanOrEqual(member, constant);

                case EnumConditionalOperator.Contains:
                    {
                        var toLowerExpression = Expression.Call(member, ServiceConst.ToLowerMethod);
                        return Expression.Call(toLowerExpression, ServiceConst.ContainsMethod, constant);
                        //return Expression.Call(member, ServiceConst.ContainsMethod, constant);
                    }
                case EnumConditionalOperator.NotContains:
                    {
                        //return Expression.Not(Expression.Call(member, ServiceConst.ContainsMethod, constant));
                        var toLowerExpression = Expression.Call(member, ServiceConst.ToLowerMethod);
                        return Expression.Not(Expression.Call(toLowerExpression, ServiceConst.ContainsMethod, constant));
                    }

                case EnumConditionalOperator.StartsWith:
                    {
                        //return Expression.Call(member, ServiceConst.StartsWithMethod, constant);
                        var toLowerExpression = Expression.Call(member, ServiceConst.ToLowerMethod);
                        return Expression.Call(toLowerExpression, ServiceConst.StartsWithMethod, constant);
                    }

                case EnumConditionalOperator.EndsWith:
                    {
                        //return Expression.Call(member, ServiceConst.EndsWithMethod, constant);
                        var toLowerExpression = Expression.Call(member, ServiceConst.ToLowerMethod);
                        return Expression.Call(toLowerExpression, ServiceConst.EndsWithMethod, constant);
                    }
            }

            return null;
        }

        /// <summary>
        /// Create Member Expression including case of nested property. e.g. "ComplexProperty.Child1.Child2"
        /// </summary>
        /// <param name="param"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        static Expression CreateExpression(ParameterExpression param, string fieldName)
        {
            Expression body = param;
            if (fieldName.IndexOf(ServiceConst.PropertySeparator) >= 0)
            {
                foreach (var member in fieldName.Split(ServiceConst.PropertySeparator))
                {
                    body = Expression.PropertyOrField(body, member);
                }
            }
            else
            {
                body = Expression.PropertyOrField(body, fieldName);
            }
            return body;
        }


        /// <summary>
        /// get the Binary expression using two filterColumnInfo and associated logic condition and or.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="param"></param>
        /// <param name="filter1"></param>
        /// <param name="filter2"></param>
        /// <param name="logicalCondition"></param>
        /// <returns></returns>
        private BinaryExpression GetExpression(ParameterExpression param, FilterByField filter1, FilterByField filter2, EnumLogicalOperator logicalCondition)
        {
            Expression bin1 = GetExpression(param, filter1);
            Expression bin2 = GetExpression(param, filter2);

            if (logicalCondition == EnumLogicalOperator.AND)
            {
                return Expression.AndAlso(bin1, bin2);
            }
            else
            {
                return Expression.OrElse(bin1, bin2);
            }
        }

        #endregion

    }

    #endregion

}
