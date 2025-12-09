using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConfidoSoft.Data.Services.DataQuery
{
    /// <summary>
    /// Data Query Helper class to re-factor the sorting and filtering information from 
    /// URL string.
    /// </summary>
    public static class DataQueryOptionsHelper
    {
        public const String EqSplit = ",eq,";
        public const String NeSplit = ",ne,";
        public const String GtSplit = ",gt,";
        public const String GeSplit = ",ge,";
        public const String LtSplit = ",lt,";
        public const String LeSplit = ",le,";
        public const String AndSplit = ",and,";
        public const String OrSplit = ",or,";
        public const String ContainsSplit = ",contains,";
        public const String NotContainsSplit = ",notcontains,";
        public const String StartsWithSplit = ",startswith,";
        public const String EndsWithSplit = ",endswith,";
        public const String Custom = ",custom,";

        public const String FieldSeparator = ",";
        public const String SortOrderAsc = " asc";
        public const String SortOrderDesc = " desc";


        /// <summary>
        /// Get filter object from Query String arguments
        /// </summary>
        /// <param name="filter">filter string</param>
        /// <param name="orderBy">order by string</param>
        /// <param name="pageNo">page number.</param>
        /// <param name="size">number of records to be return.</param>
        /// <returns></returns>
        public static DataQueryOptions FillDataQueryOptions(string filter = null, string orderBy = null,
            int pageNo = 1, int size = -1)
        {
            DataQueryOptions  dataQueryOptions = new DataQueryOptions();;
             dataQueryOptions.Filter = FillFilterByList(filter);
             dataQueryOptions.OrderBy = FillOrderByList(orderBy);
             dataQueryOptions.PageNo = pageNo;
             dataQueryOptions.Size = size;
            return  dataQueryOptions;
        }

        /// <summary>
        /// Fill filterByField list from filter expression
        /// </summary>
        /// <param name="filter">filter string.</param>
        /// <returns></returns>
        private static List<FilterByField> FillFilterByList(string filter)
        {
            List<FilterByField> filterByFieldList = null;
            if (false == String.IsNullOrEmpty(filter))
            {
                filterByFieldList = new List<FilterByField>();

                var filterString = filter.ToLowerInvariant().Trim();
                while (false == String.IsNullOrEmpty(filterString))
                {
                    var andIndex = filterString.IndexOf(AndSplit, StringComparison.InvariantCultureIgnoreCase);
                    var orIndex = filterString.IndexOf(OrSplit, StringComparison.InvariantCultureIgnoreCase);
                    var oneFilterInfo = String.Empty;
                    string[] splitedFilterStrings = null;
                    String currentFilterInfo = null;
                    FilterByField filterbyField = null;

                    filterbyField = new FilterByField();

                    if ((andIndex != -1) && (orIndex != -1))
                    {
                        splitedFilterStrings = andIndex > orIndex ? filterString.Split(new string[] { OrSplit }, 2, StringSplitOptions.RemoveEmptyEntries) :
                                                                    filterString.Split(new string[] { AndSplit }, 2, StringSplitOptions.RemoveEmptyEntries);

                        filterbyField.LogicalOperator = andIndex > orIndex ? EnumLogicalOperator.OR : EnumLogicalOperator.AND;
                        currentFilterInfo = splitedFilterStrings[0].Trim();
                        filterString = splitedFilterStrings[1];
                    }
                    else if (andIndex != -1)
                    {
                        splitedFilterStrings = filterString.Split(new string[] { AndSplit }, 2, StringSplitOptions.RemoveEmptyEntries);
                        currentFilterInfo = splitedFilterStrings[0].Trim();
                        filterString = splitedFilterStrings[1];
                    }
                    else if (orIndex != -1)
                    {
                        splitedFilterStrings = filterString.Split(new string[] { OrSplit }, 2, StringSplitOptions.RemoveEmptyEntries);
                        currentFilterInfo = splitedFilterStrings[0].Trim();
                        filterString = splitedFilterStrings[1];
                    }
                    else
                    {
                        currentFilterInfo = filterString;
                        filterString = String.Empty;
                    }

                    //Find the correct Conditional Operator..
                    if (currentFilterInfo.Contains(ContainsSplit))
                    {
                        splitedFilterStrings = currentFilterInfo.Split(new string[] { ContainsSplit }, StringSplitOptions.RemoveEmptyEntries);
                        filterbyField.ConditionalOperator = EnumConditionalOperator.Contains;
                    }
                    else if (currentFilterInfo.Contains(EqSplit))
                    {
                        splitedFilterStrings = currentFilterInfo.Split(new string[] { EqSplit }, StringSplitOptions.RemoveEmptyEntries);
                        filterbyField.ConditionalOperator = EnumConditionalOperator.Equal;
                    }
                    else if (currentFilterInfo.Contains(NeSplit))
                    {
                        splitedFilterStrings = currentFilterInfo.Split(new string[] { NeSplit }, StringSplitOptions.RemoveEmptyEntries);
                        filterbyField.ConditionalOperator = EnumConditionalOperator.NotEqual;
                    }
                    else if (currentFilterInfo.Contains(NotContainsSplit))
                    {
                        splitedFilterStrings = currentFilterInfo.Split(new string[] { NotContainsSplit }, StringSplitOptions.RemoveEmptyEntries);
                        filterbyField.ConditionalOperator = EnumConditionalOperator.NotContains;
                    }
                    else if (currentFilterInfo.Contains(StartsWithSplit))
                    {
                        splitedFilterStrings = currentFilterInfo.Split(new string[] { StartsWithSplit }, StringSplitOptions.RemoveEmptyEntries);
                        filterbyField.ConditionalOperator = EnumConditionalOperator.StartsWith;
                    }
                    else if (currentFilterInfo.Contains(EndsWithSplit))
                    {
                        splitedFilterStrings = currentFilterInfo.Split(new string[] { EndsWithSplit }, StringSplitOptions.RemoveEmptyEntries);
                        filterbyField.ConditionalOperator = EnumConditionalOperator.EndsWith;
                    }
                    else if (currentFilterInfo.Contains(GeSplit))
                    {
                        splitedFilterStrings = currentFilterInfo.Split(new string[] { GeSplit }, StringSplitOptions.RemoveEmptyEntries);
                        filterbyField.ConditionalOperator = EnumConditionalOperator.GreaterThanOrEqual;
                    }
                    else if (currentFilterInfo.Contains(GtSplit))
                    {
                        splitedFilterStrings = currentFilterInfo.Split(new string[] { GtSplit }, StringSplitOptions.RemoveEmptyEntries);
                        filterbyField.ConditionalOperator = EnumConditionalOperator.GreaterThan;
                    }
                    else if (currentFilterInfo.Contains(LeSplit))
                    {
                        splitedFilterStrings = currentFilterInfo.Split(new string[] { LeSplit }, StringSplitOptions.RemoveEmptyEntries);
                        filterbyField.ConditionalOperator = EnumConditionalOperator.LessThanOrEqual;
                    }
                    else if (currentFilterInfo.Contains(LtSplit))
                    {
                        splitedFilterStrings = currentFilterInfo.Split(new string[] { LtSplit }, StringSplitOptions.RemoveEmptyEntries);
                        filterbyField.ConditionalOperator = EnumConditionalOperator.LessThan;
                    }
                    else if (currentFilterInfo.Contains(Custom))
                    {
                        splitedFilterStrings = currentFilterInfo.Split(new string[] { Custom }, StringSplitOptions.RemoveEmptyEntries);
                        filterbyField.ConditionalOperator = EnumConditionalOperator.Custom;
                    }
                    else
                    {
                        splitedFilterStrings = null;
                    }

                    if ((splitedFilterStrings != null) && (splitedFilterStrings.Length == 2)) // must be in two part, name, and, value
                    {
                        filterbyField.Name = splitedFilterStrings[0].Trim();
                        filterbyField.Value = splitedFilterStrings[1].Trim();
                        filterByFieldList.Add(filterbyField);
                    }
                }
            }
            return filterByFieldList;
        }

        /// <summary>
        /// Populate OrderByField list from  orderBy expression
        /// </summary>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public static List<OrderByField> FillOrderByList(String orderBy)
        {
            List<OrderByField> orderByFieldList = null;
            if (false == String.IsNullOrEmpty(orderBy))
            {
                orderByFieldList = new List<OrderByField>();
                var orderByString = orderBy.ToLowerInvariant();
                var sortExpList = orderByString.Split(new String[] { FieldSeparator }, StringSplitOptions.RemoveEmptyEntries);
                OrderByField orderByField = null;
                foreach (var expression in sortExpList)
                {
                    var expressionInfo = expression.Trim();
                    orderByField = new OrderByField();
                    if (expressionInfo.EndsWith(SortOrderDesc))
                    {
                        orderByField.Name = expressionInfo.Replace(SortOrderDesc, String.Empty).Trim();
                        orderByField.Direction =  EnumOrderByDirection.Descending;
                    }
                    else if (expressionInfo.EndsWith(SortOrderAsc))
                    {
                        orderByField.Name = expressionInfo.Replace(SortOrderAsc, String.Empty).Trim();
                        orderByField.Direction = EnumOrderByDirection.Ascending;
                    }
                    else
                    {
                        orderByField.Name = expressionInfo.Trim();
                        orderByField.Direction = EnumOrderByDirection.Ascending;
                    }
                    orderByFieldList.Add(orderByField);
                }
            }
            return orderByFieldList;
        }
    }
}
