ticketWorkflowApp.controller("ticketWorkflowController", function ($timeout, $scope, $q, crudService, ticketWorkflowService) {
    var vm = this;
    var indexOfRowInEdit = -1;

    vm.sharedData = ticketWorkflowService;

    ticketWorkflowService.loadTicketTypes();
    ticketWorkflowService.loadIssueTypes();
    ticketWorkflowService.loadProductNames();
    ticketWorkflowService.loadProductCategories();
    ticketWorkflowService.loadDepartments();
    ticketWorkflowService.loadCustomerPriorities();

    vm.sideMenu = [];
    crudService.loadMenu().then(function (data) {
        vm.sideMenu = data;
    });

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

    ticketWorkflowService.loadticketWorkflow().then(function (data) {
        var table = $("#sample_1").DataTable();
        console.log(data);
        table.rows.add(data).draw();

    });

    vm.addFormField = function () {

         ticketWorkflowService.addFormField(ticketWorkflowService.buildFormField());

    }
    vm.removeFormField = function (index) {
        if (vm.sharedData.formFields.length !== 1) {
            ticketWorkflowService.removeFormField(index);
        }
    }
    
    vm.saveChanges = function () {
        validateForm().then(function(isValid) {
            if (isValid) {
                $.blockElement($("#edit-department"));
                if (indexOfRowInEdit === -1) {
                    //We get max sortorder +1 from the grid. It lets us to move the new template to the bottom
                    var newTicketWorkflowSortOrder = Math.max.apply(Math, $("#sample_1").DataTable().rows().data().map(function(v) { return v["SortOrder"]; })) + 1;

                    ticketWorkflowService.createNewForm(newTicketWorkflowSortOrder).then(function(addedObj) {
                        $.unblockElement($("#edit-department"));
                        vm.closeModal();

                        $("#sample_1").DataTable().row.add(addedObj).draw();
                        toastr["success"]("Ticket sorkflow rule sucessfully created!");
                    });
                } else {

                    ticketWorkflowService.updateForm().then(function (updatedObj) {
                        $.unblockElement($("#edit-department"));
                        toastr["success"]("Ticket workflow rule sucessfully updated!");
                        var tableDataRow = $("#sample_1").DataTable().data()[indexOfRowInEdit];
                        tableDataRow.Name = updatedObj.Name;
                        tableDataRow.IssueType = updatedObj.IssueType;
                        tableDataRow.Product = updatedObj.Product;
                        tableDataRow.ProductCategory = updatedObj.ProductCategory;
                        tableDataRow.TicketType = updatedObj.TicketType;
                        tableDataRow.CustomerPriorityId = updatedObj.CustomerPriorityId;
                        tableDataRow.Department = updatedObj.Department;
                        $("#sample_1").DataTable().row(indexOfRowInEdit).invalidate().draw();
                        vm.closeModal();

                        indexOfRowInEdit = -1;
                    });

                }
            }
        });
    }
   
   

    vm.closeModal = function () {
        closeModal();
    }

    function closeModal() {
        $("#edit-department").modal("hide");
        indexOfRowInEdit = -1;
        ticketWorkflowService.resetModalSelections();
        $scope.ticketWorkflowForm.$setUntouched();
    }


    function validateForm() {
        return $q(function(resolve, reject) {

            if ($scope.ticketWorkflowForm.selectedDepartment.$viewValue.Id === '0') {
                $scope.ticketWorkflowForm.selectedDepartment.$touched = true;
                resolve(false);
                return;
            }

            ticketWorkflowService.validateForm().then(function (errors) {
                if (errors !== "") {
                    errors.forEach(function(error, idx) {
                        toastr["error"](error.ErrorText);
                    });
                    resolve(false);
                } else {
                    resolve(true);
                }
            });
        });

    }

    $timeout(function () {
        var table = $("#sample_1");
        $('#sample_1 tbody').on('click', 'a.delete', function () {
            var row = table.DataTable().row($(this).parents('tr'));
            var idToRemove = table.DataTable().data()[row.index()].Id;
            bootbox.confirm("Are you sure you want to delete this ticket workflow rule?", function(result) {
                if (result) {
                    crudService.remove("TicketWorkflow", idToRemove).then(function() {
                        table.DataTable().row(row).remove().draw();
                        toastr["success"]("Ticket workflow rule sucessfully deleted!");
                    });
                }
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

            vm.sharedData.selectedDepartment = tableDataRow.Department;

            if (tableDataRow.CustomerPriorityId === 0) tableDataRow.CustomerPriorityId = null;

            vm.sharedData.selectedCustomerPriority = (tableDataRow.CustomerPriorityId === null) ? vm.sharedData.customerPriorities[0] : vm.sharedData.customerPriorities.find(function (val) {
                return parseInt(val.Name) === tableDataRow.CustomerPriorityId;
            });



            
            $scope.$apply();
        });
    });

    return vm;
});