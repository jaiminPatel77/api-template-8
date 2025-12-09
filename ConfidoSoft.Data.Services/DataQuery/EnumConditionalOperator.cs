using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConfidoSoft.Data.Services.DataQuery
{

    /// <summary>
    /// Condition Operator used for column value filter
    /// </summary>
    public enum EnumConditionalOperator
    {
        /// <summary>
        /// Refer to equal condition operator.
        /// Associated string name in query string 'Eq'
        /// </summary>
        Equal,

        /// <summary>
        /// Refer to not equal condition operator.
        /// Associated string name in query string 'Ne'
        /// </summary>
        NotEqual,

        /// <summary>
        /// Refer to greater than condition operator.
        /// Associated string name in query string 'Gt'
        /// </summary>
        GreaterThan,

        /// <summary>
        /// Refer to greater than or equal condition operator.
        /// Associated string name in query string 'Ge'
        /// </summary>
        GreaterThanOrEqual,

        /// <summary>
        /// Refer to less than condition operator.
        /// Associated string name in query string 'Lt'
        /// </summary>
        LessThan,

        /// <summary>
        /// Refer to less than or equal to condition operator.
        /// Associated string name in query string 'Le'
        /// </summary>
        LessThanOrEqual,

        /// <summary>
        /// Refer to Contains condition operator.
        /// Associated string name in query string 'Contains'
        /// </summary>
        Contains,

        /// <summary>
        /// Refer to not contains condition operator.
        /// Associated string name in query string 'NotContains'
        /// </summary>
        NotContains,

        /// <summary>
        /// Refer to Start With condition operator.
        /// Associated string name in query string 'StartWith'
        /// </summary>
        StartsWith,

        /// <summary>
        /// Refer to End With condition operator.
        /// Associated string name in query string 'EndWith'
        /// </summary>
        EndsWith,

        /// <summary>
        /// Refer to custom condition operator.
        /// Associated string name in query string 'Custom'
        /// You can implement custom filter as you need. e.g. IN()
        /// </summary>
        Custom,
    }
}
