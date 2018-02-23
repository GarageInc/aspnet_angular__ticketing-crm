formTemplatesApp.service("formTemplatesService", function(DropdownDatasource, networkService, crudService, $q) {
    var vm = this;

    vm.issueTypes = [];
    vm.selectedIssueType = {};

    vm.ticketTypes = [];
    vm.selectedTicketType = {};

    vm.productNames = [];
    vm.selectedProductName = {};

    vm.productCategories = [];
    vm.selectedProductCategory = {};

    vm.customFields = [];

    vm.formFields = [];

    vm.formName = "";

    vm.renderedFormFields = [];

    vm.editingFormId = 0;
    vm.editingFormSortOrder = -1;

    vm.dropdownsForForm = {};


    var dds = DropdownDatasource;
    var rb = dds.requestBuilder;

    vm.loadIssueTypes = function() {
        return $q(function(resolve, reject) {
            dds.sendRequest(rb().setPath("/IssueTypes").setNullValue("ANY").build()).then(function(issueTypes) {
                vm.issueTypes = issueTypes;
                vm.selectedIssueType = issueTypes[0];
                resolve();

            });
        });
    }

    vm.loadTicketTypes = function() {
        return $q(function(resolve, reject) {
            dds.sendRequest(rb().setPath("/TicketTypes").setNullValue("ANY").build()).then(function(ticketTypes) {
                vm.ticketTypes = ticketTypes;
                vm.selectedTicketType = ticketTypes[0];
                resolve();
            });
        });
    }

    vm.loadProductNames = function() {
        return $q(function(resolve, reject) {
            dds.sendRequest(rb().setPath("/Products").setNullValue("ANY").build()).then(function(productTypes) {
                vm.productNames = productTypes;
                vm.selectedProductName = productTypes[0];
                resolve();
            });
        });
    }

    vm.loadProductCategories = function() {
        return $q(function(resolve, reject) {
            dds.sendRequest(rb().setPath("/ProductCategories").setNullValue("ANY").build()).then(function(productCategories) {
                vm.productCategories = productCategories;
                vm.selectedProductCategory = productCategories[0];
                resolve();
            });
        });
    }

    vm.loadCustomFields = function() {
        return $q(function(resolve, reject) {
            networkService.sendRequest(networkService.requestBuilder().method("GET").url("/DropDown/CustomFields").build()).then(function(data) {
                data.unshift({ Name: "-Select field-", Id: "0" });
                vm.customFields = data;
            });
        });
    }

    vm.loadFieldsForTable = function() {
        return $q(function(resolve, reject) {
            dds.sendRequest(rb().setPath("/TableFieldNames").
                setNullValue("-Select field-").
                setParams({ dbTableName: vm.selectedTable.Name }).
                setCacheKey(vm.selectedTable.Name).build()).then(function(data) {
                vm.fieldsForTable = data;
                vm.selectedIdField = data[0];
                vm.selectedTitleField = data[0];
            });
        });
    }

    vm.loadFormFieldsList = function(formId) {
        return networkService.sendRequest(networkService.requestBuilder().method("POST").url("/FormTemplate/GetCustomFields").params({ formTemplateId: formId }).build());
    }

    //main table
    vm.loadFormTemplates = function() {
        return $q(function(resolve, reject) {
            crudService.read("FormTemplate").then(function(data) {
                var mappedArr = data.map(function(val, idx, arr) {
                    val["edit"] = "edit";
                    val["del"] = "delete";

                    return val;
                });
                mappedArr.sort(function(ft1, ft2) {
                    if (ft1.SortOrder > ft2.SortOrder)
                        return 1;
                    if (ft1.SortOrder < ft2.SortOrder)
                        return -1;
                    return 0;
                });
                resolve(mappedArr);
            });
        });
    }
   var fieldTypesEnum = {
        Dropdown : 3,
        Text: 1, // check
        LongText: 2, // check
        RelatedDropdowns: 4,
        Checkbox: 5, // check
        Date: 6,
        Attachements: 7,
        Numeric: 8,
        Email: 9, // check
        MultipleSelection:10
   };
   var inputData = [];
   var callback = null;

   var rootsForRelatedDropdowns = [];
   var relatedDropdownsLoaderCallback = null;

   vm.loadRelatedOptions = function (related) {
       console.log(related);
        return networkService.sendRequest(networkService.requestBuilder().
            url("/DropDown/TableFieldValues").
            method("GET").
            params({
                dbTableName: related.DbTableName,
                dbTableIdFieldName: related.DbIdFieldName,
                dbTableTextFieldName: related.DbTextFieldName,
                dbFilterFieldName: related.DbFilterFieldName,
                dbFilterFieldValue: related.filterValue
                
            }).build());
    }
    vm.loadDropdownsForForm = function () {
       vm.dropdownsForForm = {};
        return $q(function (resolve, reject) {
            vm.formFields.forEach(function (val, idx, arr) {
                console.log(val);
                if (val.selectedCustomField.CustomFieldType.Id === fieldTypesEnum.Dropdown ||
                    val.selectedCustomField.CustomFieldType.Id === fieldTypesEnum.MultipleSelection ||
                    val.selectedCustomField.CustomFieldType.Id === fieldTypesEnum.RelatedDropdowns) {
                    inputData.push({
                        fieldId: val.selectedCustomField.Id,
                        fieldName: val.selectedCustomField.Name,
                        fieldPlaceholder: val.selectedCustomField.PlaceholderText,
                        fieldTypeId: val.selectedCustomField.CustomFieldType.Id,
                        tableName: val.selectedCustomField.DbTableName,
                        idFieldName: val.selectedCustomField.DbTableIdFieldName,
                        textFieldName: val.selectedCustomField.DbTableTextFieldName
                    });
                }
            });
            callback = function() {
                Object.getOwnPropertyNames(vm.dropdownsForForm).forEach(function(val, idx) {
                    var dropdown = vm.dropdownsForForm[val];
                    if (dropdown.fieldType === fieldTypesEnum.RelatedDropdowns) {
                        rootsForRelatedDropdowns.push({ root: dropdown.fieldId, dropdownName: dropdown.fieldName });
                    }
                });
                relatedDropdownsLoaderCallback = function () {
                    
                    resolve();
                }

                relatedDropdownsLoader();
            }
            dropdownDataLoader();
            
        });
   };

    function relatedDropdownsLoader() {
        if (rootsForRelatedDropdowns.length === 0) {
            relatedDropdownsLoaderCallback();
        } else {
            var t = rootsForRelatedDropdowns.pop();
            networkService.sendRequest(networkService.
                requestBuilder().
                url("/CustomField/List").
                method("GET").params({ rootId: t.root }).build()).then(function(data) {
                    //vm.dropdownsForForm[t.dropdownName]
                    var relatedData = [];
                data.forEach(function(val) {
                    relatedData.push({
                        DbTableName: val.DbTableName,
                        DbTextFieldName: val.DbTableTextFieldName,
                        DbIdFieldName: val.DbTableIdFieldName,
                        DbFilterFieldName: val.DbFilterFieldName,
                        PlaceholderText: val.PlaceholderText,
                        options: [],
                        selectedOption: { Name: val.PlaceholderText, Id: "" }
                    });
                });
                console.log(vm.dropdownsForForm);
                vm.dropdownsForForm[t.dropdownName]['relatedDropdowns'] = relatedData;
                relatedDropdownsLoader();
            });
        }
    }

    var curDataIdx = 0;   
    function dropdownDataLoader() {
        if (inputData.length === 0)
            callback();
        else {
            var input = inputData.pop();
            networkService.sendRequest(networkService.
                requestBuilder().
                method("GET").
                url("/DropDown/TableFieldValues").
                params({
                    dbTableName: input.tableName,
                    dbTableIdFieldName: input.idFieldName,
                    dbTableTextFieldName: input.textFieldName
                }).build()).then(function(data) {
                    console.log(data);
                var dummyModel = null;
                if (input.fieldTypeId === fieldTypesEnum.MultipleSelection) {
                    dummyModel = [];
                } else {
                    data.unshift({ "Name": input.fieldPlaceholder, "Id": 0 });
                    dummyModel = data[0];
                }
                vm.dropdownsForForm[input.fieldName] = {
                    fieldType: input.fieldTypeId,
                    fieldId: input.fieldId,
                    fieldName: input.fieldName,
                    data: data, 
                    dummyModel : dummyModel
            };
                dropdownDataLoader();
            });
        }
    }

    vm.createNewForm = function(sortOrder) {
        var body = {
            Name: vm.formName,
            TicketType: vm.selectedTicketType,
            Product: vm.selectedProductName,
            IssueType: vm.selectedIssueType,
            ProductCategory: vm.selectedProductCategory,
            SortOrder: sortOrder
        };
        
        return $q(function(resolve, reject) {
            networkService.sendRequest(networkService.requestBuilder().method("POST").url("/FormTemplate/Insert").body(body).build()).
                then(function (addedObj) {
                    var customFields = vm.formFields.map(function (val, idx, arr) {
                        return {
                            SortOrder: idx,
                            CustomField: val.selectedCustomField
                        }
                    });
                    networkService.sendRequest(networkService.requestBuilder().method("POST").url("/FormTemplate/UpdateCustomFields").
                        params({formTemplateId : addedObj.Id}).
                        body(customFields).
                        build(customFields)).then(function(data) {
                            addedObj['CustomFields'] = customFields;
                            addedObj["edit"] = "edit";
                            addedObj["del"] = "delete";
                        resolve(addedObj);
                    });
                });
        });
        
    }

    vm.updateForm = function() {
        var body = {
            Name: vm.formName,
            TicketType: vm.selectedTicketType,
            Product: vm.selectedProductName,
            IssueType: vm.selectedIssueType,
            ProductCategory: vm.selectedProductCategory,
            SortOrder: vm.editingFormSortOrder,
            Id: vm.editingFormId
        };

        return $q(function (resolve, reject) {
            networkService.sendRequest(networkService.requestBuilder().method("POST").url("/FormTemplate/Update").body(body).build()).
                then(function (addedObj) {
                    var customFields = vm.formFields.map(function (val, idx, arr) {
                        return {
                            SortOrder: idx,
                            CustomField: val.selectedCustomField
                        }
                    });
                    networkService.sendRequest(networkService.requestBuilder().method("POST").url("/FormTemplate/UpdateCustomFields").
                        params({ formTemplateId: addedObj.Id }).
                        body(customFields).
                        build(customFields)).then(function (data) {
                            addedObj['CustomFields'] = customFields;
                            resolve(addedObj);
                        });
                });
        });
    }


    vm.validateForm = function () {
        var body = {
            Name: vm.formName,
            TicketType: vm.selectedTicketType,
            Product: vm.selectedProductName,
            IssueType: vm.selectedIssueType,
            ProductCategory: vm.selectedProductCategory,
            SortOrder: vm.editingFormSortOrder,
            Id: vm.editingFormId == null ? 0 : vm.editingFormId
        };

        return $q(function (resolve, reject) {
            networkService.sendRequest(networkService.requestBuilder().method("POST").url("/FormTemplate/Validate").body(body).build()).
                then(function (errors) {
                    resolve(errors);
                });
        });
    }

    //for form templates in general
    vm.updateSortOrder = function(newOrder) {
        return networkService.sendRequest(networkService.requestBuilder().method("POST").url("/FormTemplate/UpdateSortOrder").body(newOrder).build());
    }

    vm.resetModalSelections = function() {
        vm.selectedIssueType = vm.issueTypes[0];
        vm.selectedTicketType = vm.ticketTypes[0];
        vm.selectedProductName = vm.productNames[0];
        vm.selectedProductCategory = vm.productCategories[0];
        vm.formName = "";
        vm.formFields.splice(0, vm.formFields.length);

    }

    /*######################################FORM FIELDS DATA#####################################*/
    vm.addFormField = function(formField) {
        vm.formFields.push(formField);
    }
    vm.removeFormField = function(idx) {
        vm.formFields.splice(idx, 1);
    }
    vm.buildFormField = function() {
        return {
            selectedCustomField:vm.customFields[0] //TODO: hidden dependence
        };
    }
    return vm;

});