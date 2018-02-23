ticketWorkflowApp.service("ticketWorkflowService", function(DropdownDatasource, networkService, crudService, $q) {
    var vm = this;

    vm.issueTypes = [];
    vm.selectedIssueType = {};

    vm.ticketTypes = [];
    vm.selectedTicketType = {};

    vm.productNames = [];
    vm.selectedProductName = {};

    vm.productCategories = [];
    vm.selectedProductCategory = {};

    vm.departments = [];
    vm.selectedDepartment = {};

    vm.customerPriorities = [];
    vm.selectedCustomerPriority = {};

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


    vm.loadDepartments = function () {
        return $q(function (resolve, reject) {
            dds.sendRequest(rb().setPath("/Departments").setNullValue("-Select Department-").build()).then(function (departments) {
                vm.departments = departments;
                vm.selectedDepartment = departments[0];
                resolve();
            });
        });
    }


    vm.loadCustomerPriorities = function () {
        return $q(function (resolve, reject) {
            dds.sendRequest(rb().setPath("/CustomerPriorities").setNullValue("ANY").build()).then(function (customerPriorities) {
                vm.customerPriorities = customerPriorities;
                vm.selectedCustomerPriority = customerPriorities[0];
                resolve();
            });
        });
    }

  

    vm.loadFieldsForTable = function() {
        return $q(function(resolve, reject) {
            dds.sendRequest(rb().setPath("/TableFieldNames").
                setNullValue("ANY").
                setParams({ dbTableName: vm.selectedTable.Name }).
                setCacheKey(vm.selectedTable.Name).build()).then(function(data) {
                vm.fieldsForTable = data;
                vm.selectedIdField = data[0];
                vm.selectedTitleField = data[0];
            });
        });
    }

    

    //main table
    vm.loadticketWorkflow = function() {
        return $q(function(resolve, reject) {
            crudService.read("ticketWorkflow").then(function(data) {
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

    vm.createNewForm = function(sortOrder) {
        var body = {
            
            TicketType: vm.selectedTicketType,
            Product: vm.selectedProductName,
            IssueType: vm.selectedIssueType,
            ProductCategory: vm.selectedProductCategory,
            SortOrder: sortOrder,
            CustomerPriorityId: vm.selectedCustomerPriority.Id,
            Department: vm.selectedDepartment
        };
        
        return $q(function(resolve, reject) {
            networkService.sendRequest(networkService.requestBuilder().method("POST").url("/TicketWorkflow/Insert").body(body).build()).
                then(function (addedObj) {
                    addedObj["edit"] = "edit";
                    addedObj["del"] = "delete";
                    resolve(addedObj);
                });
        });
        
    }

    vm.updateForm = function() {
        var body = {
            TicketType: vm.selectedTicketType,
            Product: vm.selectedProductName,
            IssueType: vm.selectedIssueType,
            ProductCategory: vm.selectedProductCategory,
            SortOrder: vm.editingFormSortOrder,
            Id: vm.editingFormId,
            CustomerPriorityId: vm.selectedCustomerPriority.Id,
            Department: vm.selectedDepartment
        };

        return $q(function (resolve, reject) {
            networkService.sendRequest(networkService.requestBuilder().method("POST").url("/TicketWorkflow/Update").body(body).build()).
                then(function (addedObj) {
                    resolve(addedObj);
                });
        });
    }


    vm.validateForm = function() {
        var body = {
            TicketType: vm.selectedTicketType,
            Product: vm.selectedProductName,
            IssueType: vm.selectedIssueType,
            ProductCategory: vm.selectedProductCategory,
            Id: vm.editingFormId,
            CustomerPriorityId: vm.selectedCustomerPriority.Id,
            Department: vm.selectedDepartment
        };

        return $q(function (resolve, reject) {
            networkService.sendRequest(networkService.requestBuilder().method("POST").url("/TicketWorkflow/Validate").body(body).build()).
                then(function (errors) {
                    resolve(errors);
                });
        });
    }

    vm.updateSortOrder = function (newOrder) {
        return networkService.sendRequest(networkService.requestBuilder().method("POST").url("/TicketWorkflow/UpdateSortOrder").body(newOrder).build());
    }

    vm.resetModalSelections = function() {
        vm.selectedIssueType = vm.issueTypes[0];
        vm.selectedTicketType = vm.ticketTypes[0];
        vm.selectedProductName = vm.productNames[0];
        vm.selectedProductCategory = vm.productCategories[0];
        vm.selectedCustomerPriority = vm.customerPriorities[0];
        vm.selectedDepartment = vm.departments[0];
        vm.formName = "";
        vm.formFields.splice(0, vm.formFields.length);
    }

    return vm;

});