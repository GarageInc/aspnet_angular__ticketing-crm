customFieldsApp.service("customFieldsService", function (DropdownDatasource, networkService, crudService, $q) {
    var vm = this;

    vm.DROPDOWN_TYPE_ID = 3;
    vm.RELATED_DROPDOWN_TYPE_ID = 4;
    vm.MULTIPLE_SELECT_TYPE_ID = 10;

    var relatedFieldsOriginal = new HashMap(); // as in loaded from server

    vm.fieldTypes = [];
    vm.selectedFieldType = {};

    vm.tableNames = [];
    vm.selectedTable = {};

    vm.fieldsForTable = [];
    vm.selectedIdField = {};
    vm.selectedTitleField = {};

    vm.selectedCustomField = {}; // selected field from the MAIN/CENTRAL/NUMERO UNO/DOMINIUS SUPERIORE table

    vm.buildStageForDropDown = buildStageForDropDown;
    vm.stagesForDropdown = [];

    vm.dropdowns = []; // the are all of the dropdown custom fields, that exists in the system
    vm.dropdownsById = new HashMap(); // retrieve dropdown names by id

    var dds = DropdownDatasource;
    var rb = dds.requestBuilder;

    vm.loadFieldTypes= function () {
        return $q(function (resolve, reject) {
            
            dds.sendRequest(rb().setPath("/CustomFieldTypes").setNullValue("-Select field type-").build()).then(function (fieldTypes) {
                vm.fieldTypes = fieldTypes;
                vm.selectedFieldType = vm.fieldTypes[0];
                resolve();
            });
        });
    }

    vm.loadTableNames = function() {
        return $q(function (resolve, reject) {

            dds.sendRequest(rb().setPath("/TableNames").setNullValue('-Select table-').setCacheKey("tables").build()).then(function (data) {
                vm.tableNames = data;
                vm.selectedTable = vm.tableNames[0];
                resolve();
            });
        });
    }

    vm.loadFieldsForTable = function() {
        return $q(function(resolve, reject) {
            dds.sendRequest(rb().setPath("/TableFieldNames").
                setNullValue("-Select field-").
                setParams({ dbTableName: vm.selectedTable.Name }).
                setCacheKey(vm.selectedTable.Name).build()).then(function (data) {
                    vm.fieldsForTable = data;
                    vm.selectedIdField = data[0];
                    vm.selectedTitleField = data[0];
                resolve();
            });
        });
    }
    vm.loadFieldsForTableName = function (tableName, shouldAugment) {
        return $q(function (resolve, reject) {
            dds.sendRequest(rb().setPath("/TableFieldNames").

                setNullValue("-Select field-").
                setParams({ dbTableName: tableName }).
                setCacheKey(tableName).build())
                .then(function (data) {
                  
                    resolve(data);
                });
        });
    }

    vm.loadDropdowns = function() {
        return $q(function(resolve, reject) {
            dds.sendRequest(rb().setPath("/CustomFieldDropdowns")
                .setNullValue("-Select stage dropdown-")
                .build()).then(function (data) {
                    console.log(data);
                    vm.dropdowns = data;
                vm.dropdowns.forEach(function(val, idx, arr) {
                    vm.dropdownsById.putValue(val.Id.toString(), val);
                });
                resolve();
            });
        });
    }

    vm.addStageForDropDown = function (stage) {
        vm.stagesForDropdown.push(stage);
    }

    vm.removeStage = function (index) {
        vm.stagesForDropdown.splice(index, 1);
    }

    function clearStagesForDropdown() {
        vm.stagesForDropdown.splice(0, vm.stagesForDropdown.length);
        relatedFieldsOriginal.clearMap();
    }

    function buildStageForDropDown() {
        return {
            stageId: 0,
            selectedDropdown:vm.dropdowns[0],
            selectedFilterField: vm.fieldsForTable[0],
            fieldsForTable: []
    };
    }
    vm.loadCustomFieldIntoViewHelper = function () {
        vm.selectedFieldType = vm.fieldTypes.find(function (val) {
            return vm.selectedCustomField.CustomFieldType.Id === val.Id;
        });
        if (vm.selectedCustomField.CustomFieldType.Id === vm.DROPDOWN_TYPE_ID ||
            vm.selectedCustomField.CustomFieldType.Id === vm.MULTIPLE_SELECT_TYPE_ID) {
            return dropDownAndMultipleSelectLoaderHelper();
        }

        if (vm.selectedCustomField.CustomFieldType.Id === vm.RELATED_DROPDOWN_TYPE_ID) {
            clearStagesForDropdown();
            return $q(function(resolve, reject) {
                stage0LoadHelper().then(function() {
                    loadRelatedFieldsForDropdown(vm.selectedCustomField.Id, function () {
                        resolve();
                    });
                });
            });
        }

        return $q(function (resolve) { resolve(); });
    }

    /*#############################################CODE RESPONSIBLE FOR LOADING STAGES FROM SERVER#################################*/
    /**
     * WARNING!
     * SEVERE SPAGHETTIFICATION!
     */
    var tableFields = {};
    var tableNames = [];
    var userCallback = null;
    var internalCallback = null;
    var stageData = null;
    var curTableName = 0;
    function loadRelatedFieldsForDropdown(rootId, callback) {

        tableNames = [];
        relatedFieldsOriginal.clearMap();
        tableFields = {}; // fields array for table by name
        userCallback = callback;
        stageData = null;
        curTableName = 0;

        networkService.sendRequest(networkService.
                requestBuilder().
                url("/CustomField/List").
                method("GET").params({ rootId: rootId }).build())
                .then(function (data) {
                console.log(data);
                stageData = data;
                
                    data.forEach(function(d) { // foreach data
                        relatedFieldsOriginal.putValue(d.Id.toString(), d); // map by id
                        
                        relatedFieldsOriginal.putValue(buildKey({ // map by param combination
                            dropdownName:d.Name,
                            filterField: d.DbFilterFieldName
                        }), d);

                        var stage = buildStageForDropDown(); // init stage
                        stage.stageId = d.Id; // save stage id
                        stage.selectedDropdown = vm.dropdownsById.valueForKey(d.DropdownCustomFieldId.toString());
            
                        tableNames.push(d.DbTableName);
                        vm.addStageForDropDown(stage);
                    });
                var mapFieldByName = new HashMap();
                    internalCallback = function() {
                        stageData.forEach(function(val, idx) {
                            vm.stagesForDropdown[idx + 1].fieldsForTable = tableFields[val.DbTableName];
                            var stageFields = vm.stagesForDropdown[idx + 1].fieldsForTable;
                            //TODO: should probably make use of hashmap - field idx, perfield name, per table
                                var idxFilter = 0;
                                for (var k = 0; k < stageFields.length; k++) {
                                    if (stageFields[k].Name === val.DbFilterFieldName) {
                                        idxFilter = k;
                                        break;
                                    }
                                }
                                console.log(idxFilter);
                                mapFieldByName.putValue(val.DbFilterFieldName, idxFilter);

                            //+1 cuz 0 is stage0
                            vm.stagesForDropdown[idx + 1].selectedFilterField = stageFields[idxFilter];//stageFields[mapFieldByName.valueForKey(val.DbFilterFieldName)];
                            // }
                            console.log(stageFields);
                        });
                        
                        userCallback();
                    }
                    loadFieldsForTableNames();
            });
    }
    function loadFieldsForTableNames() {
        if (curTableName === tableNames.length) {
            internalCallback();
            return;
        }
        if (!tableFields[tableNames[curTableName]]) { // didn't load
            vm.loadFieldsForTableName(tableNames[curTableName]).then(function(fields) {
                tableFields[tableNames[curTableName]] = fields;
                curTableName++;
                loadFieldsForTableNames();
            });
        } else { // already loaded
            curTableName++;
            loadFieldsForTableNames();
        }

    }
 /*###############################################END OF CODE FOR LOADING STAGES#####################################################*/
    function buildKey(keyObj) {
        return keyObj.dropdownName +  keyObj.filterField;
    }

    //main table
    vm.loadCustomFields = function () {
        return $q(function (resolve, reject) {
            crudService.read("CustomField").then(function (data) {

                resolve(data.map(function(val, idx, arr) {
                    val["edit"] = "edit";
                    val["del"] = "delete";
                    return val;
                }));
            });
        });
    }

    /**
     * public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Identifier { get; set; }
        public string PlaceholderText { get; set; }
        public int CustomFieldTypeId { get; set; }
        public string DbTableName { get; set; }
        public string DbTableIdFieldName { get; set; }
        public string DbTableTextFieldName { get; set; }
        public string DbFilterFieldName { get; set; }
        public int? StepNumber { get; set; }
        public int? RootCustomFieldId { get; set; }

        public CustomFieldType CustomFieldType { get; set; } 
     */
    vm.createNewCustomField = function () {
        var newCustomField = {
            Name: vm.selectedCustomField.Name,
            Title: vm.selectedCustomField.Title,
            Identifier: vm.selectedCustomField.Identifier,
            PlaceholderText: vm.selectedCustomField.PlaceholderText,
            CustomFieldTypeId: vm.selectedFieldType.Id,
            CustomFieldType: vm.selectedFieldType
        };

        if (vm.selectedFieldType.Id === vm.DROPDOWN_TYPE_ID || vm.selectedFieldType.Id === vm.MULTIPLE_SELECT_TYPE_ID) {
            newCustomField["DbTableName"] = vm.selectedTable.Name;
            newCustomField["DbTableIdFieldName"] = vm.selectedIdField.Name;
            newCustomField["DbTableTextFieldName"] = vm.selectedTitleField.Name;
        }
        if (vm.selectedFieldType.Id === vm.RELATED_DROPDOWN_TYPE_ID) {
            var stage0 = vm.stagesForDropdown[0];
            newCustomField["DropdownCustomFieldId"] = stage0.selectedDropdown.Id;
        }
        return $q(function (resolve, reject) {
            crudService.create("CustomField", newCustomField).then(function (addedObj) {
                vm.selectedCustomField.Id = addedObj.Id;
                addedObj["edit"] = "edit";
                addedObj["del"] = "delete";

                if (vm.selectedFieldType.Id === vm.RELATED_DROPDOWN_TYPE_ID) { // related dropdown
                    vm.stagesForDropdown.shift();
                    var body = buildStagesAddDeleteUpdateArrays(addedObj.Id);
                    networkService.
                        sendRequest(networkService.requestBuilder().method("POST").
                            url("/CustomField/UpdateCustomFieldCollection").body(
                                {
                                    ItemsToAdd: body.add,
                                    ItemsToDelete: body.del,
                                    ItemsToUpdate: body.update
                                }
                            ).build()).then(function (data) {

                                resolve(addedObj);
                        });

                } else {  // not related dropdown    
                    resolve(addedObj);
                }
            });
        });
    }

    function buildStagesAddDeleteUpdateArrays(rootId) {
        var add = [];
        var update = [];
        var del = [];
        vm.stagesForDropdown.forEach(function(val,idx) {
            if (val.stageId === 0 && !relatedFieldsOriginal.hasKey(buildKey({
                dropdownName: val.selectedDropdown.Name,
                filterField: val.selectedFilterField.Name
            }))) {
                add.push(convertUserStageToServer(val,idx, rootId));
            }
            if (val.stageId !== 0 && !relatedFieldsOriginal.hasKey(buildKey({
                dropdownName: val.selectedDropdown.Name,
                filterField: val.selectedFilterField.Name
            }))) {
                var updateStage = convertUserStageToServer(val, idx, rootId);
                updateStage.Id = val.stageId;
                update.push(updateStage);
            }

        });
        var userStagesHashmap = convertStagesArrayToHashMap();
        var originalStagesHashmapKeys = relatedFieldsOriginal.allKeys();
        //TODO: will do double delete, fixit filter(isNaN(parseInt(val)))

       originalStagesHashmapKeys.forEach(function(val) {
           var originalStage = relatedFieldsOriginal.valueForKey(val);

            if (!userStagesHashmap.hasKey(originalStage.Id.toString()) &&
                !userStagesHashmap.hasKey(buildKey({
                dropdownName: vm.dropdownsById.valueForKey(originalStage.DropdownCustomFieldId.toString()).Name,
                filterField: originalStage.DbFilterFieldName
            }))) {
                del.push(originalStage);
            }

        });


        return { add: add, update: update, del: del };
    }

    function convertStagesArrayToHashMap() {
        var hashmap = new HashMap();
        vm.stagesForDropdown.forEach(function(val) {
            hashmap.putValue(buildKey({
                dropdownName: val.selectedDropdown.Name,
                filterField: val.selectedFilterField.Name
            }), val);
            hashmap.putValue(val.stageId.toString(), val);
        });
        return hashmap;
    }

    function convertUserStageToServer(stage, idx, rootId) {

        return {
                Name: "dummy",
                Title: "dummy",
                CustomFieldTypeId: vm.selectedFieldType.Id,
                CustomFieldType: vm.selectedFieldType,
                DropdownCustomFieldId: stage.selectedDropdown.Id,
                DbFilterFieldName: stage.selectedFilterField.Name,
                StepNumber: idx+1,
                RootCustomFieldId: rootId
            };
    }

    vm.updateCustomField = function () {

        return $q(function (resolve, reject) {
            var updateCustomField = {
                Id: vm.selectedCustomField.Id,
                Name: vm.selectedCustomField.Name,
                Title: vm.selectedCustomField.Title,
                Identifier: vm.selectedCustomField.Identifier,
                PlaceholderText: vm.selectedCustomField.PlaceholderText,
                CustomFieldTypeId: vm.selectedFieldType.Id,
                CustomFieldType: vm.selectedFieldType
            };

            if (vm.selectedFieldType.Id === vm.DROPDOWN_TYPE_ID || vm.selectedFieldType.Id === vm.MULTIPLE_SELECT_TYPE_ID) {
                updateCustomField["DbTableName"] = vm.selectedTable.Name;
                updateCustomField["DbTableIdFieldName"] = vm.selectedIdField.Name;
                updateCustomField["DbTableTextFieldName"] = vm.selectedTitleField.Name;
            }
            if (vm.selectedFieldType.Id === vm.RELATED_DROPDOWN_TYPE_ID) {
                var stage0 = vm.stagesForDropdown[0];
                updateCustomField["DropdownCustomFieldId"] = stage0.selectedDropdown.Id;
            }
            crudService.update("CustomField", updateCustomField).then(function (updatedObj) {
                if (vm.selectedFieldType.Id === vm.RELATED_DROPDOWN_TYPE_ID) {
                    vm.stagesForDropdown.shift();
                    var body = buildStagesAddDeleteUpdateArrays(updatedObj.Id);
                    networkService.
                        sendRequest(networkService.requestBuilder().method("POST").
                            url("/CustomField/UpdateCustomFieldCollection").body(
                                {
                                    ItemsToAdd: body.add,
                                    ItemsToDelete: body.del,
                                    ItemsToUpdate: body.update
                                }
                            ).build()).then(function (data) {
                                resolve(updatedObj);
                            });
                } else {
                    resolve(updatedObj);
                }
                
            });
        });
    }

    vm.resetModalSelections = function() {
        vm.selectedFieldType = vm.fieldTypes[0];
        vm.selectedIdField = vm.fieldsForTable[0];
        vm.selectedTitleField = vm.fieldsForTable[0];
        vm.selectedTable = vm.tableNames[0];
        resetSelectedCustomField();
        clearStagesForDropdown();
    }


    function resetSelectedCustomField() {
        Object.getOwnPropertyNames(vm.selectedCustomField).forEach(function(val, idx, arr) {
            vm.selectedCustomField[val] = null;
        });
    }

    //load helper handlers
    function dropDownAndMultipleSelectLoaderHelper() {
        return $q(function (resolve, reject) {
            vm.loadTableNames().then(function () {
                vm.selectedTable = vm.tableNames.find(function (val) {
                    return val.Name === vm.selectedCustomField.DbTableName;
                });

                vm.loadFieldsForTable().then(function () {
                    vm.selectedTitleField = vm.fieldsForTable.find(function (val) {
                        return val.Name === vm.selectedCustomField.DbTableTextFieldName;
                    });
                    vm.selectedIdField = vm.fieldsForTable.find(function (val) {
                        return val.Name === vm.selectedCustomField.DbTableIdFieldName;
                    });
                    resolve();
                });
            });
        });
    }

    function stage0LoadHelper() {
        return $q(function (resolve, reject) {
            vm.loadDropdowns().then(function () {
                console.log(vm.selectedCustomField.DropdownCustomFieldId);
                vm.selectedDropdown = vm.dropdowns.find(function (val) {
                    return val.Id === vm.selectedCustomField.DropdownCustomFieldId;
                });
                    var stage0 = buildStageForDropDown();
                    stage0.selectedDropdown= vm.selectedDropdown;
                    stage0.stageId = vm.selectedCustomField.Id;
                    vm.addStageForDropDown(stage0);
                    resolve();
                });
            });
    }

    return vm;

});