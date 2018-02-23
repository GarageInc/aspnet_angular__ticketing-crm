staffManagementApp.controller("staffManagementController", function ($timeout, $scope, $q,crudService, staffManagementService, networkService) {
    var vm = this;

    vm.sharedData = staffManagementService;
    var indexOfRowInEdit = -1;

    staffManagementService.loadDepartments();
    staffManagementService.loadRoles();

    vm.sideMenu = [];
    crudService.loadMenu().then(function (data) {
        vm.sideMenu = data;
    });

    vm.addDepartmentRole = function () {
        addUserRoleToTable(
            {
                Name: vm.sharedData.selectedDepartment.Name,
                Role: vm.sharedData.selectedRole.Name,
                del: "delete",
                DepartmentId: vm.sharedData.selectedDepartment.Id.toString(),
                RoleId: vm.sharedData.selectedRole.Id
            });
    }


    $scope.emailFormat = /^[a-z]+[a-z0-9.\-_]+@[a-z]+\.[a-z.]{2,5}$/;



    function addUserRoleToTable(userRole) {
        if (userRole.Name !== '-Select department-' && userRole.Role !== '-Select role-') {
            var userRoleTable = $("#user_role_table").DataTable();

            var needToAdd = true;
            userRoleTable.rows().eq(0).each(function(index) {
                var row = userRoleTable.row(index);
                var rowData = row.data();
                console.log(rowData);
                console.log(userRole);
                if (rowData.DepartmentId === userRole.DepartmentId && rowData.RoleId === userRole.RoleId) {
                    needToAdd = false;
                }
                if (rowData.DepartmentId === userRole.DepartmentId && rowData.RoleId !== userRole.RoleId) {
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

            $scope.popupForm['selectedDepartment'].$touched = true;
            $scope.popupForm['selectedRole'].$touched = true;
        }

    }




    $timeout(function () {
        var table = $("#sample_1");
        $('#sample_1 tbody').on('click', 'a.delete', function () {
            var row = table.DataTable().row($(this).parents('tr'));
            var idToRemove = table.DataTable().data()[row.index()].Id;
            bootbox.confirm("Are you sure you want to delete this staff member?", function(result) {
                if (result) {
                    crudService.remove("StaffManagement", idToRemove).then(function() {
                        table.DataTable().row(row).remove().draw();
                        toastr["success"]("Staff was successfully removed!");
                    });
                }
            });
        });

        $('#sample_1 tbody').on('click', 'a.edit', function () {
            indexOfRowInEdit = table.DataTable().row($(this).parents('tr')).index();
            vm.sharedData.selectedStaff.Email = table.DataTable().data()[indexOfRowInEdit].Name;
            vm.sharedData.selectedStaff.FirstName = table.DataTable().data()[indexOfRowInEdit].FirstName;
            vm.sharedData.selectedStaff.LastName = table.DataTable().data()[indexOfRowInEdit].LastName;
            vm.sharedData.selectedStaff.Id = table.DataTable().data()[indexOfRowInEdit].Id;
            vm.sharedData.selectedStaff.IsAdministrator = table.DataTable().data()[indexOfRowInEdit].IsAdministrator;
            console.log(vm.sharedData.selectedStaff);
            $scope.$apply();
            if (!vm.sharedData.selectedStaff.IsAdministrator) {
                staffManagementService.loadDepartmentRolesForStaff().then(function(data) {
                    $("#user_role_table").DataTable().rows.add(data.map(function(val, idx, arr) {
                        return {
                            Name: val.Department.Name,
                            Role: val.DepartmentRole.Name,
                            del: "delete",
                            DepartmentId: val.Department.Id.toString(),
                            RoleId: val.DepartmentRole.Id
                        }
                    })).draw();
                });
            }
        });
        staffManagementService.loadStaff().then(function (data) {
            
            table.DataTable().rows.add(data).draw();
        });

        var userRoleDatable = $("#user_role_table").DataTable();
        $("#user_role_table").on("click", "a.delete", function () {
            userRoleDatable.row(userRoleDatable.row($(this).parents('tr'))).remove().draw();
        });


    });
    vm.popupActionName = "Edit";
    vm.popupIsEditing = true;
    vm.addNewStaff = function () {
        vm.popupActionName = "Add";
        vm.popupIsEditing = false;
    }




    vm.createStaffMember = function () {
        $.blockElement($("#edit-department"));
        validateForm().then(function(data) {

            if (data) {
                createNewStaffMember().then(function() {
                    $.unblockElement($("#edit-department"));
                    vm.popupIsEditing = true;
                    toastr["success"]("Staff was successfully created!");
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
                if (indexOfRowInEdit === -1) { // actually adding row
                    staffManagementService.updateStaffMember().then(function() {
                        $.unblockElement($("#edit-department"));
                        vm.closeModal();
                        toastr["success"]("Staff was successfully updated!");
                    });

                } else {
                    staffManagementService.updateStaffMember().then(function() {
                        $.unblockElement($("#edit-department"));
                        table.data()[indexOfRowInEdit].Name = vm.sharedData.selectedStaff.Email;
                        table.data()[indexOfRowInEdit].FirstName = vm.sharedData.selectedStaff.FirstName;
                        table.data()[indexOfRowInEdit].LastName = vm.sharedData.selectedStaff.LastName;
                        table.data()[indexOfRowInEdit].IsAdministrator = vm.sharedData.selectedStaff.IsAdministrator;
                        table.row(indexOfRowInEdit).invalidate().draw();
                        vm.closeModal();
                        toastr["success"]("Staff was successfully updated!");
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



    vm.adminStatusChanged = function() {

        if (vm.popupIsEditing) {
            if (!vm.sharedData.selectedStaff.IsAdministrator) {
                staffManagementService.loadDepartmentRolesForStaff().then(function (data) {
                    $("#user_role_table").DataTable().clear();
                    $("#user_role_table").DataTable().rows.add(data.map(function (val, idx, arr) {
                        return {
                            Name: val.Department.Name,
                            Role: val.DepartmentRole.Name,
                            del: "delete",
                            DepartmentId: val.Department.Id.toString(),
                            RoleId: val.DepartmentRole.Id
                        }
                    })).draw();
                });
            }
        }
    }



    function closeModal() {
        $("#edit-department").modal("hide");
        indexOfRowInEdit = -1;
        staffManagementService.resetSelectedItem();
        vm.popupActionName = "Edit";
        vm.popupIsEditing = true;
        $("#user_role_table").DataTable().clear().draw();
        $scope.addEditStaffForm.$setUntouched();
        $scope.popupForm.$setUntouched();
        vm.sharedData.selectedRole = vm.sharedData.roles[0];
        vm.sharedData.selectedDepartment = vm.sharedData.departments[0];
    }



    function createNewStaffMember() {
        return $q(function (resolve, reject) {
            staffManagementService.createNewStaffMember().then(function (addedObj) {
                var table = $("#sample_1").DataTable();
                addedObj["edit"] = "edit";
                addedObj["del"] = "delete";
                addedObj["Name"] = addedObj.UserName;
                console.log(addedObj);
                table.row.add(addedObj).draw();
                resolve();
            });
        });
    }



    function validateForm() {
        return $q(function(resolve, reject) {
            networkService.isEmailUnique(vm.sharedData.selectedStaff.Email, vm.sharedData.selectedStaff.Id).then(function(data) {
            

                if ($scope.addEditStaffForm['firstName'].$invalid)
                $scope.addEditStaffForm['firstName'].$touched = true;


            if ($scope.addEditStaffForm['lastName'].$invalid)
                $scope.addEditStaffForm['lastName'].$touched = true;


            if ($scope.addEditStaffForm['email'].$invalid)
                $scope.addEditStaffForm['email'].$touched = true;

            if(!data)
                toastr["error"]("User email is not unique!");

            resolve ((!$scope.addEditStaffForm['firstName'].$invalid) && (!$scope.addEditStaffForm['lastName'].$invalid)
                && (!$scope.addEditStaffForm['email'].$invalid) && data);
            });
        });
    }

    return vm;
});