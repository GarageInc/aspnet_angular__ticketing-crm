customFieldsApp.controller("customFieldsController", function ($timeout, $scope, $q, crudService, networkService, customFieldsService) {
    var vm = this;
    var indexOfRowInEdit = -1;

    vm.DROPDOWN_TYPE_ID = 3;
    vm.RELATED_DROPDOWN_TYPE_ID = 4;
    vm.MULTIPLE_SELECT_TYPE_ID = 10;

    vm.sharedData = customFieldsService;

    //departmentService.loadRoles();

    vm.sideMenu = [];
    crudService.loadMenu().then(function (data) {
        vm.sideMenu = data;
    });

    customFieldsService.loadFieldTypes();
    customFieldsService.loadDropdowns();

    customFieldsService.loadCustomFields().then(function (data) {
        console.log(data);
        var table = $("#sample_1").DataTable();
        table.rows.add(data).draw();

    });
    vm.fieldTypeSelectionChanged = function() {

        if (vm.sharedData.selectedFieldType.Id === vm.DROPDOWN_TYPE_ID ||
            vm.sharedData.selectedFieldType.Id === vm.MULTIPLE_SELECT_TYPE_ID) {
            customFieldsService.loadTableNames();
        }

        if (vm.sharedData.selectedFieldType.Id === vm.RELATED_DROPDOWN_TYPE_ID) {
                customFieldsService.loadDropdowns().then(function () {
                    console.log(vm.sharedData.dropdowns);
                    vm.sharedData.addStageForDropDown(vm.sharedData.buildStageForDropDown());                  
                });
            }
    }

    vm.addStage = function() {
        customFieldsService.addStageForDropDown(vm.sharedData.buildStageForDropDown());
    }
    vm.removeStage = function(index) {
        customFieldsService.removeStage(index);
    }

    vm.tableNameSelectionChanged = function() {
        if ($scope.customFieldForm.selectedTable.$viewValue.Id !== '0')
        customFieldsService.loadFieldsForTable();
    }

    vm.stageDropdownSelectionChanged = function (index) {
        if ($scope.customFieldForm['stage'+index].$viewValue.Id !== '0') {
            var stage = vm.sharedData.stagesForDropdown[index];
            customFieldsService.loadFieldsForTableName(stage.selectedDropdown.TableName).then(function(data) {
                stage.selectedFilterField = data[0];
                stage.fieldsForTable = data;
            });
            
        }
    }

    vm.saveChanges = function () {
        var table = $("#sample_1").DataTable();
        $.blockElement($("#edit-department"));
        validateForm().then(function(data) {
            if (data) {
                if (indexOfRowInEdit === -1) { // saving changes for the freshly added department
                    customFieldsService.createNewCustomField().then(function (createdField) {
                        $.unblockElement($("#edit-department"));
                        table.row.add(createdField).draw();
                        vm.closeModal();
                        toastr["success"]("Custom field was successfully created!");
                    });

                } else {
                    customFieldsService.updateCustomField().then(function (updatedObj) {
                        $.unblockElement($("#edit-department"));
                        console.log(updatedObj);
                        table.data()[indexOfRowInEdit].Name = updatedObj.Name;
                        table.data()[indexOfRowInEdit].Id = updatedObj.Id;
                        table.data()[indexOfRowInEdit].Title = updatedObj.Title;
                        table.data()[indexOfRowInEdit].Identifier = updatedObj.Identifier;
                        table.data()[indexOfRowInEdit].PlaceholderText = updatedObj.PlaceholderText;
                        table.data()[indexOfRowInEdit].CustomFieldType = updatedObj.CustomFieldType;
                        table.data()[indexOfRowInEdit].DbTableName = updatedObj.DbTableName;
                        table.data()[indexOfRowInEdit].DbTableIdFieldName = updatedObj.DbTableIdFieldName;
                        table.data()[indexOfRowInEdit].DbTableTextFieldName = updatedObj.DbTableTextFieldName;
                        table.data()[indexOfRowInEdit].DropdownCustomFieldId = updatedObj.DropdownCustomFieldId;


                        table.row(indexOfRowInEdit).invalidate().draw();
                        vm.closeModal();
                        toastr["success"]("Custom field was successfully updated!");
                    });
                }
            } else {
                $.unblockElement($("#edit-department"));
            }
        });
    }

    vm.closeModal = function () {
        closeModal();
    }

    function closeModal() {
        $("#edit-department").modal("hide");
        indexOfRowInEdit = -1;
        customFieldsService.resetModalSelections();
        $scope.customFieldForm.$setUntouched();
    }

    $timeout(function () {
        var table = $("#sample_1");
        $('#sample_1 tbody').on('click', 'a.delete', function () {
            var row = table.DataTable().row($(this).parents('tr'));
            var idToRemove = table.DataTable().data()[row.index()].Id;
            bootbox.confirm("Are you sure you want to delete this custom field?", function (result) {
                if (result) {
                    crudService.remove("CustomField", idToRemove).then(function() {
                        table.DataTable().row(row).remove().draw();
                        toastr["success"]("Custom field was successfully removed!");
                    });
                }
            });
        });

        $('#sample_1 tbody').on('click', 'a.edit', function () {
            indexOfRowInEdit = table.DataTable().row($(this).parents('tr')).index();
            console.log(table.DataTable().data()[indexOfRowInEdit]);
            //loading object from datable to view
            vm.sharedData.selectedCustomField.Name = table.DataTable().data()[indexOfRowInEdit].Name;
            vm.sharedData.selectedCustomField.Id = table.DataTable().data()[indexOfRowInEdit].Id;
            vm.sharedData.selectedCustomField.Title = table.DataTable().data()[indexOfRowInEdit].Title;
            vm.sharedData.selectedCustomField.Identifier = table.DataTable().data()[indexOfRowInEdit].Identifier;
            vm.sharedData.selectedCustomField.PlaceholderText = table.DataTable().data()[indexOfRowInEdit].PlaceholderText;
            vm.sharedData.selectedCustomField.CustomFieldType = table.DataTable().data()[indexOfRowInEdit].CustomFieldType;
            vm.sharedData.selectedCustomField.DbTableName = table.DataTable().data()[indexOfRowInEdit].DbTableName;
            vm.sharedData.selectedCustomField.DbTableIdFieldName = table.DataTable().data()[indexOfRowInEdit].DbTableIdFieldName;
            vm.sharedData.selectedCustomField.DbTableTextFieldName = table.DataTable().data()[indexOfRowInEdit].DbTableTextFieldName;
            vm.sharedData.selectedCustomField.DropdownCustomFieldId = table.DataTable().data()[indexOfRowInEdit].DropdownCustomFieldId;

            customFieldsService.loadCustomFieldIntoViewHelper();

            $scope.$apply();

        });
    });



    function validateForm() {
        return $q(function (resolve, reject) {

            var isFormValid = true;

            if ($scope.customFieldForm.Name.$invalid) {
                $scope.customFieldForm.Name.$touched = true;
                isFormValid = false;
            }

            if ($scope.customFieldForm.Identifier.$invalid) {
                $scope.customFieldForm.Identifier.$touched = true;
                isFormValid = false;
            }

            if ($scope.customFieldForm.selectedFieldType.$viewValue.Id === '0') {
                $scope.customFieldForm.selectedFieldType.$touched = true;
                isFormValid = false;
            }

            //If dropdown or multiple select we need to check fields
            if (vm.sharedData.selectedFieldType.Id === vm.DROPDOWN_TYPE_ID ||
                vm.sharedData.selectedFieldType.Id === vm.MULTIPLE_SELECT_TYPE_ID) {
               
                if ($scope.customFieldForm.selectedTable.$viewValue.Id === '0') {
                    $scope.customFieldForm.selectedTable.$touched = true;
                    isFormValid = false;
                }
                if ($scope.customFieldForm.selectedIdField.$viewValue.Id === '0') {
                    $scope.customFieldForm.selectedIdField.$touched = true;
                    isFormValid = false;
                }
                if ($scope.customFieldForm.selectedTitleField.$viewValue.Id === '0') {
                    $scope.customFieldForm.selectedTitleField.$touched = true;
                    isFormValid = false;
                }
            }




            if (vm.sharedData.selectedFieldType.Id === vm.RELATED_DROPDOWN_TYPE_ID) {
                customFieldsService.stagesForDropdown.forEach(function(val, idx) {
                    var stageName = 'stage' + idx;
                    var stageFilterName = 'stageFilter' + idx;
                    if ($scope.customFieldForm[stageName].$viewValue.Id === '0') {
                        $scope.customFieldForm[stageName].$touched = true;
                        isFormValid = false;
                    }
                    if (idx !== 0 && $scope.customFieldForm[stageFilterName].$viewValue.Id === '0') {
                        $scope.customFieldForm[stageFilterName].$touched = true;
                        isFormValid = false;
                    }
                });
            }


            if (!isFormValid) {
                resolve(false);
                return;
            }

            networkService.isCustomFieldValuesUnique(vm.sharedData.selectedCustomField.Name, vm.sharedData.selectedCustomField.Identifier, vm.sharedData.selectedCustomField.Id || 0).then(function (data) {

                if (data !== "0")
                    toastr["error"](data);

                if (data.includes("Name"))
                    $scope.customFieldForm.Name.$invalid = true;

                if (data.includes("Identifier"))
                    $scope.customFieldForm.Identifier.$invalid = true;

                resolve(data === "0" && isFormValid);
            });
        });
    }

    return vm;
});