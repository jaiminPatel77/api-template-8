using AutoMapper;
using ConfidoSoft.Data.Domain.Consts;
using ConfidoSoft.Data.Domain.Database;
using ConfidoSoft.Data.Domain.DBModels;
using ConfidoSoft.Data.Domain.Dtos;
using ConfidoSoft.Data.Services.BLServices;
using ConfidoSoft.Data.Services.DataQuery;
using ConfidoSoft.Data.Services.Helpers;
using ConfidoSoft.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ConfidoSoft.Data.Services.DBServices
{
    #region IUserRefreshTokenService interface

    /// <summary>
    /// UserRefreshToken Entity Service Interface.
    /// User refresh token related methods.
    /// Allow to store refresh token id for each unique client session. 
    /// Which allow to view all user live session and manually terminate 
    /// a session by admin.
    /// </summary>
    public interface IUserRefreshTokenService : IBaseService<UserRefreshToken>
    {
        /// <summary>
        /// Create refresh token record into database.
        /// </summary>
        /// <param name="record">user refresh token detail.</param>
        /// <returns>return record created into database.</returns>
        Task<UserRefreshToken> CreateRefreshToken(UserRefreshToken record);

        /// <summary>
        /// remove all the user refresh token record(s) from database if they are 
        /// already expired.
        /// </summary>
        /// <param name="userId">User id of which expired refresh token records to be removed form db.</param>
        /// <param name="clientType">client type for which token records to be deleted.</param>
        Task RemoveExpiredTokens(long userId, EnumClientType clientType);

        /// <summary>
        /// Get the refresh token record for given value. 
        /// if found and not expired then return the record otherwise return null..
        /// </summary>
        /// <param name="refreshToken">refresh token value for which record to be fetch from database.</param>
        /// <returns>if found and not expired then return the record otherwise return null</returns>
        Task<UserRefreshToken> GetRefreshToken(String refreshToken);

        /// <summary>
        /// Remove record associated with given refresh token value.
        /// </summary>
        /// <param name="refreshToken">Unique refresh token Id/value</param>
        Task RemoveRefreshToken(String refreshToken);

        /// <summary>
        /// Generate new token from existing token if old token found in database.
        /// otherwise do noting and return null.
        /// </summary>
        /// <param name="existingToken">existing refresh token</param>
        /// <param name="newRefreshToken">new refresh_token string</param>
        /// <param name="validTill">Valid till if any otherwise null for infinity</param>
        /// <returns>new token or null value</returns>
        Task<UserRefreshToken> RefreshToken(UserRefreshToken existingToken, String newRefreshToken, DateTime? validTill);
    }
    #endregion

    #region UserService class

    /// <summary>
    /// UserRefreshToken Entity Service.
    /// User refresh token related methods.
    /// Allow to store refresh token id for each unique client session. 
    /// Which allow to view all user live session and manually terminate 
    /// a session by admin.
    /// </summary>
    public class UserRefreshTokenService : BaseService<UserRefreshToken>, IUserRefreshTokenService
    {

        #region ctor
        public UserRefreshTokenService(ApplicationDbContext dbContext,
            ILogger<UserRefreshTokenService> logger,
            ICurrentUserInfo currentUserInfo,
            IMapper mapper) : base(logger, dbContext, currentUserInfo, mapper)
        {
        }
        #endregion

        #region Entity Specific Methods

        /// <summary>
        /// Create refresh token record into database.
        /// </summary>
        /// <param name="record">user refresh token detail.</param>
        /// <returns>return record created into database.</returns>
        public async Task<UserRefreshToken> CreateRefreshToken(UserRefreshToken record)
        {
            return await base.Create(record);
        }

        /// <summary>
        /// remove all the user refresh token record(s) from database if they are 
        /// already expired.
        /// </summary>
        /// <param name="userId">User id of which expired refresh token records to be removed form db.</param>
        /// <param name="clientType">client type for which token records to be deleted.</param>
        public async Task RemoveExpiredTokens(long userId, EnumClientType clientType)
        {
            try
            {
                var oldTokens = await this._dbContext.UserRefreshTokens.Where(e => (e.UserId == userId && e.ClientType == clientType)
                && (e.ValidTill != null && e.ValidTill < DateTime.UtcNow)).ToListAsync();
                this._dbContext.UserRefreshTokens.RemoveRange(oldTokens);
                await this._dbContext.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                //do nothing.. if error...
                this._logger.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Get the refresh token record for given value. 
        /// if found and not expired then return the record otherwise return null..
        /// </summary>
        /// <param name="refreshToken">refresh token value for which record to be fetch from database.</param>
        /// <returns>if found and not expired then return the record otherwise return null</returns>
        public async Task<UserRefreshToken> GetRefreshToken(String refreshToken)
        {
            try
            {
                var existingToken = await this._dbContext.UserRefreshTokens.Where(e => e.RefreshToken == refreshToken).FirstOrDefaultAsync();
                if (existingToken != null)
                {
                    if (existingToken.ValidTill == null || existingToken.ValidTill >= DateTime.UtcNow)
                    {
                        //Existing refresh token is valid so allow it to refreshed.
                        //otherwise don't return such token to refresh token.
                        this._logger.LogInformation($"Valid refersh Token Id:{refreshToken}");
                        return existingToken;
                    }
                    else
                    {
                        this._dbContext.UserRefreshTokens.Remove(existingToken);
                        this._logger.LogInformation($"Expired refersh Token Id:{refreshToken}");
                        await this._dbContext.SaveChangesAsync();
                        this._logger.LogInformation($"Expired refersh Token Id:{refreshToken} deleted successs.");
                    }
                }
            }
            catch(Exception ex)
            {
                //Do nothing..
                this._logger.LogError(ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Remove record associated with given refresh token value.
        /// </summary>
        /// <param name="refreshToken">Unique refresh token Id/value</param>
        public async Task RemoveRefreshToken(String refreshToken)
        {   
            try
            {
                var existingToken = await this._dbContext.UserRefreshTokens.Where(e => e.RefreshToken == refreshToken).FirstOrDefaultAsync();
                if (existingToken != null)
                {
                    this._dbContext.UserRefreshTokens.Remove(existingToken);
                    await this._dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                //Do nothing..
                this._logger.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Generate new token from existing token if old token found in database.
        /// otherwise do noting and return null.
        /// </summary>
        /// <param name="existingToken">existing refresh token</param>
        /// <param name="newRefreshToken">new refresh_token string</param>
        /// <param name="validTill">Valid till if any otherwise null for infinity</param>
        /// <returns>new token or null value</returns>
        public async Task<UserRefreshToken> RefreshToken(UserRefreshToken existingToken, String newRefreshToken, DateTime? validTill)
        {
            this._logger.LogInformation($"RefreshToken called for existing token: {existingToken.RefreshToken}, new token {newRefreshToken}, valid till: {validTill} ");
            var oldToken = await this._dbContext.UserRefreshTokens.FindAsync(existingToken.Id);
            if (oldToken != null)
            {
                UserRefreshToken newToken = new UserRefreshToken();
                newToken.UserId = oldToken.UserId;
                newToken.RefreshToken = newRefreshToken;
                newToken.DeviceId = oldToken.DeviceId;
                newToken.ClientType = oldToken.ClientType;
                newToken.ValidTill = validTill;
                newToken.CreatedById = oldToken.CreatedById;
                newToken.CreatedOn = DateTime.UtcNow;
                this._dbContext.UserRefreshTokens.Remove(oldToken);
                this._dbContext.UserRefreshTokens.Add(newToken);
                this._logger.LogInformation($"RefreshToken called before save existing token: {existingToken.RefreshToken}, new token {newRefreshToken}, valid till: {validTill} ");
                this._dbContext.SaveChanges();
                this._logger.LogInformation($"RefreshToken called after save existing token: {existingToken.RefreshToken}, new token {newRefreshToken}, valid till: {validTill} ");
                return newToken;
            }
            else
            {
                //TODO: throw exception instead of null ?
                return null;
            }
        }

        #endregion

        #region Must override methods

        #endregion

    }

    #endregion

}