departmentApp.controller("departmentController", function($timeout, $scope, $q, crudService, networkService, departmentService) {
    var vm = this;
    var indexOfRowInEdit = -1;

    vm.sharedData = departmentService;

    departmentService.loadUsers();
    departmentService.loadRoles();

    vm.addUserRole = function() {
        addUserRoleToTable({
            Name: vm.sharedData.selectedUser.Name,
            Role: vm.sharedData.selectedRole.Name,
            del: "delete",
            UserId: vm.sharedData.selectedUser.Id,
            RoleId: vm.sharedData.selectedRole.Id
        });
    }


    vm.sideMenu = [];
    crudService.loadMenu().then(function (data) {
        vm.sideMenu = data;
    });

    function addUserRoleToTable(userRole) {
        if (userRole.Name !== "-Select user-" && userRole.Role !== "-Select role-") {
            var userRoleTable = $("#user_role_table").DataTable();

            var needToAdd = true;
            userRoleTable.rows().eq(0).each(function(index) {
                var row = userRoleTable.row(index);
                var rowData = row.data();
                if (rowData.UserId === userRole.UserId && rowData.RoleId === userRole.RoleId) {
                    needToAdd = false;
                }
                if (rowData.UserId === userRole.UserId && rowData.RoleId !== userRole.RoleId) {
                    rowData.RoleId = userRole.RoleId;
                    rowData.Role = userRole.Role;
                    needToAdd = false;
                    row.invalidate().draw();
                }

            });

            if (needToAdd) {
                userRoleTable.row.add(userRole).draw();
            }
        } else {
            $scope.popupForm["selectedUser"].$touched = true;
            $scope.popupForm["selectedRole"].$touched = true;
        }

    }




    $timeout(function () {
        var table = $("#sample_1");
        $("#sample_1 tbody").on("click", "a.delete", function () {

            var row = table.DataTable().row($(this).parents("tr"));
            var idToRemove = table.DataTable().data()[row.index()].Id;
            bootbox.confirm("Are you sure you want to delete this department?", function (result) {
                if (result) {
                    crudService.remove("Department", idToRemove).then(function () {
                        table.DataTable().row(row).remove().draw();
                        toastr["success"]("Department was successfully removed!");
                    });
                }
            });
            
        });
        $("#sample_1 tbody").on("click", "a.edit", function () {
            indexOfRowInEdit = table.DataTable().row($(this).parents("tr")).index();
            vm.sharedData.selectedDepartment.Name = table.DataTable().data()[indexOfRowInEdit].Name;
            vm.sharedData.selectedDepartment.Id = table.DataTable().data()[indexOfRowInEdit].Id;
            vm.sharedData.selectedDepartment.CanSeeCustomerDetails = table.DataTable().data()[indexOfRowInEdit].CanSeeCustomerDetails;
            $scope.$apply();

            departmentService.loadUserRoleListForDepartment().then(function (data) {
                $("#user_role_table").DataTable().rows.add(data.map(function (val, idx, arr) {
                    return {
                        Name: val.User.Email,
                        Role: val.DepartmentRole.Name,
                        del: "delete",
                        UserId: val.User.Id,
                        RoleId: val.DepartmentRole.Id
                    }
                })).draw();
            });

        });
        //load departments
        departmentService.loadDepartments().then(function() {

            table.DataTable().clear().draw();
            table.DataTable().rows.add(vm.sharedData.departments).draw();
        });

        var userRoleDatable = $("#user_role_table").DataTable();
        $("#user_role_table").on("click", "a.delete", function() {
            userRoleDatable.row(userRoleDatable.row($(this).parents("tr"))).remove().draw();
        });


    });
    vm.popupActionName = "Edit";
    vm.popupIsEditing = true;

    vm.addNewDepartment = function() {
        vm.popupActionName = "Add";
        vm.popupIsEditing = false;
    }




    vm.createDepartment = function () {
        $.blockElement($("#edit-department"));
        validateForm().then(function (data) {

            if (data) {
                createNewDepartment().then(function () {
                    $.unblockElement($("#edit-department"));
                    vm.popupIsEditing = true;
                    toastr["success"]("Department was successfully created!");
                });
            } else {
                $.unblockElement($("#edit-department"));
            }
        });
        

    }



    vm.saveChanges = function () {
        var table = $("#sample_1").DataTable();
        $.blockElement($("#edit-department"));
        validateForm().then(function(data) {

            if (data && vm.popupIsEditing) {
                if (indexOfRowInEdit === -1) { // saving changes for the freshly added department

                    departmentService.updateDepartment().then(function () {
                        $.unblockElement($("#edit-department"));
                        vm.closeModal();
                        toastr["success"]("Department was successfully updated!");
                    });
                } else {
                    departmentService.updateDepartment().then(function () {
                        $.unblockElement($("#edit-department"));
                        table.data()[indexOfRowInEdit].Name = vm.sharedData.selectedDepartment.Name;
                        table.data()[indexOfRowInEdit].CanSeeCustomerDetails = vm.sharedData.selectedDepartment.CanSeeCustomerDetails;
                        table.row(indexOfRowInEdit).invalidate().draw();
                        vm.closeModal();
                        toastr["success"]("Department was successfully updated!");
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

        vm.sharedData.selectedDepartment.Name = "";
        vm.sharedData.selectedDepartment.CanSeeCustomerDetails = false;
        vm.sharedData.selectedDepartment.Id = 0;

        vm.popupActionName = "Edit";
        vm.popupIsEditing = true;

        $("#user_role_table").DataTable().clear().draw();
        $scope.addEditDepartmentForm["departmentName"].$touched = false;
        $scope.popupForm.$setUntouched();
        vm.sharedData.selectedRole = vm.sharedData.roles[0];
        vm.sharedData.selectedUser = vm.sharedData.users[0];
    }



    function createNewDepartment() {
        return $q(function(resolve, reject) {
            departmentService.createNewDepartment().then(function (addedObj) {
                var table = $("#sample_1").DataTable();
                table.row.add({
                    Id: addedObj.Id,
                    Name: addedObj.Name,
                    CanSeeCustomerDetails: addedObj.CanSeeCustomerDetails,
                    edit: "edit",
                    del: "delete"
                }).draw();
                resolve();
            });
        });
    }



    function validateForm() {
        return $q(function(resolve, reject) {
            networkService.isDepartmentNameUnique(vm.sharedData.selectedDepartment.Name, vm.sharedData.selectedDepartment.Id).then(function (data) {
                if (!data) {
                    toastr["error"]("Department name is not unique!");
                }

                if ($scope.addEditDepartmentForm["departmentName"].$invalid)
                    $scope.addEditDepartmentForm["departmentName"].$touched = true;

                resolve((!$scope.addEditDepartmentForm["departmentName"].$invalid) && data);
            });
        });
    }


    return vm;
});