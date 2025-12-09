using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConfidoSoft.Data.Services.DataQuery
{

    /// <summary>
    /// Logical Operator used to separate two filter expressions.
    /// </summary>
    public enum EnumLogicalOperator
    {
        /// <summary>
        /// Represent logical AND Operator. This is default.
        /// Associated string name in query string 'And'
        /// </summary>
        AND,

        /// <summary>
        /// Represent logical OR Operator.
        /// Associated string name in query string 'Or'
        /// </summary>
        OR
    }
}
