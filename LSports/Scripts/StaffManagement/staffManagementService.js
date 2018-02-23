staffManagementApp.service("staffManagementService", function (DropdownDatasource, networkService, crudService, $q) {
    var vm = this;

    vm.departments = [];
    vm.roles = [];

    vm.staff = [];

    vm.selectedUser = {};
    vm.selectedDepartment = {};

    vm.selectedStaff = {
        Id: 0,
        FirstName: "",
        LastName: "",
        Email: "",
        IsAdministrator: false
    };

    vm.departmentRoleTable = [];

    var dds = DropdownDatasource;
    var rb = dds.requestBuilder;

    vm.resetSelectedItem = function() {
        vm.selectedStaff.Id = 0;
        vm.selectedStaff.FirstName = "";
        vm.selectedStaff.LastName = "";
        vm.selectedStaff.Email = "";
        vm.selectedStaff.IsAdministrator = false;
    }

    vm.loadDepartments = function () {
        return $q(function (resolve, reject) {
            dds.sendRequest(rb().setPath("/Departments").setNullValue("-Select department-").build()).then(function (departments) {
                vm.departments = departments;
                vm.selectedDepartment = vm.departments[0];
                resolve();
            });
        });
    }
    vm.loadRoles = function () {
        return $q(function (resolve, reject) {
            dds.sendRequest(rb().setPath("/DepartmentRoles").setNullValue("-Select role-").build()).then(function (roles) {
                vm.roles = roles;
                vm.selectedRole = vm.roles[0];

            });
        });
    }

    vm.loadStaff = function () {
        return $q(function (resolve, reject) {
            crudService.read("StaffManagement").then(function (data) {
                vm.staff.push.apply(vm.staff, data.map(function (val, idx, arr) {
                    val["edit"] = "edit";
                    val["del"] = "delete";
                    return val;
                }));
                console.log(vm.staff);
                resolve(vm.staff);
            });
        });
    }

    vm.createNewStaffMember = function () {
        var newStaffMember = {
            UserName: vm.selectedStaff.Email,
            FirstName: vm.selectedStaff.FirstName,
            LastName: vm.selectedStaff.LastName,
            IsAdministrator: vm.selectedStaff.IsAdministrator
        };

        return $q(function (resolve, reject) {
            crudService.create("StaffManagement", newStaffMember).then(function (addedObj) {

                vm.selectedStaff.Id = addedObj.Id;
                resolve(addedObj);
            });
        });
    }

    vm.updateStaffMember= function () {

        return $q(function (resolve, reject) {
            var updateDepartment = {
                Id: vm.selectedStaff.Id,
                UserName: vm.selectedStaff.Email,
                Email: vm.selectedStaff.Email,
                FirstName: vm.selectedStaff.FirstName,
                LastName: vm.selectedStaff.LastName,
                IsAdministrator: vm.selectedStaff.IsAdministrator
            };
            console.log(updateDepartment);
            crudService.update("StaffManagement", updateDepartment).then(function (updatedObj) {

                updateStaffMemberRecords(updateDepartment).then(resolve, reject);
            });
        });
    }
    var originalDepartmentRoleData = new HashMap();

    vm.loadDepartmentRolesForStaff = function () {
        return $q(function (resolve, reject) {
            networkService.sendRequest(networkService.requestBuilder().
                method("POST").
                url("/StaffManagement/ActionResultListForUser").
                params({ userId: vm.selectedStaff.Id }).build()).then(function (data) {
                    originalDepartmentRoleData.clearMap();
                    data.forEach(function (val, idx, arr) {
                        console.log(val);
                        originalDepartmentRoleData.putValue(val.Department.Id.toString(), { RoleId: val.DepartmentRole.Id, RecordId: val.Id });
                    });
                    resolve(data);

                });
        });
    }

    function updateStaffMemberRecords(updateDepartment) {
        return networkService.sendRequest(networkService.requestBuilder().
            method("POST").
            url("/StaffManagement/UpdateUserDepartments").
            body(buildBody(updateDepartment)).build());

    }

    function buildBody(updateDepartment) {
        var body = {};
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
        tableData.forEach(function (val, idx, arr) {
            res.putValue(val.DepartmentId, val.RoleId); // department name is key
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
        var originalDataKeys = originalDepartmentRoleData.allKeys();
        tableData.forEach(function (val, idx, arr) {
            if (originalDepartmentRoleData.hasKey(val.DepartmentId)) { // same as in the original
                var value = originalDepartmentRoleData.valueForKey(val.DepartmentId);
                if (value.RoleId !== val.RoleId) { // has a new role, schedule for update, otherwise - no role / no name change
                    update.push({
                        DepartmentId: val.DepartmentId,
                        UserId: vm.selectedStaff.Id, // some
                        DepartmentRoleId: val.RoleId, // from 
                        Id: value.RecordId // record id to update
                    });
                }

            } else { // it is in the table, but not in the original data
                add.push({
                    DepartmentId: val.DepartmentId,
                    UserId: vm.selectedStaff.Id,
                    DepartmentRoleId: val.RoleId
                });
            }
        });

        originalDataKeys.forEach(function (val, idx, arr) {
            if (!tableDataHashMap.hasKey(val)) { // if it is on the original data, but not on table
                del.push({ Id: originalDepartmentRoleData.valueForKey(val).RecordId });
            }
        });

        return { add: add, update: update, del: del };
    }


    return vm;

});