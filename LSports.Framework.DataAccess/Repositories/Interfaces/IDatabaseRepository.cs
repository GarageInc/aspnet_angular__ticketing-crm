using System;
using System.Collections.Generic;
using LSports.Framework.Models.CustomClasses;


namespace LSports.Framework.DataAccess.Repositories.Interfaces
{
    public interface IDatabaseRepository
    {
        IList<string> GetTableNames();

        IList<string> GetTableFieldNames(string tableName);

        IList<DropdownItem> GetDropdownValuesFromTable(string tableName, string idFieldName, string idNameFieldName);

        IList<DropdownItem> GetDropdownValuesFromTable(string tableName, string idFieldName, string nameFieldName, string filterFieldName, string filterFieldNameValue);

    }
}
