using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfidoSoft.Data.Domain.Helpers
{
    public class PgTriggerHelper
    {
        private const string CommonFunctionSql = @"
        CREATE OR REPLACE FUNCTION increment_version()
        RETURNS TRIGGER AS $$
        BEGIN
            NEW.""RowVersion"" := NEW.""RowVersion"" + 1;
            RETURN NEW;
        END;
        $$ LANGUAGE plpgsql;
    ";

        public static string CreateCommonIncrementVersionFunction()
        {
            return CommonFunctionSql;
        }

        public static string CreateIncrementVersionTrigger(string tableName)
        {
            return $@"
            CREATE TRIGGER before_update_{tableName}_rowversion
            BEFORE UPDATE ON ""{tableName}""
            FOR EACH ROW
            EXECUTE FUNCTION increment_version();
        ";
        }

        public static string DropIncrementVersionTrigger(string tableName)
        {
            return $@"
            DROP TRIGGER IF EXISTS before_update_{tableName}_rowversion ON ""{tableName}"";
        ";
        }
    }
}
