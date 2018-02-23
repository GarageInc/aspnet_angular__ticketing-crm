ticketsApp.service('formManagementService', function($q, $timeout, networkService) {
    var vm = this;

    vm.loadedForm = [];

    vm.submitForm = function (ticketId, fileIds, scopeFormObject, isNewTicket) {
        if (validateForm(scopeFormObject)) {
            var ticketFields = [];
            vm.loadedForm.forEach(function(val, idx) {
                if (val.CustomFieldType.Id === fieldTypesEnum.Dropdown)
                    ticketFields.push({ TicketId: ticketId, CustomFieldId: val.Id, Value: val.model.Id, TextValue: val.model.Name });
                else if (val.CustomFieldType.Id === fieldTypesEnum.MultipleSelection) {
                    var textValues = val.model.map(function(val1) { return val1.Name });
                    var values = val.model.map(function(val1) { return val1.Id });
                    ticketFields.push({ TicketId: ticketId, CustomFieldId: val.Id, Value: values.join(","), TextValue: textValues.join(",") });
                } else if (val.CustomFieldType.Id === fieldTypesEnum.RelatedDropdowns) {
                    ticketFields.push({ TicketId: ticketId, CustomFieldId: val.Id, Value: val.model.Id, TextValue: val.model.Name });

                    //related stages
                    val.related.forEach(function(relatedVal) {
                        ticketFields.push({
                            TicketId: ticketId,
                            CustomFieldId: relatedVal.Id,
                            Value: relatedVal.data[parseInt(relatedVal.model.Id)]._Id,
                            TextValue: relatedVal.model.Name
                        });
                    });
                } else if (val.CustomFieldType.Id !== fieldTypesEnum.Attachments) {
                    if (val.model) {
                        ticketFields.push({ TicketId: ticketId, CustomFieldId: val.Id, Value: val.model.toString(), TextValue: val.model.toString() });
                    }
                } else {
                    ticketFields.push({ TicketId: ticketId, CustomFieldId: val.Id, Value: fileIds.join(","), TextValue: "attached files" });
                }
            });
            return networkService.sendRequest(networkService.requestBuilder().
                method("POST").
                url("/Ticket/SubmitTicketFields").
                body({ ticketId: ticketId, customFields: ticketFields, isNewTicket: isNewTicket}).build());
        } else {
            return $q(function(resolve) { resolve("invalid"); });
        }
    }
    
    var ms_domNames = [];
    var df_domNames = [];
    vm.loadFormForEdit = function (formForEdit) {
        vm.loadedForm = [];
        df_domNames = [];
        ms_domNames = [];
                formForEdit.forEach(function(val, idx, arr) {
                    var field = val.CustomField;
                    switch (field.CustomFieldType.Id) {
                        
                        case fieldTypesEnum.Date:
                        {
                            field['domName'] = 'd_' + idx;
                            df_domNames.push({ name: field['domName'], idx: field, initialDate: val.TextValue });
                            field['model'] = "";//val.TextValue;
                            break;
                        }
                        case fieldTypesEnum.Numeric:
                            {

                                field['domName'] = generateGUID();
                                field['model'] = parseInt(val.TextValue);
                                break;
                            }
                        case fieldTypesEnum.Text:
                        case fieldTypesEnum.Email:                           
                        case fieldTypesEnum.LongText:
                            field['model'] = val.TextValue;
                            field['domName'] = generateGUID();
                            break;
                        case fieldTypesEnum.Dropdown:
                            field['domName'] = generateGUID();
                            val.OriginalValues.unshift({ Name: field.PlaceholderText, Id: "0" });
                            field['data'] = val.OriginalValues;
                            field['model'] = {Id : val.Value, Name: val.TextValue};    
                            break;
                        case fieldTypesEnum.MultipleSelection:
                            field['model'] = restoreMultipleSelectionModel(val);
                            field['data'] = val.OriginalValues;
                            field['domName'] = 'm' + idx;
                            ms_domNames.push(field['domName']);
                            break;
                       case fieldTypesEnum.RelatedDropdowns:
                            
                            if (field.RootCustomFieldId === null) {
                                field['related'] = [];
                                for (var i = idx + 1; i < formForEdit.length; i++) {
                                    var relatedDd = formForEdit[i];
                                    if (relatedDd.CustomField.RootCustomFieldId === field.Id) {
                                        var relatedField = relatedDd.CustomField;

                                        relatedDd.OriginalValues.unshift({ Name: relatedDd.PlaceholderText, Id: "0" });

                                        //do not care
                                        relatedField['data'] = relatedDd.OriginalValues;

                                        relatedField['model'] = { Id: indexOfModelValueInOriginalValues(relatedField['data'], relatedDd.Value, relatedDd.TextValue).toString(), Name: relatedDd.TextValue };

                                        relatedField['data'] = transformOriginalValues(relatedField['data']);

                                        relatedField['domName'] = generateGUID();

                                        field['related'].push(relatedField);
                                    } else {
                                        break;
                                    }
                                }
                                field['domName'] = generateGUID();
                                field['model'] = { Id: val.Value, Name: val.TextValue };
                                val.OriginalValues.unshift({ Name: field.PlaceholderText, Id: "0" });
                                field['data'] = val.OriginalValues;
                            }
                            
                            break;
                        case fieldTypesEnum.Checkbox:
                            field['model'] = (val.Value === 'true');
                            field['domName'] = generateGUID();
                            break;
                        default:
                            break;
                  
                    }
                    if (field.RootCustomFieldId === null) {
                        vm.loadedForm.push(field);
                    }
                });

                $timeout(function () {
                    ComponentsBootstrapMultiselect.init(function (option, checked, select) {
                        $($(this.$select).parent().children()[1]).removeClass('aha-invalid');
                        //console.log($(this.$select).parent());
                    });
                    /*ms_domNames.forEach(function (val) {
                        $("#" + val).multiselect();
                        
                    });*/
                    //
                    df_domNames.forEach(function (val) {
                        var splitDate = val.initialDate.split("-");
                        var date = new Date(splitDate[2]+"-"+splitDate[1]+"-"+splitDate[0]); // dd/mm/yyyy
                        var v = val;
                        $('#' + v.name).datepicker('setDate', date).on('changeDate', function (e) {                          
                            v.idx.model = e.format();
                        });
                    });


                });

        return vm.loadedForm;
    }

    function indexOfModelValueInOriginalValues(origValues, modelValue, textValue) {
        var unique = isUnique(modelValue, origValues);
        for (var i = 0; i < origValues.length; i++) {
            if (!unique) {
                if (origValues[i].Id === modelValue && origValues[i].Name === textValue)
                    return i;
            } else {
                if (origValues[i].Id === modelValue)
                    return i;
            }
        }
        return 0;
    }

    function isUnique(key, arr) {
        var c = 0;
        for (var i = 0; i < arr.length; i++) {
            if (key === arr[i].Id) {
                ++c;
                if (c >= 2) {
                    console.warn("Id field is not unique in one of the related dropdown stages select.");
                    return false;
                }
            }
        }
        return true;
    }

    function transformOriginalValues(originalValues) {

        return originalValues.map(function(val, idx) {
            val['_Id'] = val.Id;
            val.Id = idx.toString();
            return val;
        });
    }

    function restoreMultipleSelectionModel(val) {
        var valuesArr = val.Value.split(",");
        var textsArr = val.TextValue.split(",");

        var res = [];
        valuesArr.forEach(function(v, idx) {
            res.push({ Id: v, Name: textsArr[idx] });
        });
        return res;
    }

    
    var tables = [];

    var relatedDdRoots= [];

    var tableLoaderCallback = null;
    var relatedDdsLoaderCallback = null; // related dropdowns

    vm.dropdownStageSelectionChanged = function (stageIndex, fieldIndex, formObject) {
        var relatedArr = vm.loadedForm[fieldIndex].related;
        var filterValue = null;
        if (stageIndex === fieldIndex) {
            var firstRelated = vm.loadedForm[stageIndex].related[0];
            filterValue = vm.loadedForm[stageIndex].model.Id;

            loadFilteredValues(firstRelated, filterValue).then(function (filteredValues) {
                
                filteredValues.unshift({ Name: firstRelated.PlaceholderText, Id: "0" });
                firstRelated.data = transformOriginalValues(filteredValues);
                firstRelated.model = firstRelated.data[0];
            });
            formObject[firstRelated.domName].$touched = false;
            relatedArr.forEach(function (val, idx) {
                if (idx !== stageIndex) {
                    val.data = [];
                    val.model = {};
                    formObject[val.domName].$touched = false;
                }
            });

        } else {
            if (typeof relatedArr[stageIndex + 1] !== 'undefined') { // not the last stage
                var relatedToFilter = vm.loadedForm[fieldIndex].related[stageIndex + 1];
                filterValue = vm.loadedForm[fieldIndex].related[stageIndex].model.Id;
                loadFilteredValues(relatedToFilter, filterValue).then(function (filteredValues) {
                    filteredValues.unshift({ Name: relatedToFilter.PlaceholderText, Id: "0" });
                    relatedToFilter.data = transformOriginalValues(filteredValues);
                    relatedToFilter.model = relatedToFilter.data[0];
                });
                formObject[relatedToFilter.domName].$touched = false;
                relatedArr.forEach(function(val, idx) {
                    if (idx !== stageIndex + 1 && idx !== stageIndex) {
                        val.data = [];
                        val.model = {};
                        formObject[val.domName].$touched = false;
                    }
                });
            }
        }
    }

    vm.loadFormFromCreatedTicket = function(form) {
        var tmpLoadedForm = [];
        vm.loadedForm = [];
        df_domNames = [];
        ms_domNames = [];
        return $q(function(resolve, reject) {
            form.CustomFields.forEach(function(val, idx, arr) {
                var field = val.CustomField;
                switch (field.CustomFieldType.Id) {
                    case fieldTypesEnum.Date:
                        field['domName'] = 'd_' + idx;
                        var dt = new Date();
                        field['model'] = dt.getDate()+"-"+(dt.getMonth()+1)+"-"+dt.getFullYear();
                        df_domNames.push({ name: field['domName'], idx: idx });
                        break;
                    case fieldTypesEnum.Text:
                    case fieldTypesEnum.Email:
                    case fieldTypesEnum.Numeric:                 
                    case fieldTypesEnum.LongText:
                        field['model'] = "";
                        field['domName'] = generateGUID();
                        break;
                    case fieldTypesEnum.Dropdown:
                        field['model'] = {};
                        field['data'] = [];
                        field['domName'] = generateGUID();
                        tables.push(buildTableLoaderObject(field));
                        break;
                    case fieldTypesEnum.MultipleSelection:
                        field['model'] = [];
                        field['data'] = [];
                        field['domName'] = 'm' + idx;
                        ms_domNames.push(field['domName']);
                        tables.push(buildTableLoaderObject(field));
                        break;
                    case fieldTypesEnum.RelatedDropdowns:
                        relatedDdRoots.push(field);
                        field['model'] = [];
                        field['data'] = [];
                        field['related'] = [];
                        field['domName'] = generateGUID();
                        tables.push(buildTableLoaderObject(field));
                        break;
                    case fieldTypesEnum.Checkbox:
                        field['model'] = false;
                        field['domName'] = generateGUID();
                        break;
                    default:
                        break;
                  
                }
                tmpLoadedForm.push(field);
            });
            //mac & cheese a.k.a spaghetti
            
            
            
            //end of execution
            relatedDdsLoaderCallback = function () {
                vm.loadedForm = tmpLoadedForm;
                resolve(vm.loadedForm);
                $timeout(function () {
                    ComponentsBootstrapMultiselect.init(function (option, checked, select) {
                        $($(this.$select).parent().children()[1]).removeClass('aha-invalid');
                    });
                    ms_domNames.forEach(function(val) {
                        $("#" + val).multiselect();
                    });
                    

                    df_domNames.forEach(function(val) {
                        $('#' + val.name).datepicker('setDate', new Date()).on('changeDate', function (e) {
                            vm.loadedForm[val.idx].model = e.format();
                        });
                    });
                    
                    
                });
            }
            tableLoaderCallback = function () {
                loadRelatedDropdowns();
            }

            loadTables();
            
        });
    }

    function buildTableLoaderObject(field) {
        return { 
            tableName: field.DbTableName, 
            textField: field.DbTableTextFieldName, 
            idField: field.DbTableIdFieldName,
            field:field};
    }

    function loadRelatedDropdowns() {
        var ddRoot = relatedDdRoots.pop();
        if (!ddRoot)
            relatedDdsLoaderCallback();
        else {
            networkService.sendRequest(
                networkService.
                requestBuilder().
                url("/CustomField/List").
                method("GET").params({ rootId: ddRoot.Id }).build()).then(function (dd) {
                dd = dd[0];
                dd['model'] = {};
                dd['data'] = [];
                dd['domName'] = generateGUID();
                ddRoot.related.push(dd);
                loadRelatedDropdowns();
            });
        }
    }

    function loadTables() {
        var tableToLoad = tables.pop();
        if (typeof tableToLoad == 'undefined' || tableToLoad == null)
            tableLoaderCallback();
        else {
            networkService.sendRequest(networkService.
                requestBuilder().
                method("GET").
                url("/DropDown/TableFieldValues").
                params({
                    dbTableName: tableToLoad.tableName,
                    dbTableIdFieldName: tableToLoad.idField,
                    dbTableTextFieldName: tableToLoad.textField
                }).build()).then(function (data) {
                
                    tableToLoad.field.data = data;
                    if (tableToLoad.field.CustomFieldType.Id !== fieldTypesEnum.MultipleSelection) {
                        data.unshift({ Name: tableToLoad.field.PlaceholderText, Id: "0" });
                    tableToLoad.field.model = data[0];
                }
                loadTables();
            });
        }
    }

    function loadFilteredValues(related, filterValue) {
            return networkService.sendRequest(networkService.requestBuilder().
                url("/DropDown/TableFieldValues").
                method("GET").
                params({
                    dbTableName: related.DbTableName,
                    dbTableIdFieldName: related.DbTableIdFieldName,
                    dbTableTextFieldName: related.DbTableTextFieldName,
                    dbFilterFieldName: related.DbFilterFieldName,
                    dbFilterFieldValue: filterValue

                }).build());
    }

    function generateGUID() {
        return 'Axxxxxxxxxxxx4xxxyxxxxxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            var r = Math.random()*16|0, v = c == 'x' ? r : (r&0x3|0x8);
            return v.toString(16);
        });
    }

    function validateForm(scopeFormObject) {
        var valid = true;
        vm.loadedForm.forEach(function(val) {
            if (val.CustomFieldType.Id === fieldTypesEnum.Numeric || val.CustomFieldType.Id === fieldTypesEnum.Text ||
                val.CustomFieldType.Id === fieldTypesEnum.Email || val.CustomFieldType.Id === fieldTypesEnum.LongText) {
                if (scopeFormObject[val.domName].$invalid) {
                    scopeFormObject[val.domName].$touched = true;
                    valid = false;
                } 
            } else if (val.CustomFieldType.Id === fieldTypesEnum.Dropdown) {
                if (scopeFormObject[val.domName].$viewValue.Id === '0') {
                    scopeFormObject[val.domName].$touched = true;
                    valid = false;
                }
            } else if (val.CustomFieldType.Id === fieldTypesEnum.RelatedDropdowns) {
                if (scopeFormObject[val.domName].$viewValue.Id === '0') {
                    scopeFormObject[val.domName].$touched = true;
                    valid = false;
                }
                val.related.forEach(function(val1) {
                    if (scopeFormObject[val.domName].$viewValue.Id === '0') {
                        scopeFormObject[val1.domName].$touched = true;
                        valid = false;
                    }
                });
            } else if (val.CustomFieldType.Id === fieldTypesEnum.MultipleSelection) {
                if (scopeFormObject[val.domName].$viewValue.length === 0) {
                    valid = false;
                    $($("#" + val.domName).parent().children()[1]).addClass('aha-invalid');
                } else
                    $($("#" + val.domName).parent().children()[1]).removeClass('aha-invalid');
            }
        });

        return valid;

    }

    

    return vm;

});