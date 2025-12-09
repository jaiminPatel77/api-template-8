using ConfidoSoft.Data.Domain.Consts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSCoreEFTemplate8.Helpers
{
    /// <summary>
    /// ApiResponseExtension to create common response type for any API controllers!
    /// </summary>
    public static class ApiResponseExtension
    {
        /// <summary>
        /// Entity event information for logging.
        /// </summary>
        public static String EntityEventString = "Entity : {0}, Event: {1} ";
        public static String EntityEventStringWithError = "Entity : {0}, Event: {1}, Error:{2}";

        /// <summary>
        /// Create OK response
        /// </summary>
        /// <param name="controler"></param>
        /// <param name="result">API result to be send as Data property back to client</param>
        /// <returns>OkObjectResult</returns>
        public static OkObjectResult OkResponse(this ControllerBase controler, 
            EnumEntityType entityCode, EnumEntityEvents eventCode, Object result)
        {
            return controler.Ok(new ApiOkResponse(entityCode, eventCode, result));
        }

        /// <summary>
        /// Create OK response for new uri or resource.
        /// </summary>
        public static CreatedResult CreatedResponse(this ControllerBase controler,
            EnumEntityType entityCode, EnumEntityEvents eventCode, Object result, String uri)
        {
            return controler.Created(uri, new ApiCreatedResponse(entityCode, eventCode, result, uri));
        }

        /// <summary>
        /// Create Bad request with given string error detail
        /// </summary>
        public static BadRequestObjectResult CreateBadRequest(this ControllerBase controler,
            EnumEntityType entityCode, EnumEntityEvents eventCode, ILogger logger)
        {
            if (logger != null)
            {
                logger.LogError((int)entityCode, EntityEventString, entityCode, eventCode);
            }
            return controler.BadRequest(new ApiResponse(entityCode, eventCode));
        }

        /// <summary>
        /// Create Bad request with given string error detail
        /// </summary>
        public static BadRequestObjectResult CreateBadRequest(this ControllerBase controler,
            EnumEntityType entityCode, EnumEntityEvents eventCode, String errorDetail, ILogger logger)
        {
            if (logger != null)
            {
                logger.LogError((int)entityCode, EntityEventStringWithError, entityCode, eventCode, errorDetail);
            }
            return controler.BadRequest(new ApiBadRequestResponse(entityCode, eventCode, errorDetail));
        }


        /// <summary>
        /// Create Bad request with given exception error detail
        /// </summary>
        public static BadRequestObjectResult CreateBadRequest(this ControllerBase controler,
            EnumEntityType entityCode, EnumEntityEvents eventCode, Exception errorDetail, ILogger logger)
        {
            if (logger != null)
            {
                logger.LogError((int)eventCode, errorDetail, EntityEventString, entityCode, eventCode);
            }
            return controler.BadRequest(new ApiBadRequestResponse(entityCode, eventCode, errorDetail));
        }

        /// <summary>
        /// Create Bad request with given error object detail
        /// </summary>
        public static BadRequestObjectResult CreateBadRequest(this ControllerBase controler,
            EnumEntityType entityCode, EnumEntityEvents eventCode, Object errorDetail, ILogger logger)
        {
            if (logger != null)
            {
                logger.LogError((int)entityCode, EntityEventStringWithError, entityCode, eventCode, errorDetail);
            }
            return controler.BadRequest(new ApiBadRequestResponse(entityCode, eventCode, errorDetail));
        }

        /// <summary>
        /// Create Bad request with given error using ModelStateDictionary detail
        /// </summary>
        public static BadRequestObjectResult CreateBadRequest(this ControllerBase controler,
            EnumEntityType entityCode, EnumEntityEvents eventCode, ModelStateDictionary modelState, ILogger logger)
        {
            var retVal = new ApiBadRequestResponse(entityCode, eventCode, modelState);
            if (logger != null)
            {
                logger.LogError((int)entityCode, EntityEventStringWithError, entityCode, eventCode, retVal.ErrorDetail);
            }
            return controler.BadRequest(retVal);
        }
    }
}
