using System;
using System.Collections.Generic;
using System.Text;

namespace ConfidoSoft.Infrastructure.Models
{
    /// <summary>
    /// Base class for all Application specific exception
    /// </summary>
    public class CSApplicationException : ApplicationException
    {
        /// <summary>
        /// Used to translate error or give help about given error message id.
        /// </summary>
        public String ErrorId { get; }

        /// <summary>
        /// Application specific exception
        /// </summary>
        /// <param name="errorId"> unique error id.</param>
        /// <param name="message">Additional error message detail</param>
        public CSApplicationException(string errorId, string message) : base(message)
        {
            this.ErrorId = errorId;
        }

        /// <summary>
        /// Create Application specific exception from other exception.
        /// </summary>
        /// <param name="errorId">unique error id. Used to translate error by client application!</param>
        /// <param name="message">additional error message detail</param>
        /// <param name="innerException">Inner exception detail!</param>
        public CSApplicationException(string errorId, string message, Exception innerException) : base(message, innerException)
        {
            this.ErrorId = errorId;
        }
    }
}
