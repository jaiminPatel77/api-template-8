using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConfidoSoft.Data.Services.DataQuery
{
    /// <summary>
    /// Field/Property filter information
    /// </summary>
    public class FilterByField
    {
        /// <summary>
        /// Name of the column/property/field
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// Condition Operator to be used to compare the value of a given field.
        /// </summary>
        public EnumConditionalOperator ConditionalOperator { get; set; }

        /// <summary>
        /// Value of the column
        /// </summary>
        public String Value { get; set; }

        /// <summary>
        /// Logical Condition Operator to be used to con-cat the next FilterColumnInfo.
        /// </summary>
        public EnumLogicalOperator LogicalOperator { get; set; }
    }


    /// <summary>
    /// Column sorting information.
    /// </summary>
    public class OrderByField
    {
        /// <summary>
        /// Name of the column/field
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// By default ascending sorting. True if descending sorting to be applied.
        /// </summary>
        public EnumOrderByDirection Direction { get; set; }
    }


    /// <summary>
    /// Generic model to perform any data query using EF.
    /// Perform data query to get list of matching records with paging and sorting as required.
    /// </summary>
    public class DataQueryOptions
    {
        /// <summary>
        /// List of columns on which filter is to be applied.
        /// </summary>
        public List<FilterByField> Filter { get; set; }

        /// <summary>
        /// List of columns on which sorting is to be applied.
        /// </summary>
        public List<OrderByField> OrderBy { get; set; }

        /// <summary>
        /// Page number to be displayed. Start from 1
        /// </summary>
        public int PageNo { get; set; }

        //Number of records per page.
        public int Size { get; set; }

        /// <summary>
        /// Return the record to be skipped in Link query.
        /// </summary>
        /// <returns></returns>
        public int SkipCount
        {
            get
            {
                int skipCount = 0;
                if (PageNo > 1)
                {
                    skipCount = (PageNo - 1) * Size;
                }
                return skipCount;
            }
        }

    }
}
