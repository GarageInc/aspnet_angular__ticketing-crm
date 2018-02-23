using System;
using System.Collections.Generic;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using System.Linq;

namespace LSports.Framework.DataAccess.Repositories
{
    public class DatabaseRepository : IDatabaseRepository
    {

        private void checkSqlInjection(string s)
        {
            if(s.Contains(" ") || s.Contains(";") || s.Contains(","))
                throw new Exception("Security exception");
        }


        public IList<string> GetTableNames()
        {
            using (var model = new gb_ts_stagingEntities())
            {
                return model.Database.SqlQuery<string>("SHOW TABLES").OrderBy(rec=>rec).ToList();
            }

        }


        public IList<string> GetTableFieldNames(string tableName)
        {
            checkSqlInjection(tableName);

            using (var model = new gb_ts_stagingEntities())
            {
                var query = "DESCRIBE " + tableName;
                var fields = model.Database.SqlQuery<TableFieldMetaData>(query).ToList();

                return fields.Select(rec => rec.Field).OrderBy(rec => rec).ToList();
            }
        }


        public IList<DropdownItem> GetDropdownValuesFromTable(string tableName, string idFieldName, string nameFieldName)
        {
            checkSqlInjection(tableName);
            checkSqlInjection(idFieldName);
            checkSqlInjection(nameFieldName);

            using (var model = new gb_ts_stagingEntities())
            {
                var isActiveCheck = "";
                //If IsActive field exists then show only IsActive records
                if (model.Database.SqlQuery<TableFieldMetaData>("DESCRIBE " + tableName).FirstOrDefault(rec => rec.Field == "IsActive") != null)
                    isActiveCheck = " WHERE IsActive=1 ";

                var query = string.Format("SELECT {0} AS Id, {1} AS Name FROM {2} {3} ORDER BY {1}", idFieldName, nameFieldName, tableName, isActiveCheck);
                var fields = model.Database.SqlQuery<DropdownItem>(query);

                return fields.OrderBy(rec => rec.Name).ToList();
            }
        }


        public IList<DropdownItem> GetDropdownValuesFromTable(string tableName, string idFieldName, string nameFieldName, string filterFieldName,
            string filterFieldValue)
        {
            checkSqlInjection(tableName);
            checkSqlInjection(idFieldName);
            checkSqlInjection(nameFieldName);
            checkSqlInjection(filterFieldName);
            checkSqlInjection(filterFieldValue);
            

            using (var model = new gb_ts_stagingEntities())
            {
                var isActiveCheck = "";
                //If IsActive field exists then show only IsActive records
                if (model.Database.SqlQuery<TableFieldMetaData>("DESCRIBE " + tableName).FirstOrDefault(rec => rec.Field == "IsActive") != null)
                    isActiveCheck = " AND IsActive=1 ";

                var query = string.Format("SELECT {0} AS Id, {1} AS Name FROM {2} WHERE {3}='{4}' {5} ORDER BY {1}", idFieldName, nameFieldName, tableName, filterFieldName, filterFieldValue, isActiveCheck);
                var fields = model.Database.SqlQuery<DropdownItem>(query);

                return fields.OrderBy(rec => rec.Name).ToList();
            }
        }


    }

}
