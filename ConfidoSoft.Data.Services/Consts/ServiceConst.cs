using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ConfidoSoft.Data.Services.Consts
{
    /// <summary>
    /// Constant/static used in service classes.
    /// </summary>
    public static class ServiceConst
    {
        #region Methods to be used in generating dynamic filter query's.

        public static MethodInfo ContainsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        public static MethodInfo ToLowerInvariantMethod = typeof(string).GetMethod("ToLowerInvariant");
        public static MethodInfo ToLowerMethod = typeof(string).GetMethod("ToLower", BindingFlags.Public| BindingFlags.Instance, Type.EmptyTypes);
        public static MethodInfo StartsWithMethod = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
        public static MethodInfo EndsWithMethod = typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });
        public static Type StringType = typeof(string);
        public const String OrderByDescendingText = "OrderByDescending";
        public const String OrderByText = "OrderBy";
        public const String ThenByDescendingText = "ThenByDescending";
        public const String ThenByText = "ThenBy";

        public static MethodInfo OrderByMethod = typeof(Queryable).GetMethods().Single(method => method.Name == OrderByText
            && method.IsGenericMethodDefinition && method.GetGenericArguments().Length == 2 && method.GetParameters().Length == 2);

        public static MethodInfo ThenByMethod = typeof(Queryable).GetMethods().Single(method => method.Name == ThenByText
            && method.IsGenericMethodDefinition && method.GetGenericArguments().Length == 2 && method.GetParameters().Length == 2);

        public static MethodInfo OrderByDescendingMethod = typeof(Queryable).GetMethods().Single(method => method.Name == OrderByDescendingText
            && method.IsGenericMethodDefinition && method.GetGenericArguments().Length == 2 && method.GetParameters().Length == 2);

        public static MethodInfo ThenByDescendingMethod = typeof(Queryable).GetMethods().Single(method => method.Name == ThenByDescendingText
            && method.IsGenericMethodDefinition && method.GetGenericArguments().Length == 2 && method.GetParameters().Length == 2);

        public const String Id = "Id";

        public const String ModifiedOn = "modifiedon";
        public const String ModifiedById = "modifiedby";

        public const char PropertySeparator = '.';

        #endregion
    }
}
