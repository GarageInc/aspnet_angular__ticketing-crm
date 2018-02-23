issueTypesApp.controller("issueTypeController", function($timeout, $scope, crudService, DropdownDatasource, networkService) {
    var vm = this;

    var issues = [];
    vm.selectedIssue = {Id:0, Name: "", Icon:"" };
    var indexOfRowInEdit = -1;
    vm.issueTypes = [];
    vm.selectedIssueType = {};
    DropdownDatasource.sendRequest(DropdownDatasource.requestBuilder().setPath("/Icons").setNullValue("-Select Icon-").setCacheKey("icons").build()).then(function(data) {
        console.log(data);
        vm.issueTypes = data;
        vm.selectedIssueType.Name = vm.issueTypes[0].Name;
        vm.selectedIssueType.Icon = vm.issueTypes[0].Icon;
        vm.selectedIssueType.Id = vm.issueTypes[0].Id;
    });
    vm.actionName = "Edit";

    vm.sideMenu = [];
    crudService.loadMenu().then(function (data) {
        console.log(data);
        vm.sideMenu = data;
    });

    $timeout(function () {
        var table = $("#sample_1");
        table.DataTable({
           // data: issues,
            columns: [
                { "data": "Id" },
                { "data" : "Icon"},
                { "data": "Name" },
                { "data": "edit" },
                { "data": "del" }],
            columnDefs: [
                { "width": "7%", "type" : "num" ,"targets" : 0 },
                {
                    "width": "5%","targets" : 1,
                    "render" : function(data, type, row) {
                        return '<div style="text-align:center"><i class="fa ' + data + '"></i></div>';
                    }
                },
                { "width": "50%", "type" : "string","targets": 2 },
                {
                    "width": "19%",
                    "targets": 3,
                    "render" : function(data, type, row) {
                        return '<div style="text-align:center"><a style="cursor: pointer;" class="edit"  data-toggle="modal" href="#edit-issue">' + data + '</a></div>';
                    }
                },
                {
                    "width": "19%", "targets": 4,
                    "render": function (data, type, row) {
                        return '<div style="text-align:center"><a style="cursor: pointer;" class="delete">' + data + '</a></div>';
                    }
                }
            ]
        });
        $('#sample_1 tbody').on('click', 'a.delete', function () {
            var row = table.DataTable().row($(this).parents('tr'));
            var idToRemove = table.DataTable().data()[row.index()].Id;
            bootbox.confirm("Are you sure you want to delete this issue type?", function (result) {
                if (result) {              
                    crudService.remove("IssueType", idToRemove).then(function () {
                        table.DataTable().row(row).remove().draw();
                        toastr["success"]("Issue type was successfully deleted!");
                    });
                }
            });
        });

        $('#sample_1 tbody').on('click', 'a.edit', function () {
            indexOfRowInEdit = table.DataTable().row($(this).parents('tr')).index();
            vm.selectedIssue.Name = table.DataTable().data()[indexOfRowInEdit].Name;
            vm.selectedIssue.Id = table.DataTable().data()[indexOfRowInEdit].Id;

            var index = resolveIndexByIconClass(table.DataTable().data()[indexOfRowInEdit].Icon);
            vm.selectedIssueType.Name = vm.issueTypes[index].Name;
            vm.selectedIssueType.Icon = vm.issueTypes[index].Icon;
            vm.selectedIssueType.Id = vm.issueTypes[index].Id;
            $scope.$apply();
        });
        //load issues
        crudService.read("IssueType").then(function (data) {
            console.log(data);
            table.DataTable().clear().draw();
            for (var i = 0; i < data.length; i++) {
                issues.push({ Id: data[i].Id, Icon: data[i].Icon.Icon, Name: data[i].Name , edit: "edit", del: "delete" });
            }
            table.DataTable().rows.add(issues).draw();
        });     
    });

    vm.addNewIssueType = function () {
        vm.actionName = "Add";
    }

    vm.saveChanges = function () {      
        if (validateForm()) {
            $.blockElement($("#edit-issue"));
            networkService.isIssueTypeNameUnique(vm.selectedIssue.Name, vm.selectedIssue.Id).then(function (data) {
                if (data) {
                    var table = $("#sample_1").DataTable();
                    if (indexOfRowInEdit === -1) { // actually adding row
                        var newIssue = { Name: vm.selectedIssue.Name, Icon: { Id: vm.selectedIssueType.Id, Icon: vm.selectedIssueType.Icon, Name: vm.selectedIssueType.Name } };
                        crudService.create("IssueType", newIssue).then(function(addedObj) {

                            table.row.add({ Id: addedObj.Id, Icon: addedObj.Icon.Icon, Name: addedObj.Name, edit: "edit", del: "delete" }).draw();
                            closeModal();
                            toastr["success"]("Issue type was successfully added!");
                        }).then(function() {
                            $.unblockElement($("#edit-issue"));
                        });

                    } else {

                        var updateIssue = { Id: vm.selectedIssue.Id, Name: vm.selectedIssue.Name, Icon: { Id: vm.selectedIssueType.Id, Icon: vm.selectedIssueType.Icon, Name: vm.selectedIssueType.Name } };
                        crudService.update("IssueType", updateIssue).then(function(updatedObj) {
                            //console.log(updatedObj);
                            $.unblockElement($("#edit-issue"));
                            table.data()[indexOfRowInEdit].Name = updateIssue.Name;
                            table.data()[indexOfRowInEdit].Icon = updateIssue.Icon.Icon;
                            table.row(indexOfRowInEdit).invalidate().draw();
                            closeModal();
                            toastr["success"]("Issue type was successfully updated!");
                        });
                    }
                } else {
                    $.unblockElement($("#edit-issue"));
                    toastr["error"]("Issue type name is not unique!");
                }
            });
        }
    }
    vm.closeModal = function () {
        closeModal();
    }



    function validateForm() {
        vm.selectedIssue.Name = (typeof vm.selectedIssue.Name === 'undefined' ? "" : vm.selectedIssue.Name);
        if (vm.selectedIssue.Name === "" ||
            vm.selectedIssueType.Name === "-Select Icon-") {
            if (vm.selectedIssueType.Name === "-Select Icon-") {
                setDropdownInvalid();
            }
            if (vm.selectedIssue.Name === "") {
                $scope.addEditForm['issueName'].$touched = true;
            }
            return false;
        }
        return true;
    }



    function closeModal() {
        indexOfRowInEdit = -1;
        vm.selectedIssueType.Name = vm.issueTypes[0].Name;
        vm.selectedIssueType.Icon = vm.issueTypes[0].Icon;
        vm.selectedIssueType.Id = vm.issueTypes[0].Id;
        vm.selectedIssue.Name = "";
        vm.selectedIssue.Id = 0;
        $scope.addEditForm['issueName'].$touched = false;
        vm.actionName = "Edit";
        setDropdownValid();
        $("#edit-issue").modal("hide");
    }

    function resolveIndexByIconClass(iconClass) {
        for (var i = 0; i < vm.issueTypes.length; i++) {
            if (vm.issueTypes[i].Icon === iconClass) {
                return i;
            }
        }
        return -1;
    }
    /*##################################Customized select###########################*/
    var isShowing = false;
    vm.ddFaceClicked = function() {
        if (!isShowing) {
            setDropdownValid();
            $("#aha-options-list").css("display", "block");
            $("#aha-options-list").css({
                'width': ($("#issue-type").innerWidth() + 'px')
            });
            isShowing = true;
        } else {
            isShowing = false;
            $("#aha-options-list").css("display", "none");
        }
    }
    vm.ddSelected = function(idx) {
        if (isShowing) {
            $("#aha-options-list").css("display", "none");
            isShowing = false;
            vm.selectedIssueType.Name = vm.issueTypes[idx].Name;
            vm.selectedIssueType.Icon = vm.issueTypes[idx].Icon;
            vm.selectedIssueType.Id = vm.issueTypes[idx].Id;
        }
    }

    function setDropdownInvalid() {
        $("#issue-type").addClass("aha-invalid");
    }

    function setDropdownValid() {
        $("#issue-type").removeClass("aha-invalid");
    }

    return vm;
});