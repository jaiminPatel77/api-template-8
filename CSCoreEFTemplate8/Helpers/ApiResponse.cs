using ConfidoSoft.Data.Domain.Consts;
using ConfidoSoft.Infrastructure.Extensions;
using ConfidoSoft.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSCoreEFTemplate8.Helpers
{
    /// <summary>
    /// Base class for any responce to be return to client.
    /// https://www.devtrends.co.uk/blog/handling-errors-in-asp.net-core-web-api
    /// </summary>
    public class ApiResponse
    {
        public ApiResponse(EnumEntityType entityCode, EnumEntityEvents eventCode)
        {
            EntityCode = entityCode;
            EventCode = eventCode;            
            EventMessageId = eventCode.ToString();
        }
        public EnumEntityType EntityCode { get; }
        public EnumEntityEvents EventCode { get; }
        public String EventMessageId { get; protected set; }
    }

    /// <summary>
    /// Any ApiBadRequestResponse to be return to client
    /// </summary>
    public class ApiBadRequestResponse : ApiResponse
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Object ErrorDetail { get; }

        /// <summary>
        /// Generate bad Response for a given event of an entity with error detail as per modelState
        /// </summary>
        /// <param name="entityCode">application entity/table type code</param>
        /// <param name="eventCode">generic/specific event type</param>
        /// <param name="modelState">status of the model posted</param>
        public ApiBadRequestResponse(EnumEntityType entityCode, EnumEntityEvents eventCode, ModelStateDictionary modelState)
            : base(entityCode, eventCode)
        {
            StringBuilder errorDetail = new StringBuilder();
            foreach (var errorKeyValue in modelState)
            {
                errorDetail.Append(errorKeyValue.Key).Append(" : ");
                foreach (var error in errorKeyValue.Value.Errors)
                {
                    if (!String.IsNullOrEmpty(error.ErrorMessage))
                    {
                        errorDetail.Append(error.ErrorMessage);
                    }
                    else if (error.Exception != null)
                    {
                        errorDetail.Append(error.Exception.ToErrorMessage());
                    }
                    errorDetail.Append("! ");
                }
            }

            ErrorDetail = errorDetail.ToString();
            //ErrorDetail = String.Join("! ", modelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage));
        }

        /// <summary>
        /// Generate bad Response for a given event of an entity with error detail as per errorDetail string
        /// </summary>
        /// <param name="entityCode">application entity/table type code</param>
        /// <param name="eventCode">generic/specific event type</param>
        /// <param name="errorDetail">Addtional error detail as string</param>
        public ApiBadRequestResponse(EnumEntityType entityCode, EnumEntityEvents eventCode, String errorDetail)
          : base(entityCode, eventCode)
        {
            ErrorDetail = errorDetail;
        }

        /// <summary>
        /// Generate bad Response for a given event of an entity with error detail as per errorDetail object.
        /// </summary>
        /// <param name="entityCode">application entity/table type code</param>
        /// <param name="eventCode">generic/specific event type</param>
        /// <param name="errorDetail">Addtional error detail as object</param>
        public ApiBadRequestResponse(EnumEntityType entityCode, EnumEntityEvents eventCode, Object errorDetail)
          : base(entityCode, eventCode)
        {
            this.ErrorDetail = errorDetail;
        }

        /// <summary>
        /// Generate bad Response for a given event of an entity with error detail as per Exception object.
        /// </summary>
        /// <param name="entityCode">application entity/table type code</param>
        /// <param name="eventCode">generic/specific event type</param>
        /// <param name="error">Error/Exception detail</param>
        public ApiBadRequestResponse(EnumEntityType entityCode, EnumEntityEvents eventCode, Exception error)
           : base(entityCode, eventCode)
        {
            var csAppException = error as CSApplicationException;
            if(csAppException != null)
            {
                this.EventMessageId = csAppException.ErrorId;
            }
            ErrorDetail = error.ToErrorMessage();
        }
    }

    /// <summary>
    ///  Generate Valid/OK responce for a given event of an entity with Data to be return
    /// </summary>
    public class ApiOkResponse : ApiResponse
    {
        public Object Data { get; }

        public ApiOkResponse(EnumEntityType entityCode, EnumEntityEvents eventCode, Object result)
            : base(entityCode, eventCode)
        {
            Data = result;
        }
    }

    /// <summary>
    ///Generate Valid/OK Create responce for a given event of an entity with Data to be return with locaation for newly
    ///created resource URI
    /// </summary>
    public class ApiCreatedResponse : ApiOkResponse
    {
        /// <summary>
        /// Gets or sets the location at which the content has been created.
        /// </summary>
        public string Location { get; set; }

        public ApiCreatedResponse(EnumEntityType entityCode, EnumEntityEvents eventCode, Object result, String uri)
             : base(entityCode, eventCode, result)
        {
            Location = uri;
        }
    }
}
