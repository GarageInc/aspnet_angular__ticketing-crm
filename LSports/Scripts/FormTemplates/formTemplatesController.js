formTemplatesApp.controller("formTemplatesController", function ($timeout, $scope, $q, crudService, formTemplatesService) {
    var vm = this;
    var indexOfRowInEdit = -1;

    vm.sharedData = formTemplatesService;

    vm.sideMenu = [];
    crudService.loadMenu().then(function (data) {
        vm.sideMenu = data;
    });

    formTemplatesService.loadTicketTypes();
    formTemplatesService.loadIssueTypes();
    formTemplatesService.loadProductNames();
    formTemplatesService.loadCustomFields();
    formTemplatesService.loadProductCategories();

    vm.fieldTypesEnum = {
        Dropdown : 3,
        Text: 1, // check
        LongText: 2, // check
        RelatedDropdowns: 4,
        Checkbox: 5, // check
        Date: 6,
        Attachments: 7,
        Numeric: 8,
        Email: 9, // check
        MultipleSelection:10
    };

    formTemplatesService.loadFormTemplates().then(function (data) {
        var table = $("#sample_1").DataTable();
        table.rows.add(data).draw();

    });

    vm.addFormField = function () {

         formTemplatesService.addFormField(formTemplatesService.buildFormField());

    }
    vm.removeFormField = function (index) {
        if (vm.sharedData.formFields.length !== 1) {
            formTemplatesService.removeFormField(index);
        }
    }
    
    vm.saveChanges = function () {
        $.blockElement($("#edit-department"));
        validateForm().then(function(isValid) {
            if (isValid) {
                if (indexOfRowInEdit === -1) {
                    //We get max sortorder +1 from the grid. It lets us to move the new template to the bottom
                    var newFormTemplateSortOrder = Math.max.apply(Math, $("#sample_1").DataTable().rows().data().map(function(v) { return v["SortOrder"]; })) + 1;

                    formTemplatesService.createNewForm(newFormTemplateSortOrder).then(function(addedObj) {
                        $.unblockElement($("#edit-department"));
                        vm.closeModal();

                        $("#sample_1").DataTable().row.add(addedObj).draw();
                        toastr["success"]("Template sucessfully created!");
                    });
                } else {

                    formTemplatesService.updateForm().then(function (updatedObj) {
                        $.unblockElement($("#edit-department"));
                        toastr["success"]("Template sucessfully updated!");
                        var tableDataRow = $("#sample_1").DataTable().data()[indexOfRowInEdit];
                        tableDataRow.Name = updatedObj.Name;
                        tableDataRow.IssueType = updatedObj.IssueType;
                        tableDataRow.Product = updatedObj.Product;
                        tableDataRow.ProductCategory = updatedObj.ProductCategory;
                        tableDataRow.TicketType = updatedObj.TicketType;
                        $("#sample_1").DataTable().row(indexOfRowInEdit).invalidate().draw();
                        vm.closeModal();

                        indexOfRowInEdit = -1;
                    });
                }
            } else {
                $.unblockElement($("#edit-department"));
            }
        });
    }


    vm.showFormEditor = function() {
        vm.sharedData.renderedFormFields = [];
    }

    vm.multiselectDropdownIds = {};
    vm.dateIds = {};

    vm.addNewFormTemplate = function() {

        formTemplatesService.addFormField(formTemplatesService.buildFormField());
        vm.sharedData.editingFormId = 0;
        $("#edit-department").modal("show");
    }


    vm.showFormPreview = function () {
        vm.multiselectDropdownIds = {};
        $.blockElement($("#edit-department"));
        formTemplatesService.loadDropdownsForForm().then(function () {
            vm.sharedData.formFields.forEach(function(val, idx, arr) {
                if (val.selectedCustomField.CustomFieldType.Id=== vm.fieldTypesEnum.MultipleSelection) {
                    vm.multiselectDropdownIds[val.selectedCustomField.Name] = "m" + idx;
                }
                if (val.selectedCustomField.CustomFieldType.Id === vm.fieldTypesEnum.Date) {
                    vm.dateIds[val.selectedCustomField.Name] = "d" + idx;
                }
            });

            vm.sharedData.renderedFormFields = vm.sharedData.formFields;
            console.log(vm.sharedData.renderedFormFields);
            $timeout(function() {
                Object.getOwnPropertyNames(vm.multiselectDropdownIds).forEach(function (val, idx, arr) {
                    $("#" + vm.multiselectDropdownIds[val]).multiselect({maxHeight:200});
                });
                $('.date-picker').datepicker();
                $.unblockElement($("#edit-department"));
            });
        });

    }


    var relatedToReload = {};

    vm.relatedDropdownSelectionChanged = function(index, fieldName) {
        relatedToReload = {};
        if (index === 0) {
            vm.sharedData.dropdownsForForm[fieldName].relatedDropdowns.forEach(function(val, idx) {
                if (idx === 0) {
                    relatedToReload = vm.sharedData.dropdownsForForm[fieldName].relatedDropdowns[idx];
                    relatedToReload['filterValue'] = vm.sharedData.dropdownsForForm[fieldName].dummyModel.Id;
                } else {
                    val.options = [];
                    val.selectedOption = { Id: "", Name: val.PlaceholderText };
                }
            });

            formTemplatesService.loadRelatedOptions(relatedToReload).then(function (data) {
                vm.sharedData.dropdownsForForm[fieldName].relatedDropdowns[0].options = data;
                vm.sharedData.dropdownsForForm[fieldName].relatedDropdowns[0].selectedOption = data[0];
            });
        } else {
            vm.sharedData.dropdownsForForm[fieldName].relatedDropdowns.forEach(function (val, idx) {
                if (idx === index+1) {
                    relatedToReload = vm.sharedData.dropdownsForForm[fieldName].relatedDropdowns[idx];
                    relatedToReload['filterValue'] = vm.sharedData.dropdownsForForm[fieldName].relatedDropdowns[idx - 1].selectedOption.Id;
                } else {
                    val.options = [];
                    val.selectedOption = { Id: "", Name: val.PlaceholderText };
                }
            });

            formTemplatesService.loadRelatedOptions(relatedToReload).then(function (data) {
                vm.sharedData.dropdownsForForm[fieldName].relatedDropdowns[index + 1].options = data;
                vm.sharedData.dropdownsForForm[fieldName].relatedDropdowns[index + 1].selectedOption = data[0];
            });
        }

    }



    vm.closeModal = function () {
        closeModal();
    }



    function closeModal() {
        $("#edit-department").modal("hide");
        indexOfRowInEdit = -1;
        formTemplatesService.resetModalSelections();
        $("#edit-tab-li").addClass("active");
        $("#edit_tab").addClass("active");
        $("#preview-tab-li").removeClass("active");
        $("#preview_tab").removeClass("active");
        $scope.formTemplateForm.$setUntouched();
    }

    var startIndex = -1;


    function validateForm() {
        return $q(function (resolve, reject) {

            var isFormValid = true;

            if ($scope.formTemplateForm.templateName.$invalid) {
                $scope.formTemplateForm.templateName.$touched = true;
                isFormValid = false;
            }

            formTemplatesService.formFields.forEach(function (val, idx) {
                var stageName = "formElem"+idx;
                if ($scope.formTemplateForm[stageName].$viewValue.Id === '0') {
                    $scope.formTemplateForm[stageName].$touched = true;
                    isFormValid = false;
                }
                
            });

            if (isFormValid === false) {
                resolve(isFormValid);
                return;
            }

            formTemplatesService.validateForm().then(function (errors) {
                if (errors !== "") {
                    errors.forEach(function (error, idx) {
                        toastr["error"](error.ErrorText);
                        if (error.ErrorText.indexOf("name") !== -1) {
                            $scope.formTemplateForm.templateName.$invalid = true;
                            $scope.formTemplateForm.templateName.$touched = true;
                        }
                    });
                    resolve(false);
                } else {
                    resolve(true);
                }
            });
        });
    }



    $scope.sortableOptions = {
        axis:'y'
    };



    vm.deleteFormTemplate = function () {
        bootbox.confirm("Are you sure you want to delete this form template?", function (result) {
            if (result) {
                if (indexOfRowInEdit !== -1) {
                    var table = $("#sample_1");
                    var row = table.DataTable().row(indexOfRowInEdit);
                    var idToRemove = table.DataTable().data()[row.index()].Id;
                    $.blockElement($("body"));
                    crudService.remove("FormTemplate", idToRemove).then(function () {
                        $.unblockElement($("body"));
                        table.DataTable().row(row).remove().draw();
                        closeModal();
                        toastr["success"]("Template sucessfully deleted!");
                    });
                    indexOfRowInEdit = -1;
                }
            }
        });
    }



    $timeout(function () {
        var table = $("#sample_1");
        $('#sample_1 tbody').on('click', 'a.delete', function () {
            var row = table.DataTable().row($(this).parents('tr'));
            var idToRemove = table.DataTable().data()[row.index()].Id;
            bootbox.confirm("Are you sure you want to delete this form template?", function(result) {
                if (result) {
                    crudService.remove("FormTemplate", idToRemove).then(function() {
                        table.DataTable().row(row).remove().draw();
                        toastr["success"]("Template sucessfully deleted!");
                    });
                };
            });
        });
        $('#sample_1 tbody').on('click', 'a.edit', function() {
            indexOfRowInEdit = $("#sample_1").DataTable().row($(this).parents('tr')).index();
            var tableDataRow = $("#sample_1").DataTable().data()[indexOfRowInEdit];
            vm.sharedData.editingFormId = tableDataRow.Id;
            vm.sharedData.editingFormSortOrder = tableDataRow.SortOrder;
            vm.sharedData.formName = tableDataRow.Name;

            vm.sharedData.selectedIssueType = tableDataRow.IssueType === null ? vm.sharedData.issueTypes[0] : tableDataRow.IssueType;
            vm.sharedData.selectedProductName = tableDataRow.Product === null ? vm.sharedData.productNames[0] : tableDataRow.Product;
            vm.sharedData.selectedProductCategory = tableDataRow.ProductCategory === null ? vm.sharedData.productCategories[0] : tableDataRow.ProductCategory;
            vm.sharedData.selectedTicketType = tableDataRow.TicketType === null ? vm.sharedData.ticketTypes[0] : tableDataRow.TicketType;

            formTemplatesService.loadFormFieldsList(tableDataRow.Id).then(function(data) {
                var sorted = data.sort(function(ff1, ff2) {
                    if (ff1.SortOrder > ff2.SortOrder)
                        return 1;
                    if (ff1.SortOrder < ff2.SortOrder)
                        return -1;
                    return 0;
                });
                vm.sharedData.formFields = sorted.map(function(val, idx, arr) {
                    return {selectedCustomField: vm.sharedData.customFields.find(function(val1) { return val1.Id === val.CustomField.Id; })}
                });

            });
            $scope.$apply();
        });
    });

    return vm;
});