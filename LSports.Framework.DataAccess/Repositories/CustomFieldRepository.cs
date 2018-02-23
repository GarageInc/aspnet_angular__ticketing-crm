using System;
using System.Collections.Generic;
using System.Linq;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Framework.Models;

namespace LSports.Framework.DataAccess.Repositories
{
    public class CustomFieldRepository : ICustomFieldRepository
    {

        public IList<CustomFieldType> GetCustomFieldTypes()
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q =
                    model.tic_CustomFieldTypes.Select(rec => new CustomFieldType {Id = rec.Id, Name = rec.Name})
                        .OrderBy(rec => rec.Name);

                return q.ToList();
            }
        }



        public string IsCustomFieldValuesUnique(string name, string identifier, int id)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from rec in model.tic_CustomFields
                    where rec.IsActive
                    where rec.Identifier == identifier || rec.Name == name
                    where rec.Id != id
                    select rec).ToList();

                if (q.Count == 0) return "0";

                var nameIsNotUnique = q.FirstOrDefault(rec => rec.Name == name) != null;

                var idIsNotUnique = q.FirstOrDefault(rec => rec.Identifier == identifier) != null;

                if (nameIsNotUnique && idIsNotUnique) return "Name and Identifier should be unique.";

                if (nameIsNotUnique) return "Name should be unique";

                if (idIsNotUnique) return "Identifier should be unique";

                return "0";

            }
        }
        

        public IList<CustomField> GetList(int? rootId)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from customField in model.tic_CustomFields
                         join customFieldType in model.tic_CustomFieldTypes on customField.CustomeFieldTypeId equals customFieldType.Id
                         join dropdownCustomField in model.tic_CustomFields on new {Id = customField.DropdownCustomFieldId }  equals new {Id = (int?) dropdownCustomField.Id } into dropdownCustomFieldInner
                         from dropdownCustomFieldNullable in dropdownCustomFieldInner.DefaultIfEmpty()
                         where customField.RootCustomFieldId == rootId
                         where customField.IsActive
                         select new CustomField
                         {
                             Id = customField.Id,
                             Name = customField.Name,
                             DbFilterFieldName = customField.DbFilterFieldName,

                             DbTableIdFieldName = dropdownCustomFieldNullable == null ? customField.DbTableIdFieldName : dropdownCustomFieldNullable.DbTableIdFieldName,
                             DbTableName = dropdownCustomFieldNullable == null ? customField.DbTableName : dropdownCustomFieldNullable.DbTableName,
                             DbTableTextFieldName = dropdownCustomFieldNullable == null ? customField.DbTableTextFieldName : dropdownCustomFieldNullable.DbTableTextFieldName,
                             PlaceholderText = dropdownCustomFieldNullable == null ? customField.PlaceholderText : dropdownCustomFieldNullable.PlaceholderText,
                             Title = dropdownCustomFieldNullable == null ? customField.Title : dropdownCustomFieldNullable.Title,
                             Identifier = customField.Identifier,

                             StepNumber = customField.StepNumber,
                             RootCustomFieldId = customField.RootCustomFieldId,
                             DropdownCustomFieldId = customField.DropdownCustomFieldId,
                             CustomFieldType = new CustomFieldType
                             {
                                 Id = customFieldType.Id,
                                 Name = customFieldType.Name,
                             },
                         }).ToList();

                return q;
            }
        }


        public CustomField Insert(CustomField customField)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var newRecord = new tic_CustomFields
                {
                    Name = customField.Name,
                    DbFilterFieldName = customField.DbFilterFieldName,
                    DbTableIdFieldName = customField.DbTableIdFieldName,
                    DbTableName = customField.DbTableName,
                    DbTableTextFieldName = customField.DbTableTextFieldName,
                    PlaceholderText = customField.PlaceholderText,
                    StepNumber = customField.StepNumber,
                    Title = customField.Title,
                    Identifier = customField.Identifier,
                    CustomeFieldTypeId = customField.CustomFieldTypeId,
                    CreatedBy = "Admin",
                    CreationDate = DateTime.Now,
                    IsActive = true,
                    RootCustomFieldId = customField.RootCustomFieldId,
                    DropdownCustomFieldId = customField.DropdownCustomFieldId
                };

                model.tic_CustomFields.Add(newRecord);
                model.SaveChanges();
                customField.Id = newRecord.Id;

                return customField;
            }
        }

        public CustomField Update(CustomField customField)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var recordToEdit = model.tic_CustomFields.First(rec => rec.Id == customField.Id);
                recordToEdit.Name = customField.Name;
                recordToEdit.DbFilterFieldName = customField.DbFilterFieldName;
                recordToEdit.DbTableIdFieldName = customField.DbTableIdFieldName;
                recordToEdit.DbTableName = customField.DbTableName;
                recordToEdit.DbTableTextFieldName = customField.DbTableTextFieldName;
                recordToEdit.PlaceholderText = customField.PlaceholderText;
                recordToEdit.StepNumber = customField.StepNumber;
                recordToEdit.Title = customField.Title;
                recordToEdit.Identifier = customField.Identifier;
                recordToEdit.CustomeFieldTypeId = customField.CustomFieldType.Id;
                recordToEdit.UpdatedBy = "Admin";
                recordToEdit.LastUpdate = DateTime.Now;
                recordToEdit.DropdownCustomFieldId = customField.DropdownCustomFieldId;

                model.SaveChanges();

                return customField;
            }
        }

        public void Delete(int id)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var recordToDelete = model.tic_CustomFields.First(rec => rec.Id == id);
                recordToDelete.UpdatedBy = "Admin";
                recordToDelete.LastUpdate = DateTime.Now;
                recordToDelete.IsActive = false;

                model.SaveChanges();
            }
        }

        public bool IsCustomFieldNameUnique(string name, int id)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var record = model.tic_CustomFields.First(rec => rec.Name == name && rec.IsActive && rec.Id != id);

                return record == null;
            }
        }

        public bool IsCustomFieldIdentifierUnique(string identifier, int id)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var record = model.tic_CustomFields.First(rec => rec.Identifier == identifier && rec.IsActive && rec.Id != id);

                return record == null;
            }
        }

        public void Update(CustomFieldCollection data)
        {
            var updateCollection = data.ItemsToUpdate;
            var deleteCollection = data.ItemsToDelete;
            var insertCollection = data.ItemsToAdd;

            if (updateCollection != null)
                Update(updateCollection);
            if (deleteCollection != null)
                Delete(deleteCollection);
            if (insertCollection != null)
                Insert(insertCollection);
        }


        #region Private Helpers

        private void Update(IList<CustomField> fields)
        {
            foreach (var customField in fields)
            {
                Update(customField);
            }
        }

        private void Insert(IList<CustomField> fields)
        {
            foreach (var customField in fields)
            {
                Insert(customField);
            }
        }

        private void Delete(IList<CustomField> fields)
        {
            foreach (var customField in fields)
            {
                Delete(customField.Id);
            }
        }

        public IList<CustomFieldDropdown> GetCustomFieldDropdowns()
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = from customField in model.tic_CustomFields
                    where customField.IsActive
                    where customField.CustomeFieldTypeId == 3
                    //Dropdown
                    orderby customField.Name
                    select new CustomFieldDropdown
                    {
                        Id = customField.Id,
                        Name = customField.Name,
                        TableName = customField.DbTableName
                    };

                return q.ToList();
            }
        }

        public IList<CustomFieldIdentifier> GetCustomFieldIdentifiers()
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = from customField in model.tic_CustomFields
                    join customFieldType in model.tic_CustomFieldTypes on customField.CustomeFieldTypeId equals
                        customFieldType.Id
                    where customField.IsActive
                    where customField.CustomeFieldTypeId != 4
                    //Related dropdowns
                    orderby customField.Name
                    select new CustomFieldIdentifier
                    {
                        Id = customField.Identifier,
                        Name = customField.Name,
                        TypeId = customFieldType.Id,
                        TypeName = customFieldType.Name
                        
                    };

                return q.ToList();
            }
        }

        #endregion
    }
}