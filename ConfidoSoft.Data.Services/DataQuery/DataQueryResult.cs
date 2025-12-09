using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConfidoSoft.Data.Services.DataQuery
{
    /// <summary>
    /// Model used to return response of any Data Query
    /// </summary>
    /// <typeparam name="T">Type of list item.</typeparam>
    public class DataQueryResult<T> where T : class
    {
        /// <summary>
        /// Filter records return to client. Based on current page and page size.
        /// </summary>
        public List<T> Items { get; set; }

        /// <summary>
        /// Total records in database which matching passed filter and sorting information.
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Items for Page number
        /// </summary>
        public int PageNo { get; set; }

        /// <summary>
        /// Number of records per page setting.
        /// </summary>
        public int Size { get; set; }
    }
}
