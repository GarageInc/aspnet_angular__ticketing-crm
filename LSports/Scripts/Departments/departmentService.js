departmentApp.service("departmentService", function(DropdownDatasource, networkService, crudService,$q) {
    var vm = this;

    vm.users = [];
    vm.roles = [];

    vm.departments = [];

    vm.selectedUser = {};
    vm.selectedRole = {};

    vm.selectedDepartment = { Id: 0, Name: "", CanSeeCustomerDetails: false };

    vm.userRoleTable = [];

    var dds = DropdownDatasource;
    var rb = dds.requestBuilder;

    vm.loadUsers = function() {
        return $q(function(resolve, reject) {
            dds.sendRequest(rb().setPath("/Staff").setNullValue("-Select user-").build()).then(function(users) {
                vm.users = users;
                vm.selectedUser = vm.users[0];
                resolve();
            });
        });
    }
    vm.loadRoles = function() {
        return $q(function(resolve, reject) {
            dds.sendRequest(rb().setPath("/DepartmentRoles").setNullValue("-Select role-").build()).then(function(roles) {
                vm.roles = roles;
                vm.selectedRole = vm.roles[0];

            });
        });
    }

    vm.loadDepartments = function() {
        return $q(function(resolve, reject) {
            crudService.read("Department").then(function(data) {           
                for (var i = 0; i < data.length; i++) {
                    vm.departments.push({ Id: data[i].Id, Name: data[i].Name, CanSeeCustomerDetails: data[i].CanSeeCustomerDetails, edit: "edit", del: "delete" });
                }
                resolve();
            });
        });
    }

    vm.createNewDepartment = function() {
        var newDepartment = {
            Name: vm.selectedDepartment.Name,
            CanSeeCustomerDetails: vm.selectedDepartment.CanSeeCustomerDetails
        };

        return $q(function (resolve, reject) {
            crudService.create("Department", newDepartment).then(function (addedObj) {
                vm.selectedDepartment.Id = addedObj.Id;
                resolve(addedObj);
            });
        });
    }

    vm.updateDepartment = function() {

        return $q(function (resolve, reject) {
            var updateDepartment = {
                Id: vm.selectedDepartment.Id,
                Name: vm.selectedDepartment.Name,
                CanSeeCustomerDetails: vm.selectedDepartment.CanSeeCustomerDetails
            };
            crudService.update("Department", updateDepartment).then(function (updatedObj) {

                updateDepartmentalRecords(updateDepartment).then(resolve, reject);
            });
        });
    }
    var originalUserRoleData = new HashMap();
    vm.loadUserRoleListForDepartment = function() {
        return $q(function(resolve, reject) {
            networkService.sendRequest(networkService.requestBuilder().
                method("POST").
                url("/Department/ListForDepartment").
                params({ id: vm.selectedDepartment.Id }).build()).then(function (data) {
                    originalUserRoleData.clearMap();
                    data.forEach(function (val, idx, arr) {

                        
                        originalUserRoleData.putValue(val.User.Id, { RoleId: val.DepartmentRole.Id, RecordId: val.Id });
                        
                    });
               resolve(data);
                
            });
        });
    }

    function updateDepartmentalRecords(updateDepartment) {
        return networkService.sendRequest(networkService.requestBuilder().
            method("POST").
            url("/Department/UpdateUserDepartments").
            body(buildBody(updateDepartment)).build());

    }

    function buildBody(updateDepartment) {
        var body = {
            Id: updateDepartment.Id,
            Name: updateDepartment.Name,
            CanSeeCustomerDetails: updateDepartment.CanSeeCustomerDetails,
            
        };
        var items = getItems();
        if (items.add.length > 0)
            body["ItemsToAdd"] = items.add;
        if (items.update.length > 0)
            body["ItemsToUpdate"] = items.update;
        if (items.del.length > 0)
            body["ItemsToDelete"] = items.del;
        console.log(body);
        return body;

    }

    function makeTableDataToHashmap(tableData) {
        var res = new HashMap();
        tableData.forEach(function(val, idx, arr) {
            res.putValue(val.UserId, val.RoleId);
        });
        return res;
    }

    function getItems() {
        var userRoleTable = $("#user_role_table").DataTable();
        var tableData = userRoleTable.data().toArray();

        var tableDataHashMap = makeTableDataToHashmap(tableData);

        var add = [];
        var update = [];
        var del = [];

        //var count = 0;
        var originalDataKeys = originalUserRoleData.allKeys();
        tableData.forEach(function(val, idx, arr) {
            if (originalUserRoleData.hasKey(val.UserId)) { // same as in the original
                var value = originalUserRoleData.valueForKey(val.UserId);
                if (value.RoleId !== val.RoleId) { // has a new role, schedule for update, otherwise - no role / no name change
                    update.push({
                        DepartmentId: vm.selectedDepartment.Id,
                        UserId: val.UserId, // some
                        DepartmentRoleId: val.RoleId, // from 
                        Id : value.RecordId // record id to update
                    });
                }

            } else { // it is in the table, but not in the original data
                add.push({
                    DepartmentId: vm.selectedDepartment.Id,
                    UserId: val.UserId,
                    DepartmentRoleId: val.RoleId
                });
            }
        });

        originalDataKeys.forEach(function(val, idx, arr) {
            if (!tableDataHashMap.hasKey(val)) { // if it is on the original data, but not on table
                del.push({ Id: originalUserRoleData.valueForKey(val).RecordId });
            }
        });

        return { add: add, update: update, del: del };
    }
    return vm;

});